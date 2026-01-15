using UnityEngine;

public class TempPauser : MonoBehaviour
{
    public void Pause()
    {
        PauseManager.instance.Pause();
    }
}
