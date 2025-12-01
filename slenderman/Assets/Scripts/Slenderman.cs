using System.Collections;
using UnityEngine;

public class Slenderman : MonoBehaviour
{
    [Header("Target & Movement")]
    [SerializeField] Transform target;
    [SerializeField] float walkSpeed = 2f;
    [SerializeField] float runSpeed = 5f;
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] float detectionRange = 20f;
    [SerializeField] float runRange = 5f;

    [Header("Footsteps")]
    [SerializeField] float footstepIntervalWalk = 0.6f;
    [SerializeField] float footstepIntervalRun = 0.35f;
    [Header("Teleport")]
    [SerializeField] bool enableTeleport = true;
    [Tooltip("If the Slenderman is farther than this distance from the target, he will teleport closer")]
    [SerializeField] float teleportDistance = 60f;
    [Tooltip("Distance from the target (on XZ) after teleporting")]
    [SerializeField] float teleportOffset = 2f;
    [SerializeField] bool teleportSnapToGround = true;
    [Tooltip("Seconds to wait before teleporting once out of range")]
    [SerializeField] float teleportDelay = 3f;
    Coroutine teleportCoroutine = null;

    private Animator animator;
    private CharacterController characterController;
    private AudioSource audioSource;
    private Coroutine footstepCoroutine;

    enum State { Idle, Walk, Run }
    State currentState = State.Idle;

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.enabled = true;
            // Force animator to always animate while debugging so culling doesn't stop the animation
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            // Ensure base layer weight
            if (animator.layerCount > 0) animator.SetLayerWeight(0, 1f);
            Debug.Log("[Slenderman] Animator found: " + animator + " Controller: " + animator.runtimeAnimatorController);
            DumpAnimatorInfo();
        }
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
    }

    void DumpAnimatorInfo()
    {
        if (animator == null) return;
        var ac = animator.runtimeAnimatorController;
        if (ac == null)
        {
            Debug.LogWarning("[Slenderman] Animator has no Controller assigned.");
            return;
        }

        Debug.Log("[Slenderman] Controller: " + ac.name + " Layers: " + animator.layerCount);
        for (int i = 0; i < animator.layerCount; i++)
        {
            Debug.Log($"[Slenderman] Layer {i} name: {animator.GetLayerName(i)} weight: {animator.GetLayerWeight(i)}");
        }

        // List states in the controller (best-effort): show current state name info
        var state = animator.GetCurrentAnimatorStateInfo(0);
        Debug.Log($"[Slenderman] Current State (layer 0): shortNameHash={state.shortNameHash}, normalizedTime={state.normalizedTime}, isTag= {state.IsTag("Idle")} ");
    }

    void Update()
    {
        if (target == null)
        {
            SetState(State.Idle);
            return;
        }

        Vector3 toTarget = target.position - transform.position;
        float distance = toTarget.magnitude;

        // Teleport handling: start a delayed teleport if too far, cancel if returns in range
        if (enableTeleport)
        {
            if (distance > teleportDistance)
            {
                if (teleportCoroutine == null)
                {
                    teleportCoroutine = StartCoroutine(TeleportDelayed());
                    Debug.Log($"[Slenderman] Target out of range ({distance:F1}). Starting teleport timer ({teleportDelay}s).");
                }
            }
            else
            {
                if (teleportCoroutine != null)
                {
                    StopCoroutine(teleportCoroutine);
                    teleportCoroutine = null;
                    Debug.Log("[Slenderman] Teleport cancelled: target back in range.");
                }
            }
        }

        if (distance > detectionRange)
        {
            SetState(State.Idle);
            return;
        }

        if (distance > runRange)
            SetState(State.Walk);
        else
            SetState(State.Run);

        if (currentState != State.Idle)
        {
            Vector3 dir = toTarget;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
            {
                Vector3 dirNorm = dir.normalized;
                Quaternion targetRot = Quaternion.LookRotation(dirNorm);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

                float speed = currentState == State.Run ? runSpeed : walkSpeed;
                if (characterController != null)
                {
                    characterController.SimpleMove(transform.forward * speed);
                }
                else
                {
                    transform.position += transform.forward * speed * Time.deltaTime;
                }
            }
        }
    }

    void SetState(State newState)
    {
        if (newState == currentState) return;
        currentState = newState;

        if (animator != null)
        {
            // Use CrossFade with the layer path for smoother transitions. Fallback to Play.
            try
            {
                switch (currentState)
                {
                    case State.Idle:
                        animator.CrossFade("Base Layer.Idle", 0.12f);
                        break;
                    case State.Walk:
                        animator.CrossFade("Base Layer.Walk", 0.12f);
                        break;
                    case State.Run:
                        animator.CrossFade("Base Layer.Run", 0.12f);
                        break;
                }
            }
            catch
            {
                // If CrossFade fails (non-standard layer name), fallback to Play by state name.
                switch (currentState)
                {
                    case State.Idle:
                        animator.Play("Idle");
                        break;
                    case State.Walk:
                        animator.Play("Walk");
                        break;
                    case State.Run:
                        animator.Play("Run");
                        break;
                }
            }
        }

        if (currentState == State.Walk || currentState == State.Run)
        {
            float interval = currentState == State.Run ? footstepIntervalRun : footstepIntervalWalk;
            if (footstepCoroutine != null) StopCoroutine(footstepCoroutine);
            footstepCoroutine = StartCoroutine(Footsteps(interval));
        }
        else
        {
            if (footstepCoroutine != null)
            {
                StopCoroutine(footstepCoroutine);
                footstepCoroutine = null;
            }
        }
    }

    IEnumerator Footsteps(float interval)
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            if (audioSource != null) audioSource.Play();
        }
    }

    void TeleportNearTarget()
    {
        if (target == null) return;

        // place Slenderman at a point `teleportOffset` units from the target on the XZ plane
        Vector3 dir = (transform.position - target.position).normalized;
        if (dir.sqrMagnitude < 0.001f) dir = Vector3.back; // fallback direction

        Vector3 newPos = target.position + new Vector3(dir.x, 0f, dir.z) * Mathf.Max(teleportOffset, 0.1f);

        // optionally snap to ground
        if (teleportSnapToGround)
        {
            RaycastHit hit;
            Vector3 rayOrigin = newPos + Vector3.up * 5f;
            if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 20f))
            {
                newPos.y = hit.point.y;
            }
        }

        // move using CharacterController safely if present
        if (characterController != null)
        {
            bool wasEnabled = characterController.enabled;
            characterController.enabled = false;
            transform.position = newPos;
            characterController.enabled = wasEnabled;
        }
        else
        {
            transform.position = newPos;
        }

        Debug.Log($"[Slenderman] Teleported near target to {newPos} (distance before: {Vector3.Distance(transform.position, target.position):F2})");
    }

    IEnumerator TeleportDelayed()
    {
        float start = Time.time;
        while (Time.time - start < teleportDelay)
        {
            // if target moved back in range, abort
            if (target == null) break;
            float d = (target.position - transform.position).magnitude;
            if (d <= teleportDistance)
            {
                Debug.Log("[Slenderman] Teleport aborted: target returned within range during delay.");
                teleportCoroutine = null;
                yield break;
            }
            yield return null;
        }

        // Final check before teleporting
        if (target != null && (target.position - transform.position).magnitude > teleportDistance)
        {
            TeleportNearTarget();
        }
        teleportCoroutine = null;
    }
}
