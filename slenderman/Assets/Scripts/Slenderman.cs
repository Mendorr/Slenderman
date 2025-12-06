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
    
    [Header("Derrota")]
    [SerializeField] string escenaVideoDerrota = "DefeatScene";
    [SerializeField] float retrasoDerrota = 0.5f;
    [SerializeField] GameObject efectoDerrota;
    [SerializeField] AudioClip sonidoDerrota;
    
    [Header("Modo Loco")]
    [SerializeField] float probabilidadModoLoco = 30f; // 30% de probabilidad
    [SerializeField] float duracionModoLoco = 5f; // Duración en segundos
    [SerializeField] float multiplicadorVelocidadLoco = 2f; // 2x más rápido
    [SerializeField] float multiplicadorRangoLoco = 2f; // 2x más rango
    [SerializeField] AudioClip sonidoModoLoco; // Sonido cuando entra en modo loco
    [SerializeField] ParticleSystem efectoModoLoco; // Efectos visuales (opcional)
    
    private Animator animator;
    private CharacterController characterController;
    private AudioSource audioSource;
    private Coroutine footstepCoroutine;
    private Coroutine teleportCoroutine = null;
    private Coroutine modoLocoCoroutine = null;
    private bool derrotaActivada = false;
    private bool modoLocoActivo = false;
    
    // Valores originales para restaurar después del modo loco
    private float velocidadWalkOriginal;
    private float velocidadRunOriginal;
    private float rangoRunOriginal;
    private float velocidadRotacionOriginal;

    enum State { Idle, Walk, Run }
    State currentState = State.Idle;

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.enabled = true;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            if (animator.layerCount > 0) animator.SetLayerWeight(0, 1f);
        }
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        
        // Guardar valores originales
        velocidadWalkOriginal = walkSpeed;
        velocidadRunOriginal = runSpeed;
        rangoRunOriginal = runRange;
        velocidadRotacionOriginal = rotationSpeed;
    }

    void Start()
    {
        // Iniciar el chequeo aleatorio del modo loco
        InvokeRepeating("ChequearModoLoco", 5f, 10f); // Cada 10 segundos, empezando a los 5
    }

    void Update()
    {
        if (derrotaActivada) return;
        
        if (target == null)
        {
            SetState(State.Idle);
            return;
        }

        Vector3 toTarget = target.position - transform.position;
        float distance = toTarget.magnitude;
        
        // Si está en modo loco y cerca del jugador, atacar
        if (modoLocoActivo && distance <= 3f)
        {
            EjecutarAtaqueModoLoco();
            return;
        }
        
        // Derrota normal (sin modo loco)
        if (!modoLocoActivo && distance <= 3f)
        {
            StartCoroutine(ProcesarDerrota());
            return;
        }

        // Lógica de teletransporte
        if (enableTeleport)
        {
            if (distance > teleportDistance)
            {
                if (teleportCoroutine == null)
                {
                    teleportCoroutine = StartCoroutine(TeleportDelayed());
                }
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

        if (distance > detectionRange)
        {
            SetState(State.Idle);
            return;
        }

        // Decidir estado según distancia
        if (distance > runRange)
            SetState(State.Walk);
        else
            SetState(State.Run);

        // Movimiento
        if (currentState != State.Idle)
        {
            Vector3 dir = toTarget;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
            {
                Vector3 dirNorm = dir.normalized;
                Quaternion targetRot = Quaternion.LookRotation(dirNorm);
                
                // En modo loco, rotación más rápida
                float rotSpeedActual = modoLocoActivo ? rotationSpeed * 1.5f : rotationSpeed;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotSpeedActual * Time.deltaTime);

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

    // Método para chequear si activar modo loco
    void ChequearModoLoco()
    {
        if (derrotaActivada || modoLocoActivo || target == null) return;
        
        // Calcular probabilidad (30% por defecto)
        float randomValue = Random.Range(0f, 100f);
        
        if (randomValue <= probabilidadModoLoco)
        {
            ActivarModoLoco();
        }
    }

    void ActivarModoLoco()
    {
        if (modoLocoActivo) return;
        
        modoLocoActivo = true;
        Debug.Log("[Slenderman] ¡MODO LOCO ACTIVADO!");
        
        // Guardar valores actuales por si ya estaba en modo loco anteriormente
        velocidadWalkOriginal = walkSpeed;
        velocidadRunOriginal = runSpeed;
        rangoRunOriginal = runRange;
        
        // Aplicar modificadores
        walkSpeed *= multiplicadorVelocidadLoco;
        runSpeed *= multiplicadorVelocidadLoco;
        runRange *= multiplicadorRangoLoco;
        rotationSpeed *= 1.5f; // Rotación más rápida
        
        // Efectos de sonido
        if (sonidoModoLoco != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoModoLoco);
        }
        
        // Efectos visuales
        if (efectoModoLoco != null)
        {
            efectoModoLoco.Play();
        }
        
        // Cambiar animación a Run (más agresiva)
        if (animator != null)
        {
            try
            {
                animator.CrossFade("Base Layer.Run", 0.1f);
            }
            catch
            {
                animator.Play("Run");
            }
        }
        
        // Programar desactivación del modo loco
        if (modoLocoCoroutine != null)
            StopCoroutine(modoLocoCoroutine);
            
        modoLocoCoroutine = StartCoroutine(DesactivarModoLoco());
    }

    IEnumerator DesactivarModoLoco()
    {
        yield return new WaitForSeconds(duracionModoLoco);
        
        DesactivarModoLocoInternal();
    }

    void DesactivarModoLocoInternal()
    {
        if (!modoLocoActivo) return;
        
        modoLocoActivo = false;
        Debug.Log("[Slenderman] Modo loco desactivado");
        
        // Restaurar valores originales
        walkSpeed = velocidadWalkOriginal;
        runSpeed = velocidadRunOriginal;
        runRange = rangoRunOriginal;
        rotationSpeed = velocidadRotacionOriginal;
        
        // Detener efectos visuales
        if (efectoModoLoco != null)
        {
            efectoModoLoco.Stop();
        }
        
        if (modoLocoCoroutine != null)
        {
            StopCoroutine(modoLocoCoroutine);
            modoLocoCoroutine = null;
        }
    }

    void EjecutarAtaqueModoLoco()
    {
        if (derrotaActivada) return;
        
        Debug.Log("[Slenderman] ¡ATAQUE EN MODO LOCO!");
        
        // Detener movimiento
        SetState(State.Idle);
        
        // Reproducir animación de ataque
        if (animator != null)
        {
            try
            {
                animator.CrossFade("Base Layer.Attack", 0.1f);
            }
            catch
            {
                // Si no existe animación Attack, intentar crear una dinámica
                Debug.LogWarning("Animación 'Attack' no encontrada. Usando animación Run acelerada.");
                animator.speed = 2f; // Acelerar animación actual
            }
        }
        
        // Desactivar modo loco después del ataque
        DesactivarModoLocoInternal();
        
        // Iniciar secuencia de derrota después de un breve momento
        StartCoroutine(ProcesarDerrotaModoLoco());
    }

    IEnumerator ProcesarDerrotaModoLoco()
    {
        yield return new WaitForSeconds(0.3f); // Breve pausa para el ataque
        
        // Llamar a la derrota normal
        StartCoroutine(ProcesarDerrota());
    }

    IEnumerator ProcesarDerrota()
    {
        derrotaActivada = true;
        
        Debug.Log("[Slenderman] ¡Jugador atrapado! Iniciando secuencia de derrota...");
        
        // Detener modo loco si estaba activo
        if (modoLocoActivo)
        {
            DesactivarModoLocoInternal();
        }
        
        // Detener chequeos de modo loco
        CancelInvoke("ChequearModoLoco");
        
        SetState(State.Idle);
        
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
        
        enabled = false;
        
        // Animación de ataque final
        if (animator != null)
        {
            try
            {
                animator.CrossFade("Base Layer.Attack", 0.1f);
            }
            catch
            {
                animator.Play("Run"); // Fallback
            }
        }
        
        if (sonidoDerrota != null && audioSource != null)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(sonidoDerrota);
        }
        else if (audioSource != null)
        {
            audioSource.Stop();
        }
        
        if (efectoDerrota != null)
        {
            efectoDerrota.SetActive(true);
        }
        
        yield return new WaitForSeconds(retrasoDerrota);
        
        Debug.Log($"[Slenderman] Cargando escena: {escenaVideoDerrota}");
        SceneManager.LoadScene(escenaVideoDerrota);
    }

    void SetState(State newState)
    {
        if (newState == currentState) return;
        currentState = newState;

        if (animator != null)
        {
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
            if (modoLocoActivo) interval *= 0.5f; // Pasos más rápidos en modo loco
            
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
            if (audioSource != null && !derrotaActivada) audioSource.Play();
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
            RaycastHit hit;
            Vector3 rayOrigin = newPos + Vector3.up * 5f;
            if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 20f))
            {
                newPos.y = hit.point.y;
            }
        }

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
    }

    IEnumerator TeleportDelayed()
    {
        float start = Time.time;
        while (Time.time - start < teleportDelay)
        {
            if (target == null || derrotaActivada) break;
            float d = (target.position - transform.position).magnitude;
            if (d <= teleportDistance)
            {
                teleportCoroutine = null;
                yield break;
            }
            yield return null;
        }

        if (target != null && !derrotaActivada && (target.position - transform.position).magnitude > teleportDistance)
        {
            TeleportNearTarget();
        }
        teleportCoroutine = null;
    }
    
    // Para debug en el editor
    void OnDrawGizmosSelected()
    {
        // Radio normal
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Radio run (cambia en modo loco)
        Gizmos.color = modoLocoActivo ? Color.magenta : Color.red;
        Gizmos.DrawWireSphere(transform.position, runRange);
        
        // Radio de ataque
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 3f);
        
        // Indicador visual del modo loco
        if (modoLocoActivo)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawSphere(transform.position, 2f);
        }
    }
    
    // Método público para activar modo loco desde otros scripts (opcional)
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
        DesactivarModoLocoInternal();
    }
}