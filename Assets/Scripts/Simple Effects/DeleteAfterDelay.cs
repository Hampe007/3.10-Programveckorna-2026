using UnityEngine;

public class DeleteAfterDelay : MonoBehaviour
{
    [SerializeField] float delay;

    void Update()
    {
        delay -= Time.deltaTime;
        if(delay <= 0)
        {
            Destroy(gameObject);
        }
    }
}
