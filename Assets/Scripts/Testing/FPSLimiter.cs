using UnityEngine;

public class FPSLimiter : MonoBehaviour
{
    [SerializeField] int frames;
    private void Start()
    {
        Application.targetFrameRate = -1;
    }

    void OnEnable()
    {
        Application.targetFrameRate = frames;
    }

    void OnDisable()
    {
        Application.targetFrameRate = -1;
    }
}
