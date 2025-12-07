using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Slenderman : MonoBehaviour
{
    [Header("Target & Movement")]
    [SerializeField] Transform target;
    [SerializeField] float walkSpeed = 2f;
    [SerializeField] float runSpeed = 5f;
    [SerializeField] float rotationSpeed = 5f;
    [SerializeField] float detectionRange = 30f;
    [SerializeField] float runRange = 5f;
    [SerializeField] float acceleration = 2f;

    [Header("Footsteps")]
    [SerializeField] AudioClip[] footstepSounds;
    [SerializeField] float footstepIntervalWalk = 0.6f;
    [SerializeField] float footstepIntervalRun = 0.35f;
    [SerializeField] [Range(0f, 1f)] float footstepVolume = 0.5f;
    
    [Header("Teleport")]
    [SerializeField] bool enableTeleport = true;
    [SerializeField] float teleportDistance = 60f;
    [SerializeField] float teleportOffset = 2f;
    [SerializeField] bool teleportSnapToGround = true;
    [SerializeField] float teleportDelay = 3f;
    [SerializeField] AudioClip teleportSound;
    [SerializeField] ParticleSystem teleportEffect;
    
    [Header("Derrota")]
    [SerializeField] string escenaVideoDerrota = "DefeatScene";
    [SerializeField] float retrasoDerrota = 0.5f;
    [SerializeField] GameObject efectoDerrota;
    [SerializeField] AudioClip sonidoDerrota;
    [SerializeField] float attackDistance = 3f;
    
    [Header("Modo Loco")]
    [SerializeField] [Range(0f, 100f)] float probabilidadModoLoco = 30f;
    [SerializeField] float duracionModoLoco = 5f;
    [SerializeField] float multiplicadorVelocidadLoco = 2f;
    [SerializeField] float multiplicadorRangoLoco = 2f;
    [SerializeField] AudioClip sonidoModoLoco;
    [SerializeField] ParticleSystem efectoModoLoco;
    [SerializeField] float checkIntervalModoLoco = 10f;
    
    [Header("Vision System")]
    [SerializeField] bool freezeWhenLookedAt = true;
    [SerializeField] float freezeAngleThreshold = 45f;
    [SerializeField] float freezeCheckInterval = 0.2f;
    [SerializeField] AudioClip staticSound;
    
    [Header("Comportamiento Avanzado")]
    [SerializeField] bool enableSmartPatrol = true;
    [SerializeField] float patrolRadius = 20f;
    [SerializeField] float minPatrolTime = 3f;
    [SerializeField] float maxPatrolTime = 8f;
    [SerializeField] LayerMask obstacleLayer;
    
    [Header("Efectos de Miedo")]
    [SerializeField] bool enableFearSystem = true;
    [SerializeField] float fearBuildupRate = 10f;
    [SerializeField] AudioClip breathingSound;
    [SerializeField] AudioClip heartbeatSound;
    
    private Animator animator;
    private CharacterController characterController;
    private AudioSource audioSource;
    private AudioSource staticAudioSource;
    private Coroutine footstepCoroutine;
    private Coroutine teleportCoroutine;
    private Coroutine modoLocoCoroutine;
    private Coroutine patrolCoroutine;
    private bool derrotaActivada;
    private bool modoLocoActivo;
    private bool isFrozen;
    private float currentSpeed;
    private Vector3 patrolTarget;
    
    // Valores originales
    private float velocidadWalkOriginal;
    private float velocidadRunOriginal;
    private float rangoRunOriginal;
    private float velocidadRotacionOriginal;

    enum State { Idle, Walk, Run, Patrol, Frozen }
    State currentState = State.Idle;

    void Awake()
    {
        InitializeComponents();
        SaveOriginalValues();
    }

    void InitializeComponents()
    {
        animator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.enabled = true;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            if (animator.layerCount > 0) animator.SetLayerWeight(0, 1f);
        }
        
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        
        // Audio source adicional para efectos estáticos
        if (staticSound != null)
        {
            GameObject staticObj = new GameObject("StaticAudioSource");
            staticObj.transform.SetParent(transform);
            staticAudioSource = staticObj.AddComponent<AudioSource>();
            staticAudioSource.clip = staticSound;
            staticAudioSource.loop = true;
            staticAudioSource.volume = 0f;
            staticAudioSource.spatialBlend = 1f;
            staticAudioSource.maxDistance = 50f;
        }
    }

    void SaveOriginalValues()
    {
        velocidadWalkOriginal = walkSpeed;
        velocidadRunOriginal = runSpeed;
        rangoRunOriginal = runRange;
        velocidadRotacionOriginal = rotationSpeed;
    }

    void Start()
    {
        InvokeRepeating(nameof(ChequearModoLoco), 5f, checkIntervalModoLoco);
        
        if (freezeWhenLookedAt)
            InvokeRepeating(nameof(CheckIfPlayerLooking), 0.5f, freezeCheckInterval);
        
        if (enableSmartPatrol)
            StartPatrolling();
    }

    void Update()
    {
        if (derrotaActivada) return;
        
        if (isFrozen)
        {
            HandleFrozenState();
            return;
        }
        
        if (target == null)
        {
            HandlePatrolMode();
            return;
        }

        float distance = Vector3.Distance(transform.position, target.position);
        
        // Sistema de ataque mejorado
        if (distance <= attackDistance)
        {
            HandleAttack();
            return;
        }
        
        // Sistema de teletransporte mejorado
        HandleTeleportation(distance);
        
        // Lógica de movimiento principal
        HandleMovement(distance);
        
        // Sistema de miedo
        if (enableFearSystem)
            UpdateFearSystem(distance);
    }

    void HandleFrozenState()
    {
        if (currentSpeed > 0f)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, Time.deltaTime * 5f);
            
            if (characterController != null && characterController.enabled)
                characterController.SimpleMove(transform.forward * currentSpeed);
        }
        
        UpdateStaticSound(1f);
    }

    void HandleAttack()
    {
        if (modoLocoActivo)
            EjecutarAtaqueModoLoco();
        else
            StartCoroutine(ProcesarDerrota());
    }

    void HandleTeleportation(float distance)
    {
        if (!enableTeleport) return;
        
        if (distance > teleportDistance)
        {
            if (teleportCoroutine == null)
                teleportCoroutine = StartCoroutine(TeleportDelayed());
        }
        else
        {
            if (teleportCoroutine != null)
            {
                StopCoroutine(teleportCoroutine);
                teleportCoroutine = null;
            }
        }
    }

    void HandleMovement(float distance)
    {
        if (distance > detectionRange)
        {
            HandlePatrolMode();
            return;
        }

        // Decidir estado según distancia
        State targetState = distance > runRange ? State.Walk : State.Run;
        SetState(targetState);

        if (currentState == State.Idle) return;

        // Calcular dirección y rotar
        Vector3 dirToTarget = (target.position - transform.position);
        dirToTarget.y = 0f;
        
        if (dirToTarget.sqrMagnitude > 0.001f)
        {
            RotateTowards(dirToTarget.normalized);
            MoveForward();
        }
        
        UpdateStaticSound(Mathf.InverseLerp(detectionRange, attackDistance, distance));
    }

    void HandlePatrolMode()
    {
        if (!enableSmartPatrol)
        {
            SetState(State.Idle);
            return;
        }
        
        if (patrolCoroutine == null)
            StartPatrolling();
    }

    void StartPatrolling()
    {
        if (patrolCoroutine != null)
            StopCoroutine(patrolCoroutine);
        
        patrolCoroutine = StartCoroutine(PatrolRoutine());
    }

    IEnumerator PatrolRoutine()
    {
        while (true)
        {
            if (target != null)
            {
                float dist = Vector3.Distance(transform.position, target.position);
                if (dist <= detectionRange)
                {
                    patrolCoroutine = null;
                    yield break;
                }
            }
            
            // Generar punto de patrulla aleatorio
            Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
            patrolTarget = transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);
            
            // Ajustar al suelo
            if (Physics.Raycast(patrolTarget + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 20f))
                patrolTarget.y = hit.point.y;
            
            SetState(State.Patrol);
            
            // Moverse hacia el punto de patrulla
            float patrolTime = Random.Range(minPatrolTime, maxPatrolTime);
            float elapsed = 0f;
            
            while (elapsed < patrolTime)
            {
                if (target != null && Vector3.Distance(transform.position, target.position) <= detectionRange)
                {
                    patrolCoroutine = null;
                    yield break;
                }
                
                Vector3 dirToPatrol = (patrolTarget - transform.position);
                dirToPatrol.y = 0f;
                
                if (dirToPatrol.magnitude > 1f)
                {
                    RotateTowards(dirToPatrol.normalized);
                    MoveForward();
                }
                else
                {
                    break; // Llegó al punto de patrulla
                }
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // Esperar un momento antes de elegir nuevo punto
            SetState(State.Idle);
            yield return new WaitForSeconds(Random.Range(2f, 5f));
        }
    }

    void RotateTowards(Vector3 direction)
    {
        Quaternion targetRot = Quaternion.LookRotation(direction);
        float rotSpeed = modoLocoActivo ? rotationSpeed * 1.5f : rotationSpeed;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotSpeed * Time.deltaTime);
    }

    void MoveForward()
    {
        float targetSpeed = currentState == State.Run ? runSpeed : 
                           currentState == State.Patrol ? walkSpeed * 0.7f : walkSpeed;
        
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
        
        if (characterController != null && characterController.enabled)
            characterController.SimpleMove(transform.forward * currentSpeed);
        else
            transform.position += transform.forward * currentSpeed * Time.deltaTime;
    }

    void CheckIfPlayerLooking()
    {
        if (target == null || derrotaActivada || modoLocoActivo) return;
        
        Camera mainCam = Camera.main;
        if (mainCam == null) return;
        
        Vector3 dirToSlender = (transform.position - mainCam.transform.position).normalized;
        float angle = Vector3.Angle(mainCam.transform.forward, dirToSlender);
        
        bool wasLooking = isFrozen;
        isFrozen = angle < freezeAngleThreshold;
        
        if (isFrozen != wasLooking)
        {
            if (isFrozen)
                SetState(State.Frozen);
        }
    }

    void UpdateStaticSound(float intensity)
    {
        if (staticAudioSource == null) return;
        
        float targetVolume = isFrozen ? intensity * 0.7f : 0f;
        staticAudioSource.volume = Mathf.Lerp(staticAudioSource.volume, targetVolume, Time.deltaTime * 3f);
        
        if (!staticAudioSource.isPlaying && targetVolume > 0.01f)
            staticAudioSource.Play();
        else if (staticAudioSource.isPlaying && targetVolume < 0.01f)
            staticAudioSource.Stop();
    }

    void UpdateFearSystem(float distance)
    {
        // Esta función puede conectarse con un sistema de UI para mostrar el nivel de miedo
        float fearLevel = Mathf.InverseLerp(detectionRange, attackDistance, distance);
        
        // Aquí podrías llamar a un gestor de UI para actualizar efectos visuales
        // Por ejemplo: FearUIManager.Instance?.SetFearLevel(fearLevel);
    }

    void ChequearModoLoco()
    {
        if (derrotaActivada || modoLocoActivo || target == null) return;
        
        float randomValue = Random.Range(0f, 100f);
        
        if (randomValue <= probabilidadModoLoco)
            ActivarModoLoco();
    }

    void ActivarModoLoco()
    {
        if (modoLocoActivo) return;
        
        modoLocoActivo = true;
        isFrozen = false; // El modo loco ignora el freeze
        
        Debug.Log("[Slenderman] ¡MODO LOCO ACTIVADO!");
        
        // Aplicar modificadores
        walkSpeed = velocidadWalkOriginal * multiplicadorVelocidadLoco;
        runSpeed = velocidadRunOriginal * multiplicadorVelocidadLoco;
        runRange = rangoRunOriginal * multiplicadorRangoLoco;
        rotationSpeed = velocidadRotacionOriginal * 1.5f;
        
        // Efectos
        PlaySoundEffect(sonidoModoLoco);
        PlayParticleEffect(efectoModoLoco);
        
        // Forzar estado Run
        SetState(State.Run);
        
        if (modoLocoCoroutine != null)
            StopCoroutine(modoLocoCoroutine);
            
        modoLocoCoroutine = StartCoroutine(DesactivarModoLocoCoroutine());
    }

    IEnumerator DesactivarModoLocoCoroutine()
    {
        yield return new WaitForSeconds(duracionModoLoco);
        DesactivarModoLoco();
    }

    void DesactivarModoLoco()
    {
        if (!modoLocoActivo) return;
        
        modoLocoActivo = false;
        Debug.Log("[Slenderman] Modo loco desactivado");
        
        // Restaurar valores
        walkSpeed = velocidadWalkOriginal;
        runSpeed = velocidadRunOriginal;
        runRange = rangoRunOriginal;
        rotationSpeed = velocidadRotacionOriginal;
        
        StopParticleEffect(efectoModoLoco);
        
        modoLocoCoroutine = null;
    }

    void EjecutarAtaqueModoLoco()
    {
        if (derrotaActivada) return;
        
        Debug.Log("[Slenderman] ¡ATAQUE EN MODO LOCO!");
        
        SetState(State.Idle);
        PlayAnimation("Attack", "Run");
        
        DesactivarModoLoco();
        StartCoroutine(DelayedDefeat(0.3f));
    }

    IEnumerator DelayedDefeat(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(ProcesarDerrota());
    }

    IEnumerator ProcesarDerrota()
    {
        if (derrotaActivada) yield break;
        
        derrotaActivada = true;
        Debug.Log("[Slenderman] ¡Jugador atrapado!");
        
        // Limpieza
        CleanupCoroutines();
        DesactivarModoLoco();
        CancelInvoke();
        
        SetState(State.Idle);
        enabled = false;
        
        // Animación y efectos
        PlayAnimation("Attack", "Run");
        PlaySoundEffect(sonidoDerrota, true);
        ActivateGameObject(efectoDerrota);
        
        yield return new WaitForSeconds(retrasoDerrota);
        
        Debug.Log($"[Slenderman] Cargando escena: {escenaVideoDerrota}");
        SceneManager.LoadScene(escenaVideoDerrota);
    }

    void CleanupCoroutines()
    {
        if (footstepCoroutine != null)
        {
            StopCoroutine(footstepCoroutine);
            footstepCoroutine = null;
        }
        
        if (teleportCoroutine != null)
        {
            StopCoroutine(teleportCoroutine);
            teleportCoroutine = null;
        }
        
        if (patrolCoroutine != null)
        {
            StopCoroutine(patrolCoroutine);
            patrolCoroutine = null;
        }
    }

    void SetState(State newState)
    {
        if (newState == currentState) return;
        currentState = newState;

        string animName = currentState switch
        {
            State.Idle => "Idle",
            State.Walk => "Walk",
            State.Run => "Run",
            State.Patrol => "Walk",
            State.Frozen => "Idle",
            _ => "Idle"
        };
        
        PlayAnimation(animName);

        // Gestionar footsteps
        if (currentState == State.Walk || currentState == State.Run || currentState == State.Patrol)
        {
            float interval = currentState == State.Run ? footstepIntervalRun : footstepIntervalWalk;
            if (modoLocoActivo) interval *= 0.5f;
            
            if (footstepCoroutine != null) StopCoroutine(footstepCoroutine);
            footstepCoroutine = StartCoroutine(FootstepsCoroutine(interval));
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

    void PlayAnimation(string animName, string fallback = null)
    {
        if (animator == null) return;
        
        try
        {
            animator.CrossFade($"Base Layer.{animName}", 0.12f);
        }
        catch
        {
            try
            {
                animator.Play(animName);
            }
            catch
            {
                if (fallback != null)
                {
                    try { animator.Play(fallback); }
                    catch { /* Ignore */ }
                }
            }
        }
    }

    IEnumerator FootstepsCoroutine(float interval)
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            
            if (derrotaActivada) break;
            
            if (footstepSounds != null && footstepSounds.Length > 0)
            {
                AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
                if (audioSource != null && clip != null)
                    audioSource.PlayOneShot(clip, footstepVolume);
            }
            else if (audioSource != null)
            {
                audioSource.Play();
            }
        }
    }

    void TeleportNearTarget()
    {
        if (target == null || derrotaActivada) return;

        Vector3 dir = (transform.position - target.position).normalized;
        if (dir.sqrMagnitude < 0.001f) dir = Vector3.back;

        Vector3 newPos = target.position + new Vector3(dir.x, 0f, dir.z) * Mathf.Max(teleportOffset, 0.1f);

        if (teleportSnapToGround)
        {
            Vector3 rayOrigin = newPos + Vector3.up * 5f;
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 20f))
                newPos.y = hit.point.y;
        }

        // Efectos de teletransporte
        PlayParticleEffect(teleportEffect);
        PlaySoundEffect(teleportSound);

        if (characterController != null)
        {
            characterController.enabled = false;
            transform.position = newPos;
            characterController.enabled = true;
        }
        else
        {
            transform.position = newPos;
        }
        
        Debug.Log("[Slenderman] Teletransportado cerca del jugador");
    }

    IEnumerator TeleportDelayed()
    {
        float start = Time.time;
        
        while (Time.time - start < teleportDelay)
        {
            if (target == null || derrotaActivada)
            {
                teleportCoroutine = null;
                yield break;
            }
            
            float d = Vector3.Distance(target.position, transform.position);
            if (d <= teleportDistance)
            {
                teleportCoroutine = null;
                yield break;
            }
            
            yield return null;
        }

        if (target != null && !derrotaActivada)
        {
            float finalDist = Vector3.Distance(target.position, transform.position);
            if (finalDist > teleportDistance)
                TeleportNearTarget();
        }
        
        teleportCoroutine = null;
    }

    // Métodos auxiliares
    void PlaySoundEffect(AudioClip clip, bool stopPrevious = false)
    {
        if (audioSource == null || clip == null) return;
        
        if (stopPrevious)
            audioSource.Stop();
        
        audioSource.PlayOneShot(clip);
    }

    void PlayParticleEffect(ParticleSystem effect)
    {
        if (effect != null && !effect.isPlaying)
            effect.Play();
    }

    void StopParticleEffect(ParticleSystem effect)
    {
        if (effect != null && effect.isPlaying)
            effect.Stop();
    }

    void ActivateGameObject(GameObject obj)
    {
        if (obj != null)
            obj.SetActive(true);
    }

    // Método público
    public void ForzarModoLoco(float duracionExtra = 0f)
    {
        ActivarModoLoco();
        
        if (duracionExtra > 0 && modoLocoCoroutine != null)
        {
            StopCoroutine(modoLocoCoroutine);
            modoLocoCoroutine = StartCoroutine(DesactivarModoLocoConExtra(duracionExtra));
        }
    }

    IEnumerator DesactivarModoLocoConExtra(float extra)
    {
        yield return new WaitForSeconds(duracionModoLoco + extra);
        DesactivarModoLoco();
    }

    // Gizmos mejorados
    void OnDrawGizmosSelected()
    {
        // Radio de detección
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Radio run
        Gizmos.color = modoLocoActivo ? new Color(1f, 0f, 1f, 0.5f) : new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, runRange);
        
        // Radio de ataque
        Gizmos.color = new Color(0f, 1f, 1f, 0.7f);
        Gizmos.DrawWireSphere(transform.position, attackDistance);
        
        // Indicador modo loco
        if (modoLocoActivo)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawSphere(transform.position + Vector3.up * 2f, 1f);
        }
        
        // Radio de patrulla
        if (enableSmartPatrol)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, patrolRadius);
        }
        
        // Punto de patrulla actual
        if (currentState == State.Patrol)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(patrolTarget, 0.5f);
            Gizmos.DrawLine(transform.position, patrolTarget);
        }
    }
}