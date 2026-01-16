using UnityEngine;
using UnityEngine.UI;

public class RumbleSettingsMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider p1RumbleSlider;
    [SerializeField] private Slider p2RumbleSlider;

    [Header("Gain Range")]
    [Min(0f)][SerializeField] private float minGain = 0f;
    [Min(0f)][SerializeField] private float maxGain = 2f;

    private void Start()
    {
        if (p1RumbleSlider == null || p2RumbleSlider == null)
        {
            Debug.LogWarning("[RumbleSettingsMenu] One or more rumble sliders are not assigned.", this);
            return;
        }

        float p1Gain = RumblePreferences.GetGainForPlayer(0);
        float p2Gain = RumblePreferences.GetGainForPlayer(1);
        p1RumbleSlider.SetValueWithoutNotify(GainToSlider(p1Gain));
        p2RumbleSlider.SetValueWithoutNotify(GainToSlider(p2Gain));
    }

    public void SetP1RumbleAmount(float sliderValue)
    {
        float gain = SliderToGain(sliderValue);
        RumblePreferences.SetGainForPlayer(0, gain);
    }

    public void SetP2RumbleAmount(float sliderValue)
    {
        float gain = SliderToGain(sliderValue);
        RumblePreferences.SetGainForPlayer(1, gain);
    }

    private float SliderToGain(float sliderValue)
    {
        sliderValue = Mathf.Clamp01(sliderValue);
        float min = Mathf.Min(minGain, maxGain);
        float max = Mathf.Max(minGain, maxGain);
        return Mathf.Lerp(min, max, sliderValue);
    }

    private float GainToSlider(float gain)
    {
        float min = Mathf.Min(minGain, maxGain);
        float max = Mathf.Max(minGain, maxGain);
        if (Mathf.Abs(max - min) < 0.0001f)
            return 1f;

        return Mathf.InverseLerp(min, max, gain);
    }
}
