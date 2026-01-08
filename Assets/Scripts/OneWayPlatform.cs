using System.Collections.Generic;
using UnityEngine;

public class OneWayPlatform : MonoBehaviour
{
    List<CollisionObject> objects = new();
    Collider myCollider;
    void Start()
    {
        myCollider = GetComponent<Collider>();
    }

    void Update()
    {
        foreach (CollisionObject other in objects)
        {
            Physics.IgnoreCollision(myCollider, other.collider, !GetCollisionActive(other));
        }
    }

    bool GetCollisionActive(CollisionObject other)
    {
        if (other.gameObject.transform.position.y + 0.1f < transform.position.y)
        {
            return false;
        }
        if (other.rigidbody.linearVelocity.y > 0)
        {
            return false;
        }
        return true;
    }

    class CollisionObject
    {
        public GameObject gameObject;
        public Collider collider;
        public Rigidbody rigidbody;

        public CollisionObject(GameObject gameObject)
        {
            this.gameObject = gameObject;
            collider = gameObject.GetComponent<Collider>();
            rigidbody = gameObject.GetComponent<Rigidbody>();
        }
    }

    public void AddCollision(GameObject gameObject)
    {
        objects.Add(new CollisionObject(gameObject));
    }
}
