using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
public class VolumeGeneral : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioMixer myMixer;

    [Header("UI Sliders")]
    [SerializeField] private Slider generalSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private const string GENERAL_KEY = "GeneralVolume";
    private const string MUSIC_KEY = "MusicVolume";
    private const string SFX_KEY = "SFXVolume";

    private const float MIN_VOLUME = 0.0001f; // prevents Log10(0)
    private void Start()
    {
        LoadVolume();

        // Apply volumes at startup
        SetGeneralVolume(generalSlider.value);
        SetSFXVolume(musicSlider.value);
        SetMusicVolume(sfxSlider.value);


        Debug.Log("Loaded Volumes -> " + $"General: {generalSlider.value}, " + $"Music: {musicSlider.value}, " + $"SFX: {sfxSlider.value}");

    }

    public void SetGeneralVolume(float value)
    {
        value = Mathf.Clamp(value, MIN_VOLUME, 1f);
        myMixer.SetFloat("General", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("GENERAL_KEY", value);

        Debug.Log($"Saved GeneralVolume: {value}");
    }
    public void SetMusicVolume(float value)
    {
        value = Mathf.Clamp(value, MIN_VOLUME, 1f);
        myMixer.SetFloat("Music", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("MUSIC_KEY", value);

        Debug.Log($"Saved MusicVolume: {value}");
    }

    public void SetSFXVolume(float value)
    {
        value = Mathf.Clamp(value, MIN_VOLUME, 1f);
        myMixer.SetFloat("SFX", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("SFX_KEY", value);

        Debug.Log($"Saved SFXVolume: {value}");
    }
        
    private void LoadVolume()
    {
        generalSlider.value = PlayerPrefs.GetFloat(GENERAL_KEY, 1f);
        musicSlider.value = PlayerPrefs.GetFloat(MUSIC_KEY, 1f);
        sfxSlider.value = PlayerPrefs.GetFloat(SFX_KEY, 1f);
    }
    public void ResetVolumes()
    {
        PlayerPrefs.DeleteKey("GeneralVolume");
        PlayerPrefs.DeleteKey("MusicVolume");
        PlayerPrefs.DeleteKey("SFXVolume");
        
        LoadVolume();

        Debug.Log("PlayerPrefs reset to defaults");
    }

}
