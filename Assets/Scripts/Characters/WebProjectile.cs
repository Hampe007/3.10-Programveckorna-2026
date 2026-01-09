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
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Character character))
        {
            if(character.playerIndex != ownerId)
            {
                character.TakeHit(damage);
                character.WebHit();
                Destroy(gameObject);
            }
        }
        else if(other.TryGetComponent<Wall>(out _))
        {
            Destroy(gameObject);
        }
        else if (other.TryGetComponent<Ground>(out _))
        {
            Destroy(gameObject);
        }
    }
}
