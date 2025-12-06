using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class ControladorVideoDerrota : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] string escenaMenuPrincipal = "MainScene";
    [SerializeField] float duracionMinima = 3f; // Mínimo segundos antes de cambiar
    [SerializeField] bool permitirSaltar = true;
    [SerializeField] KeyCode teclaSaltar = KeyCode.Space;
    
    [Header("Referencias")]
    [SerializeField] VideoPlayer videoPlayer;
    
    private float tiempoInicio;
    private bool videoTerminado = false;
    
    void Start()
    {
        tiempoInicio = Time.time;
        
        // Buscar VideoPlayer si no está asignado
        if (videoPlayer == null)
            videoPlayer = FindObjectOfType<VideoPlayer>();
        
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoTerminado;
            videoPlayer.Play();
            Debug.Log("Reproduciendo video de derrota...");
        }
        else
        {
            Debug.LogWarning("No se encontró VideoPlayer. Cambiando al menú en 3 segundos.");
        }
        
        // Safety: cambiar automáticamente después de 10 segundos
        Invoke("IrAlMenu", 10f);
    }
    
    void OnVideoTerminado(VideoPlayer vp)
    {
        videoTerminado = true;
        Debug.Log("Video de derrota terminado.");
        
        // Verificar tiempo mínimo
        if (Time.time - tiempoInicio >= duracionMinima)
        {
            IrAlMenu();
        }
    }
    
    void Update()
    {
        // Permitir saltar video con tecla
        if (permitirSaltar && Input.GetKeyDown(teclaSaltar))
        {
            if (Time.time - tiempoInicio >= 1f) // Esperar al menos 1 segundo
            {
                Debug.Log("Video saltado por jugador.");
                IrAlMenu();
            }
        }
        
        // Si el video terminó y ya pasó el tiempo mínimo
        if (videoTerminado && Time.time - tiempoInicio >= duracionMinima)
        {
            IrAlMenu();
        }
    }
    
    void IrAlMenu()
    {
        CancelInvoke("IrAlMenu");
        Debug.Log($"Cargando menú principal: {escenaMenuPrincipal}");
        SceneManager.LoadScene(escenaMenuPrincipal);
    }
}