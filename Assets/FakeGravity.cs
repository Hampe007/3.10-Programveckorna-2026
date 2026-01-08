using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class FakeGravity : MonoBehaviour
{
    [SerializeField] float strength;
    public bool active = true;
    Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if(active)
        {
            rb.AddForce(Vector3.down * strength, ForceMode.Acceleration);
        }   
    }
}
