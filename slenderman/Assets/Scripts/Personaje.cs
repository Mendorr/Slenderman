using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Personaje : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] float velocidadMovimiento = 5f;
    [SerializeField] float velocidadRotacion = 2f;
    [SerializeField] float velocidadCorrer = 8f;
    [SerializeField] float consumoStamina = 20f;
    [SerializeField] float recuperacionStamina = 10f;

    [Header("Pisadas")]
    [SerializeField] float frecuenciaPisadas = 0.5f;
    [SerializeField] float frecuenciaPisadasCorriendo = 0.3f;
    [SerializeField] AudioClip[] sonidosPisadas;
    [SerializeField] [Range(0f, 1f)] float volumenPisadas = 0.5f;

    [Header("Referencias Slenderman")]
    [SerializeField] Transform slenderman;
    [SerializeField] Material interferenciasMaterial;
    [SerializeField] float distanciaMax = 30f;
    [SerializeField] float nitidezMax = 1f;

    [Header("Sistema de Miedo")]
    [SerializeField] bool activarSistemaMiedo = true;
    [SerializeField] float miedoMaximo = 100f;
    [SerializeField] float miedoPorSegundoCerca = 15f;
    [SerializeField] float distanciaMiedoCritico = 10f;
    [SerializeField] float distanciaMiedoAlto = 20f;
    [SerializeField] float reduccionMiedoPorSegundo = 5f;
    [SerializeField] float tiempoParaCalmarseTotal = 3f;

    [Header("Efectos de Miedo - Audio")]
    [SerializeField] AudioClip sonidoRespiracion;
    [SerializeField] AudioClip sonidoCorazon;
    [SerializeField] AudioClip sonidoSusurros;
    [SerializeField] AudioClip sonidoPanicoExtremo;
    [SerializeField] [Range(0f, 1f)] float volumenMaximoRespiracion = 0.7f;
    [SerializeField] [Range(0f, 1f)] float volumenMaximoCorazon = 0.6f;

    [Header("Efectos de Miedo - Visual")]
    [SerializeField] Image viñetaMiedo;
    [SerializeField] Color colorViñetaBajo = new Color(0, 0, 0, 0);
    [SerializeField] Color colorViñetaAlto = new Color(0.3f, 0, 0, 0.6f);
    [SerializeField] float velocidadParpadeoViñeta = 2f;
    [SerializeField] Image efectoChromaticAberration;
    [SerializeField] RawImage efectoGrano;

    [Header("Efectos de Miedo - Cámara")]
    [SerializeField] bool activarTemblor = true;
    [SerializeField] float intensidadTemblor = 0.1f;
    [SerializeField] float velocidadTemblor = 1f;
    [SerializeField] bool activarDistorsionMovimiento = true;
    [SerializeField] float distorsionMaxima = 0.3f;

    [Header("Efectos de Miedo - Gameplay")]
    [SerializeField] bool reducirVelocidadConMiedo = true;
    [SerializeField] float reduccionVelocidadMaxima = 0.5f;
    [SerializeField] bool dificultarControlConMiedo = true;
    [SerializeField] float aumentoSensibilidadMaximo = 1.5f;
    [SerializeField] bool limitarVisionConMiedo = true;
    [SerializeField] float reduccionFOVMaxima = 15f;

    [Header("UI Sistema de Miedo")]
    [SerializeField] Image barraEstamina;
    [SerializeField] Image barraMiedo;
    [SerializeField] Text textoEstadoMiedo;
    [SerializeField] GameObject panelAdvertenciaPeligro;

    [Header("Efectos de Pánico")]
    [SerializeField] bool activarModoSuperviviencia = true;
    [SerializeField] float umbralPanico = 80f;
    [SerializeField] float duracionBoostPanico = 3f;
    [SerializeField] float multiplicadorVelocidadPanico = 1.5f;

    [Header("Efectos Visuales Avanzados")]
    [SerializeField] Material materialDesaturacion;
    [SerializeField] Light luzJugador;
    [SerializeField] float reduccionLuzMaxima = 0.5f;
    [SerializeField] GameObject[] particulasMiedo;

    // Componentes privados
    private CharacterController characterController;
    private Camera mainCamera;
    private AudioSource audioSource;
    private AudioSource audioSourceRespiracion;
    private AudioSource audioSourceCorazon;
    private AudioSource audioSourceSusurros;

    // Variables de estado
    private Vector3 movimiento;
    private float rotacionY;
    private Coroutine corutinePisadas;
    private bool caminando;
    private bool corriendo;
    
    // Sistema de stamina
    private float staminaActual = 100f;
    private float staminaMaxima = 100f;
    
    // Sistema de miedo
    private float nivelMiedo = 0f;
    private float tiempoSinMiedo = 0f;
    private bool enPanico = false;
    private float tiempoPanicoRestante = 0f;
    
    // Efectos de cámara
    private Vector3 posicionOriginalCamara;
    private float FOVOriginal;
    private float intensidadLuzOriginal;
    
    // Perlin noise para efectos
    private float perlinSeed;
    
    // Textura para efecto de grano
    private Texture2D texturaGrano;
    private float tiempoGrano;

    private void Awake()
    {
        InitializeComponents();
        SetupAudioSources();
        SaveOriginalCameraSettings();
        GenerateGrainTexture();
    }

    void InitializeComponents()
    {
        characterController = GetComponent<CharacterController>();
        mainCamera = Camera.main;
        audioSource = GetComponent<AudioSource>();
        
        if (mainCamera != null)
        {
            posicionOriginalCamara = mainCamera.transform.localPosition;
            FOVOriginal = mainCamera.fieldOfView;
        }
        
        if (luzJugador != null)
        {
            intensidadLuzOriginal = luzJugador.intensity;
        }
        
        staminaActual = staminaMaxima;
        perlinSeed = Random.Range(0f, 1000f);
    }

    void SetupAudioSources()
    {
        // Audio source para respiración
        if (sonidoRespiracion != null)
        {
            GameObject respObj = new GameObject("AudioRespiracion");
            respObj.transform.SetParent(transform);
            audioSourceRespiracion = respObj.AddComponent<AudioSource>();
            audioSourceRespiracion.clip = sonidoRespiracion;
            audioSourceRespiracion.loop = true;
            audioSourceRespiracion.volume = 0f;
            audioSourceRespiracion.spatialBlend = 0f;
            audioSourceRespiracion.Play();
        }

        // Audio source para corazón
        if (sonidoCorazon != null)
        {
            GameObject heartObj = new GameObject("AudioCorazon");
            heartObj.transform.SetParent(transform);
            audioSourceCorazon = heartObj.AddComponent<AudioSource>();
            audioSourceCorazon.clip = sonidoCorazon;
            audioSourceCorazon.loop = true;
            audioSourceCorazon.volume = 0f;
            audioSourceCorazon.spatialBlend = 0f;
            audioSourceCorazon.Play();
        }

        // Audio source para susurros
        if (sonidoSusurros != null)
        {
            GameObject susurrosObj = new GameObject("AudioSusurros");
            susurrosObj.transform.SetParent(transform);
            audioSourceSusurros = susurrosObj.AddComponent<AudioSource>();
            audioSourceSusurros.clip = sonidoSusurros;
            audioSourceSusurros.loop = true;
            audioSourceSusurros.volume = 0f;
            audioSourceSusurros.spatialBlend = 0f;
        }
    }

    void SaveOriginalCameraSettings()
    {
        if (mainCamera != null)
        {
            posicionOriginalCamara = mainCamera.transform.localPosition;
            FOVOriginal = mainCamera.fieldOfView;
        }
    }

    void GenerateGrainTexture()
    {
        if (efectoGrano == null) return;
        
        texturaGrano = new Texture2D(256, 256);
        Color[] pixels = new Color[256 * 256];
        
        for (int i = 0; i < pixels.Length; i++)
        {
            float value = Random.value;
            pixels[i] = new Color(value, value, value, 0.1f);
        }
        
        texturaGrano.SetPixels(pixels);
        texturaGrano.Apply();
        texturaGrano.filterMode = FilterMode.Point;
        
        efectoGrano.texture = texturaGrano;
        efectoGrano.color = new Color(1, 1, 1, 0);
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleCameraMovement();
        ControlarInterferencias();
        
        if (activarSistemaMiedo)
        {
            UpdateFearSystem();
            ApplyFearEffects();
        }
        
        UpdateStamina();
        UpdateUI();
    }

    void HandleMovement()
    {
        float movX = Input.GetAxis("Horizontal");
        float movZ = Input.GetAxis("Vertical");

        // Detectar si está corriendo
        corriendo = Input.GetKey(KeyCode.LeftShift) && staminaActual > 0f && movZ > 0;
        
        // Calcular velocidad con modificadores de miedo y pánico
        float velocidadActual = corriendo ? velocidadCorrer : velocidadMovimiento;
        
        if (enPanico && activarModoSuperviviencia)
        {
            velocidadActual *= multiplicadorVelocidadPanico;
        }
        else if (reducirVelocidadConMiedo && nivelMiedo > 50f)
        {
            float reduccion = Mathf.Lerp(1f, reduccionVelocidadMaxima, (nivelMiedo - 50f) / 50f);
            velocidadActual *= reduccion;
        }

        // Aplicar distorsión de movimiento por miedo
        if (activarDistorsionMovimiento && nivelMiedo > 60f)
        {
            float distorsion = Mathf.Lerp(0f, distorsionMaxima, (nivelMiedo - 60f) / 40f);
            float ruido = Mathf.PerlinNoise(Time.time * 2f + perlinSeed, 0f) * 2f - 1f;
            movX += ruido * distorsion;
        }

        movimiento = transform.right * movX + transform.forward * movZ;
        
        if (movimiento.magnitude > 1f)
            movimiento.Normalize();
        
        characterController.SimpleMove(movimiento * velocidadActual);

        // Gestionar sonidos de pisadas
        bool deberiaEstarCaminando = movimiento.magnitude > 0.1f;
        
        if (deberiaEstarCaminando && !caminando)
        {
            caminando = true;
            float frecuencia = corriendo ? frecuenciaPisadasCorriendo : frecuenciaPisadas;
            corutinePisadas = StartCoroutine(FrecuenciaPisadas(frecuencia));
        }
        else if (!deberiaEstarCaminando && caminando)
        {
            caminando = false;
            if (corutinePisadas != null)
            {
                StopCoroutine(corutinePisadas);
                corutinePisadas = null;
            }
        }
    }

    void HandleCameraMovement()
    {
        float ratonX = Input.GetAxis("Mouse X") * velocidadRotacion;
        float ratonY = Input.GetAxis("Mouse Y") * velocidadRotacion;

        // Aumentar sensibilidad con el miedo
        if (dificultarControlConMiedo && nivelMiedo > 50f)
        {
            float aumento = Mathf.Lerp(1f, aumentoSensibilidadMaximo, (nivelMiedo - 50f) / 50f);
            ratonX *= aumento;
            ratonY *= aumento;
        }

        rotacionY -= ratonY;
        rotacionY = Mathf.Clamp(rotacionY, -90, 90);

        mainCamera.transform.localRotation = Quaternion.Euler(rotacionY, 0, 0);
        transform.Rotate(Vector3.up * ratonX);

        // Aplicar temblor de cámara
        if (activarTemblor && nivelMiedo > 40f)
        {
            ApplyCameraShake();
        }
        else
        {
            mainCamera.transform.localPosition = posicionOriginalCamara;
        }
    }

    void ApplyCameraShake()
    {
        float intensidad = Mathf.Lerp(0f, intensidadTemblor, (nivelMiedo - 40f) / 60f);
        float x = Mathf.PerlinNoise(Time.time * velocidadTemblor + perlinSeed, 0f) * 2f - 1f;
        float y = Mathf.PerlinNoise(0f, Time.time * velocidadTemblor + perlinSeed) * 2f - 1f;
        
        Vector3 shake = new Vector3(x, y, 0f) * intensidad;
        mainCamera.transform.localPosition = posicionOriginalCamara + shake;
    }

    void UpdateFearSystem()
    {
        if (slenderman == null) return;

        float distancia = Vector3.Distance(transform.position, slenderman.position);
        
        // Calcular incremento de miedo basado en distancia
        float incrementoMiedo = 0f;
        
        if (distancia < distanciaMiedoCritico)
        {
            // Muy cerca - miedo extremo
            incrementoMiedo = miedoPorSegundoCerca * 2f;
            tiempoSinMiedo = 0f;
            
            // Activar modo pánico
            if (!enPanico && nivelMiedo >= umbralPanico && activarModoSuperviviencia)
            {
                ActivarModoPanico();
            }
        }
        else if (distancia < distanciaMiedoAlto)
        {
            // Cerca - miedo alto
            float factor = 1f - ((distancia - distanciaMiedoCritico) / (distanciaMiedoAlto - distanciaMiedoCritico));
            incrementoMiedo = miedoPorSegundoCerca * factor;
            tiempoSinMiedo = 0f;
        }
        else if (distancia < distanciaMax)
        {
            // Distancia media - miedo moderado
            float factor = 1f - ((distancia - distanciaMiedoAlto) / (distanciaMax - distanciaMiedoAlto));
            incrementoMiedo = miedoPorSegundoCerca * 0.3f * factor;
            tiempoSinMiedo = 0f;
        }
        else
        {
            // Lejos - empezar a calmarse
            tiempoSinMiedo += Time.fixedDeltaTime;
            
            if (tiempoSinMiedo >= tiempoParaCalmarseTotal)
            {
                incrementoMiedo = -reduccionMiedoPorSegundo;
            }
        }
        
        // Actualizar nivel de miedo
        nivelMiedo = Mathf.Clamp(nivelMiedo + incrementoMiedo * Time.fixedDeltaTime, 0f, miedoMaximo);
        
        // Gestionar pánico
        if (enPanico)
        {
            tiempoPanicoRestante -= Time.fixedDeltaTime;
            if (tiempoPanicoRestante <= 0f || nivelMiedo < umbralPanico * 0.5f)
            {
                DesactivarModoPanico();
            }
        }
    }

    void ActivarModoPanico()
    {
        enPanico = true;
        tiempoPanicoRestante = duracionBoostPanico;
        
        Debug.Log("[Personaje] ¡MODO PÁNICO ACTIVADO! Boost de velocidad temporal");
        
        if (sonidoPanicoExtremo != null && audioSource != null)
        {
            audioSource.PlayOneShot(sonidoPanicoExtremo);
        }
        
        if (panelAdvertenciaPeligro != null)
        {
            StartCoroutine(ParpadearPanelPeligro());
        }
        
        // Activar partículas de pánico
        if (particulasMiedo != null)
        {
            foreach (GameObject particula in particulasMiedo)
            {
                if (particula != null)
                    particula.SetActive(true);
            }
        }
    }

    void DesactivarModoPanico()
    {
        enPanico = false;
        tiempoPanicoRestante = 0f;
        
        if (panelAdvertenciaPeligro != null)
        {
            panelAdvertenciaPeligro.SetActive(false);
        }
        
        // Desactivar partículas
        if (particulasMiedo != null)
        {
            foreach (GameObject particula in particulasMiedo)
            {
                if (particula != null)
                    particula.SetActive(false);
            }
        }
    }

    IEnumerator ParpadearPanelPeligro()
    {
        while (enPanico)
        {
            if (panelAdvertenciaPeligro != null)
            {
                panelAdvertenciaPeligro.SetActive(true);
                yield return new WaitForSeconds(0.3f);
                panelAdvertenciaPeligro.SetActive(false);
                yield return new WaitForSeconds(0.3f);
            }
            else
            {
                break;
            }
        }
    }

    void ApplyFearEffects()
    {
        float factorMiedo = nivelMiedo / miedoMaximo;
        
        // Efectos de audio
        UpdateFearAudio(factorMiedo);
        
        // Efectos visuales
        UpdateFearVisuals(factorMiedo);
        
        // Efectos de iluminación
        UpdateLightingEffects(factorMiedo);
        
        // Efectos de FOV
        if (limitarVisionConMiedo)
        {
            float targetFOV = FOVOriginal - (reduccionFOVMaxima * factorMiedo);
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, Time.fixedDeltaTime * 2f);
        }
        
        // Actualizar grano visual
        UpdateGrainEffect(factorMiedo);
    }

    void UpdateFearAudio(float factor)
    {
        // Respiración agitada
        if (audioSourceRespiracion != null)
        {
            float volumenRespiracion = Mathf.Lerp(0f, volumenMaximoRespiracion, Mathf.Pow(factor, 0.5f));
            audioSourceRespiracion.volume = volumenRespiracion;
            audioSourceRespiracion.pitch = Mathf.Lerp(1f, 1.3f, factor);
        }

        // Latidos del corazón
        if (audioSourceCorazon != null)
        {
            float volumenCorazon = Mathf.Lerp(0f, volumenMaximoCorazon, factor);
            audioSourceCorazon.volume = volumenCorazon;
            audioSourceCorazon.pitch = Mathf.Lerp(0.8f, 1.4f, factor);
        }

        // Susurros (solo con miedo alto)
        if (audioSourceSusurros != null)
        {
            if (factor > 0.6f)
            {
                if (!audioSourceSusurros.isPlaying)
                    audioSourceSusurros.Play();
                
                float volumenSusurros = Mathf.Lerp(0f, 0.3f, (factor - 0.6f) / 0.4f);
                audioSourceSusurros.volume = volumenSusurros;
            }
            else
            {
                if (audioSourceSusurros.isPlaying)
                    audioSourceSusurros.Stop();
            }
        }
    }

    void UpdateFearVisuals(float factor)
    {
        // Viñeta de miedo con parpadeo
        if (viñetaMiedo != null)
        {
            float parpadeo = Mathf.Sin(Time.time * velocidadParpadeoViñeta * factor) * 0.5f + 0.5f;
            Color colorActual = Color.Lerp(colorViñetaBajo, colorViñetaAlto, factor);
            colorActual.a *= (0.7f + parpadeo * 0.3f);
            viñetaMiedo.color = colorActual;
        }
        
        // Efecto de aberración cromática simulado
        if (efectoChromaticAberration != null)
        {
            float intensidad = Mathf.Lerp(0f, 0.5f, factor);
            efectoChromaticAberration.color = new Color(1, 1, 1, intensidad);
        }
    }

    void UpdateLightingEffects(float factor)
    {
        // Reducir intensidad de luz con el miedo
        if (luzJugador != null)
        {
            float intensidadTarget = Mathf.Lerp(intensidadLuzOriginal, intensidadLuzOriginal * reduccionLuzMaxima, factor);
            luzJugador.intensity = Mathf.Lerp(luzJugador.intensity, intensidadTarget, Time.fixedDeltaTime * 2f);
            
            // Cambiar color de luz a más rojizo con miedo alto
            if (factor > 0.6f)
            {
                Color colorMiedo = Color.Lerp(Color.white, new Color(1f, 0.5f, 0.5f), (factor - 0.6f) / 0.4f);
                luzJugador.color = colorMiedo;
            }
            else
            {
                luzJugador.color = Color.white;
            }
        }
        
        // Desaturación de la cámara
        if (materialDesaturacion != null)
        {
            float desaturacion = Mathf.Lerp(0f, 0.7f, factor);
            materialDesaturacion.SetFloat("_Desaturacion", desaturacion);
        }
    }

    void UpdateGrainEffect(float factor)
    {
        if (efectoGrano == null) return;
        
        // Actualizar opacidad del grano
        float alpha = Mathf.Lerp(0f, 0.3f, factor);
        efectoGrano.color = new Color(1, 1, 1, alpha);
        
        // Animar el grano
        tiempoGrano += Time.fixedDeltaTime * (1f + factor * 3f);
        
        if (tiempoGrano > 0.1f)
        {
            tiempoGrano = 0f;
            RegenerateGrainTexture();
        }
    }

    void RegenerateGrainTexture()
    {
        if (texturaGrano == null || efectoGrano == null) return;
        
        Color[] pixels = texturaGrano.GetPixels();
        
        for (int i = 0; i < pixels.Length; i++)
        {
            float value = Random.value;
            pixels[i] = new Color(value, value, value, pixels[i].a);
        }
        
        texturaGrano.SetPixels(pixels);
        texturaGrano.Apply();
    }

    void UpdateStamina()
    {
        if (corriendo)
        {
            staminaActual -= consumoStamina * Time.fixedDeltaTime;
            staminaActual = Mathf.Max(0f, staminaActual);
        }
        else
        {
            staminaActual += recuperacionStamina * Time.fixedDeltaTime;
            staminaActual = Mathf.Min(staminaMaxima, staminaActual);
        }
    }

    void UpdateUI()
    {
        // Actualizar barra de stamina
        if (barraEstamina != null)
        {
            barraEstamina.fillAmount = staminaActual / staminaMaxima;
        }

        // Actualizar barra de miedo
        if (barraMiedo != null)
        {
            barraMiedo.fillAmount = nivelMiedo / miedoMaximo;
            
            // Cambiar color según nivel
            if (nivelMiedo < 33f)
                barraMiedo.color = Color.green;
            else if (nivelMiedo < 66f)
                barraMiedo.color = Color.yellow;
            else
                barraMiedo.color = Color.red;
        }

        // Actualizar texto de estado
        if (textoEstadoMiedo != null)
        {
            if (enPanico)
                textoEstadoMiedo.text = "¡PÁNICO!";
            else if (nivelMiedo > 80f)
                textoEstadoMiedo.text = "Terror Extremo";
            else if (nivelMiedo > 60f)
                textoEstadoMiedo.text = "Aterrorizado";
            else if (nivelMiedo > 40f)
                textoEstadoMiedo.text = "Asustado";
            else if (nivelMiedo > 20f)
                textoEstadoMiedo.text = "Nervioso";
            else
                textoEstadoMiedo.text = "Calmado";
        }
    }

    void ControlarInterferencias()
    {
        if (slenderman == null || interferenciasMaterial == null) return;

        float distancia = Vector3.Distance(transform.position, slenderman.position);
        float intensidad = 1f - Mathf.Clamp01(distancia / distanciaMax);
        float nitidezActual = Mathf.Lerp(0, nitidezMax, intensidad);

        interferenciasMaterial.SetFloat("_Nitidez", nitidezActual);
    }

    IEnumerator FrecuenciaPisadas(float frecuencia)
    {
        while (true)
        {
            yield return new WaitForSeconds(frecuencia);
            
            // Reproducir sonido de pisada aleatorio si hay array
            if (sonidosPisadas != null && sonidosPisadas.Length > 0)
            {
                AudioClip clip = sonidosPisadas[Random.Range(0, sonidosPisadas.Length)];
                if (clip != null)
                    audioSource.PlayOneShot(clip, volumenPisadas);
            }
            else
            {
                audioSource.Play();
            }
        }
    }

    // Métodos públicos para acceso externo
    public float GetNivelMiedo() => nivelMiedo;
    public bool EstaEnPanico() => enPanico;
    public float GetStamina() => staminaActual;
    
    public void AñadirMiedo(float cantidad)
    {
        nivelMiedo = Mathf.Clamp(nivelMiedo + cantidad, 0f, miedoMaximo);
    }

    public void ReducirMiedo(float cantidad)
    {
        nivelMiedo = Mathf.Clamp(nivelMiedo - cantidad, 0f, miedoMaximo);
    }

    public void ResetearMiedo()
    {
        nivelMiedo = 0f;
        tiempoSinMiedo = 0f;
        DesactivarModoPanico();
    }

    private void OnDestroy()
    {
        // Limpiar audio sources creados
        if (audioSourceRespiracion != null)
            Destroy(audioSourceRespiracion.gameObject);
        
        if (audioSourceCorazon != null)
            Destroy(audioSourceCorazon.gameObject);
        
        if (audioSourceSusurros != null)
            Destroy(audioSourceSusurros.gameObject);
        
        // Limpiar textura
        if (texturaGrano != null)
            Destroy(texturaGrano);
    }
}