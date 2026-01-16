using UnityEngine;

public static class RumblePreferences
{
    private const string RumbleGainP1Key = "RumbleGainP1";
    private const string RumbleGainP2Key = "RumbleGainP2";
    private const float DefaultGain = 1f;

    private static float p1Gain = DefaultGain;
    private static float p2Gain = DefaultGain;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LoadGains()
    {
        p1Gain = Mathf.Max(0f, PlayerPrefs.GetFloat(RumbleGainP1Key, DefaultGain));
        p2Gain = Mathf.Max(0f, PlayerPrefs.GetFloat(RumbleGainP2Key, DefaultGain));
    }

    public static float GetGainForPlayer(int playerIndex)
    {
        return playerIndex == 0 ? p1Gain :
            playerIndex == 1 ? p2Gain :
            DefaultGain;
    }

    public static void SetGainForPlayer(int playerIndex, float gain)
    {
        gain = Mathf.Max(0f, gain);
        if (playerIndex == 0)
        {
            p1Gain = gain;
            PlayerPrefs.SetFloat(RumbleGainP1Key, gain);
        }
        else if (playerIndex == 1)
        {
            p2Gain = gain;
            PlayerPrefs.SetFloat(RumbleGainP2Key, gain);
        }
    }
}
