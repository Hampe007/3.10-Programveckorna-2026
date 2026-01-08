using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class FakeGravity : MonoBehaviour
{
    [SerializeField] float strength;
    Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        rb.AddForce(Vector3.down *  strength, ForceMode.Acceleration);
    }
}
