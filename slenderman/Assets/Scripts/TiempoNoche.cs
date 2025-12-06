using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TiempoNoche : MonoBehaviour
{
    [SerializeField] float duracionHora = 60f; // 1 "hora" = 60 segundos (puedes cambiarlo)
    [SerializeField] int horasParaGanar = 6;   // Ganar tras sobrevivir X horas
    [SerializeField] TextMeshProUGUI textoHoras;

    private float tiempoTranscurrido = 0f;
    private int horasActuales = 0;
    private bool juegoTerminado = false;

    void Update()
    {
        if (juegoTerminado) return;

        tiempoTranscurrido += Time.deltaTime;

        if (tiempoTranscurrido >= duracionHora)
        {
            tiempoTranscurrido = 0f;
            horasActuales++;
            ActualizarTexto();

            if (horasActuales >= horasParaGanar)
            {
                JuegoGanado();
            }
        }
    }

    void ActualizarTexto()
    {
        textoHoras.text = $"Horas sobrevividas: {horasActuales}/{horasParaGanar}";
    }

    public void JuegoGanado()
    {
        juegoTerminado = true;
        Debug.Log("¡Has sobrevivido a la noche! ¡Has ganado!");
        SceneManager.LoadScene("VictoryScene");
    }
}
