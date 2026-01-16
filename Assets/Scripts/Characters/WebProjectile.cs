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
                BattleTimeManager.instance.HitPause(0.1f);
                character.TakeHit(damage);
                owner?.RumbleAttackHit();
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
