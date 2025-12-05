using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OpcionesVolumen : MonoBehaviour
{
    [Header("Audio Mixer")]
    public AudioMixer mixer; 

    [Header("Slider")]
    public Slider slider;    

    void Start()
    {
        // Inicializar slider con el valor actual del audio
        float value;
        if (mixer.GetFloat("MasterVolume", out value))
        {
            slider.value = Mathf.Pow(10, value / 20f); 
        }

        slider.onValueChanged.AddListener(CambiarVolumen);
    }

    public void CambiarVolumen(float v)
    {
        mixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Clamp(v, 0.0001f, 1f)) * 20f);
    }
}

