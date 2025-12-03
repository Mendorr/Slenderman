using UnityEngine;
using System.Collections;

public class Personaje : MonoBehaviour
{
    [SerializeField] float velocidadMovimiento;
    [SerializeField] float velocidadRotacion;
    [SerializeField] float frecuenciaPisadas;

    [SerializeField] Transform slenderman;
    [SerializeField] Material interferenciasMaterial;   

    [SerializeField] float distanciaMax = 30f;   // A partir de aquí no hay interferencias
    [SerializeField] float nitidezMax = 1f;      // Valor máximo del shader cuando está muy cerca

    private CharacterController chracterController;
    private Camera camera;
    private AudioSource audioSource;

    private Vector3 movimiento;
    private float rotacionY;
    private Coroutine corutine;
    private bool caminando;

    private void Awake()
    {
        chracterController = GetComponent<CharacterController>();
        camera = Camera.main;
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        Movimiento();
        MovimientoCamara();
        ControlarInterferencias();
    }

    void ControlarInterferencias()
{
        if (slenderman == null || interferenciasMaterial == null) return;

        float distancia = Vector3.Distance(transform.position, slenderman.position);

        // Cuanto más cerca, mayor nitidez
        float intensidad = 1f - Mathf.Clamp01(distancia / distanciaMax);

        float nitidezActual = Mathf.Lerp(0, nitidezMax, intensidad);

        interferenciasMaterial.SetFloat("_Nitidez", nitidezActual);
    }

    void Movimiento()
    {
        float movX = Input.GetAxis("Horizontal");
        float movZ = Input.GetAxis("Vertical");

        movimiento = transform.right * movX + transform.forward * movZ;
        chracterController.SimpleMove(movimiento * velocidadMovimiento);

        if (movimiento != Vector3.zero && caminando == false)
        {
            caminando = true;
            corutine = StartCoroutine(FrecuenciaPisadas());
        }
        else if (movimiento == Vector3.zero && corutine != null)
        {
            caminando = false;
            StopCoroutine(corutine);
        }
    }

    void MovimientoCamara()
    {
        float ratonX = Input.GetAxis("Mouse X") * velocidadRotacion;
        float ratonY = Input.GetAxis("Mouse Y") * velocidadRotacion;

        rotacionY -= ratonY;
        rotacionY = Mathf.Clamp(rotacionY, -90, 90);

        camera.transform.localRotation = Quaternion.Euler(rotacionY, 0, 0);
        transform.Rotate(Vector3.up * ratonX);

    }

    IEnumerator FrecuenciaPisadas()
    {
        while (true)
        {
            yield return new WaitForSeconds(frecuenciaPisadas);
            audioSource.Play();
        }
    }
}
