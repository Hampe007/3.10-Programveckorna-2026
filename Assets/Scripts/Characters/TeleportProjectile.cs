using System;
using UnityEngine;

public class TeleportProjectile : SpiderProjectlie
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Wall>(out _))
        {
            owner.TeleportActivate(transform.position - Vector3.right * travelDirection * 0.2f, true);
            CameraControl.instance.ShakeCam(0.10f, 0.2f);
            OneWayManager.instance.RemoveObject(gameObject);
            Destroy(gameObject);
        }
        else if (other.TryGetComponent<Ground>(out _))
        {
            owner.TeleportActivate(transform.position, false);
            CameraControl.instance.ShakeCam(0.10f, 0.2f);
            OneWayManager.instance.RemoveObject(gameObject);
            Destroy(gameObject);
        }
    }

    public void Trigger()
    {
        owner.TeleportActivate(transform.position, true);
        CameraControl.instance.ShakeCam(0.10f, 0.2f);
        OneWayManager.instance.RemoveObject(gameObject);
        Destroy(gameObject);
    }
}
