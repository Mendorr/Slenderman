using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.SceneManagement;

public class TextoHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("REFERENCIA")]
    public TextMeshProUGUI texto;
    
    [Header("REFERENCIA PANEL OPCIONES")]
    public GameObject panelOpciones; // Referencia al panel de opciones

    [Header("COLORS")]
    public Color colorNormal = Color.white;
    public Color colorHover = new Color(0.6f, 0f, 0f); // rojo sangre oscuro

    [Header("SCALE")]
    public float tamanoNormal = 20f;
    public float tamanoHover = 15f;

    public enum AccionBoton { Ninguna, Jugar, Salir, Opciones, CerrarOpciones } 
    [Header("ACTION")]
    public AccionBoton accion = AccionBoton.Ninguna;

    // Añade una bandera para prevenir errores durante inicialización
    private bool inicializado = false;

    void Start()
    {
        InicializarReferencias();
    }

    // También inicializa si se activa/desactiva
    void OnEnable()
    {
        if (!inicializado)
            InicializarReferencias();
    }

    void InicializarReferencias()
    {
        // Busca el TextMeshProUGUI en este GameObject si no está asignado
        if (texto == null)
        {
            texto = GetComponent<TextMeshProUGUI>();
            
            // Si aún es nulo, busca en los hijos
            if (texto == null)
                texto = GetComponentInChildren<TextMeshProUGUI>();
        }

        // Si sigue siendo nulo, muestra un error
        if (texto == null)
        {
            Debug.LogError("No se encontró el componente TextMeshProUGUI en " + gameObject.name);
            return;
        }

        texto.color = colorNormal;
        texto.fontSize = tamanoNormal;
        
        // Si hay panel de opciones y es el botón de opciones, asegurarse de que esté desactivado
        if (panelOpciones != null && accion == AccionBoton.Opciones)
        {
            panelOpciones.SetActive(false);
        }

        inicializado = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Verifica que texto no sea nulo antes de usarlo
        if (texto != null)
        {
            texto.color = colorHover;
            texto.fontSize = tamanoHover;
        }
        else
        {
            Debug.LogWarning("Texto es nulo en OnPointerEnter. Intentando re-inicializar...");
            InicializarReferencias();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (texto != null)
        {
            texto.color = colorNormal;
            texto.fontSize = tamanoNormal;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        switch (accion)
        {
            case AccionBoton.Jugar:
                SceneManager.LoadScene("SampleScene");
                break;

            case AccionBoton.Salir:
                Application.Quit();

                // Esto permite que funcione en el editor
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #endif
                break;
                
            case AccionBoton.Opciones:
                if (panelOpciones != null)
                {
                    // Activar el panel de opciones
                    panelOpciones.SetActive(true);
                }
                else
                {
                    Debug.LogWarning("No se ha asignado el panel de opciones en el inspector");
                }
                break;
                
            case AccionBoton.CerrarOpciones:
                if (panelOpciones != null)
                {
                    // Desactivar el panel de opciones
                    panelOpciones.SetActive(false);
                }
                break;
        }
    }
}