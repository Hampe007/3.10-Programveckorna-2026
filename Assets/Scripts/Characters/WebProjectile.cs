using System;
using UnityEngine;

public class WebProjectile : MonoBehaviour
{
    [NonSerialized] public int ownerId;
    [NonSerialized] public int direction;
    
    [SerializeField] int damage;
    [SerializeField] float speed;
    [SerializeField] float velocityUp;
    private void Start()
    {
        GetComponent<Rigidbody>().linearVelocity = Vector2.right * direction * speed + Vector2.up * velocityUp;
        OneWayManager.instance.AddObject(gameObject);
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Character character))
        {
            if(character.playerIndex != ownerId)
            {
                character.TakeHit(damage);
                character.WebHit();
                CameraControl.instance.ShakeCam(0.10f, 0.3f);
                OneWayManager.instance.RemoveObject(gameObject);
                Destroy(gameObject);
            }
        }
        else if(other.TryGetComponent<Wall>(out _))
        {
            OneWayManager.instance.RemoveObject(gameObject);
            Destroy(gameObject);
        }
        else if (other.TryGetComponent<Ground>(out _))
        {
            OneWayManager.instance.RemoveObject(gameObject);
            Destroy(gameObject);
        }
    }
}
