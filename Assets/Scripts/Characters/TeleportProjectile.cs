using System;
using UnityEngine;

public class TeleportProjectile : MonoBehaviour
{
    [NonSerialized] public Spider owner;
    [NonSerialized] public int direction;

    [SerializeField] float speed;
    [SerializeField] float velocityUp;
    private void Start()
    {
        GetComponent<Rigidbody>().linearVelocity = Vector2.right * direction * speed + Vector2.up * velocityUp;
        OneWayManager.instance.AddObject(gameObject);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Wall>(out _))
        {
            owner.TeleportActivate(transform.position - Vector3.right * direction * 0.8f, true);
            OneWayManager.instance.RemoveObject(gameObject);
            Destroy(gameObject);
        }
        else if (other.TryGetComponent<Ground>(out _))
        {
            owner.TeleportActivate(transform.position, false);
            OneWayManager.instance.RemoveObject(gameObject);
            Destroy(gameObject);
        }
    }
}
