using NUnit.Framework.Internal.Execution;
using System;
using UnityEngine;

public class SpiderProjectlie : MonoBehaviour
{
    [NonSerialized] public Spider owner;
    [SerializeField] float speed;
    [SerializeField] float highAngle;
    [SerializeField] float midAngle;
    [SerializeField] float lowAngle;
    protected float travelDirection;
    void Awake()
    {
        
        OneWayManager.instance.AddObject(gameObject);
    }

    public void Launch(LaunchAngles angle, int direction)
    {        
        float launchAngle = 0;
        switch(angle)
        {
            case LaunchAngles.High:
                launchAngle = highAngle;
                break;
            case LaunchAngles.Mid:
                launchAngle = midAngle;
                break;
            case LaunchAngles.Low:
                launchAngle = lowAngle;
                break;
        }
        Vector2 launchVector = new Vector2(Mathf.Cos(launchAngle * Mathf.Deg2Rad) * direction, Mathf.Sin(launchAngle * Mathf.Deg2Rad));
        GetComponent<Rigidbody>().linearVelocity = launchVector * speed;

        travelDirection = direction;
    }

    public enum LaunchAngles
    {
        High, 
        Mid, 
        Low
    }
}
