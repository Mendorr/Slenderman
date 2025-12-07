using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class ControladorVideoDerrota : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] string escenaMenuPrincipal = "MainScene";
    [SerializeField] float duracionMinima = 3f;
    [SerializeField] bool permitirSaltar = true;
    [SerializeField] KeyCode teclaSaltar = KeyCode.Space;

    [Header("Referencias")]
    [SerializeField] VideoPlayer videoPlayer;

    private float tiempoInicio;
    private bool videoTerminado = false;
    private bool videoPreparado = false;

    void Start()
    {
        if (videoPlayer == null)
            videoPlayer = FindObjectOfType<VideoPlayer>();

        if (videoPlayer == null)
        {
            Debug.LogWarning("No se encontró VideoPlayer. Regresando al menú...");
            Invoke("IrAlMenu", 3f);
            return;
        }

        // Paso 1: preparar video
        videoPlayer.prepareCompleted += OnVideoPreparado;
        videoPlayer.loopPointReached += OnVideoTerminado;

        Debug.Log("Preparando video de derrota...");
        videoPlayer.Prepare();
    }

    void OnVideoPreparado(VideoPlayer vp)
    {
        videoPreparado = true;
        tiempoInicio = Time.time;
        Debug.Log("Video preparado. Reproduciendo...");

        videoPlayer.Play();

        // Safety anti-bug por si falla el evento de fin
        Invoke("IrAlMenu", (float)videoPlayer.length + 1f);
    }

    void OnVideoTerminado(VideoPlayer vp)
    {
        videoTerminado = true;
        Debug.Log("Video de derrota terminado.");

        if (Time.time - tiempoInicio >= duracionMinima)
            IrAlMenu();
    }

    void Update()
    {
        // Permitir saltar video
        if (videoPreparado && permitirSaltar && Input.GetKeyDown(teclaSaltar))
        {
            if (Time.time - tiempoInicio >= 1f)
            {
                Debug.Log("Video saltado por jugador.");
                IrAlMenu();
            }
        }

        // Si terminó y ha pasado el tiempo mínimo
        if (videoTerminado && Time.time - tiempoInicio >= duracionMinima)
            IrAlMenu();
    }

    void IrAlMenu()
    {
        Debug.Log($"Cargando menú principal: {escenaMenuPrincipal}");
        SceneManager.LoadScene(escenaMenuPrincipal);
    }
}
