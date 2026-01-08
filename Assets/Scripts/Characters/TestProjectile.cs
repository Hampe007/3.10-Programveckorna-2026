using System;
using UnityEngine;

public class TestProjectile : MonoBehaviour
{
    [NonSerialized] public int ownerId;
    [NonSerialized] public int direction;
    
    [SerializeField] int damage;
    [SerializeField] float speed;
    private void Start()
    {
        GetComponent<Rigidbody>().linearVelocity = Vector2.right * direction * speed;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Character character))
        {
            if(character.playerIndex != ownerId)
            {
                character.TakeHit(damage);
                Destroy(gameObject);
            }
        }
        else if(other.TryGetComponent<Wall>(out _))
        {
            Destroy(gameObject);
        }
    }
}
