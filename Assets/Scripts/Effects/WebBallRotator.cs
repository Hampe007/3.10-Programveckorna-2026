using UnityEngine;

public class WebBallRotator : MonoBehaviour
{
    [SerializeField] float rotSpeed = 360;

    void Update()
    {
        transform.Rotate(new Vector3(0, Time.deltaTime * rotSpeed, 0));
    }
}
