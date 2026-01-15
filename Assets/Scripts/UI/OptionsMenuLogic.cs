using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class OptionsMenuLogic : MonoBehaviour
{
    [Header("Riferimenti")]
    public AudioMixer audioMixer;         // Trascineremo qui il Mixer Audio
    public Slider sensitivitySlider;      // Trascineremo qui lo Slider Sensibilità
    public Slider volumeSlider;           // Trascineremo qui lo Slider Volume

    [Header("Testi dei Numeri")]
    public TextMeshProUGUI sensitivityText; // Trascineremo qui il testo numerico della sensibilità
    public TextMeshProUGUI volumeText;      // Trascineremo qui il testo numerico del volume

    void Start()
    {

        float savedSens = PlayerPrefs.GetFloat("Sensitivity", 1.0f);
        sensitivitySlider.value = savedSens;
        UpdateSensitivityText(savedSens);

        float savedVol = PlayerPrefs.GetFloat("MasterVolume", 1.0f);
        volumeSlider.value = savedVol;
        UpdateVolumeText(savedVol);
    }


    public void SetSensitivity(float sens)
    {
        PlayerPrefs.SetFloat("Sensitivity", sens);
        PlayerPrefs.Save();
        
        UpdateSensitivityText(sens);
    }

    public void SetVolume(float volume)
    {
        if (audioMixer != null)
        {
            // Converti valore lineare (0-1) in decibel (-80dB a 0dB)
            // 0 = silenzio (-80dB), 1 = volume massimo (0dB)
            float volumeDB = volume > 0.0001f 
                ? Mathf.Log10(volume) * 20f 
                : -80f;
            audioMixer.SetFloat("MasterVolume", volumeDB);
        }
        
        PlayerPrefs.SetFloat("MasterVolume", volume);
        
        UpdateVolumeText(volume);
    }

    void UpdateSensitivityText(float value)
    {
        if (sensitivityText != null)
        {
            sensitivityText.text = value.ToString("F0"); 
        }
    }

    void UpdateVolumeText(float value)
    {
        if (volumeText != null)
        {
            volumeText.text = value.ToString("F0");
        }
    }
}