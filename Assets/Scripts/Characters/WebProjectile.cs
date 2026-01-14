using System;
using UnityEngine;

public class WebProjectile : SpiderProjectlie
{    
    [SerializeField] int damage;
    
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Character character))
        {
            if(character.playerIndex != owner.playerIndex)
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
            Detonate();
            OneWayManager.instance.RemoveObject(gameObject);
            Destroy(gameObject);
        }
        else if (other.TryGetComponent<Ground>(out _))
        {
            Detonate();
            OneWayManager.instance.RemoveObject(gameObject);
            Destroy(gameObject);
        }
    }

    public void Detonate()
    {

    }
}
