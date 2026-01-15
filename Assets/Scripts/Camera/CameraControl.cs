using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public static CameraControl instance { get; private set; }
    Camera linkedCamera;
    public Vector2 offset;
    List<Shake> activeShakes = new List<Shake>(); //All ongoing shake effects.
    public Transform trackedTransform; //The transform the camera follows

    private void Awake()
    {
        if (instance != null) //Ensures there is always only 1 ScreenShake, accessible through the static ScreenShake.Instance.
        {
            Destroy(gameObject);
            Debug.LogWarning("Destroyed a camera as there was already a CamerControl instance");
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        linkedCamera = GetComponent<Camera>();
    }





    void LateUpdate()
    {
        transform.position = GetBasePosition();

        if(Time.timeScale > 0) //Don't shake during hitstop
        {
            ShakeAll();
        }
    }

    Vector3 GetBasePosition()
    {
        Vector2 targetPos = trackedTransform.position;

        return new Vector3(targetPos.x, targetPos.y, -10) + (Vector3)offset;
    }

    /// <summary>
    /// Queues a camera shake with the seleced time and strength.
    /// </summary>
    /// <param name="time"></param>
    /// <param name="strength"></param>
    public void ShakeCam(float time, float strength)
    {
        activeShakes.Add(new Shake(time, strength));
    }

    /// <summary>
    /// Deteriorates all active shakes and shakes by the strongest one
    /// </summary>
    void ShakeAll()
    {
        if (activeShakes.Count > 0)
        {
            Shake StrongestShake = new Shake(69, 420); //Serves no purpose other than to make variable not empty so errors aren't thrown
            foreach (Shake shake in activeShakes) //Gets the strongest shake
            {
                float MaxStrength = 0;
                //shake.RemainingTime -= Time.deltaTime;

                if (shake.remainingStrength >= MaxStrength)
                {
                    StrongestShake = shake;
                }
            }
            ShakeCamera(StrongestShake); //Shakes by the strongest shake's stength
            List<Shake> ToBeRemoved = new List<Shake>(); //List of finished shakes to clear from list
            foreach (Shake shake in activeShakes)
            {
                if (shake.Deteriorate()) //If the shake is over, mark it for deletion
                {
                    ToBeRemoved.Add(shake);
                }
            }
            foreach (Shake shake in ToBeRemoved)
            {
                activeShakes.Remove(shake);
            }
        }
    }

    /// <summary>
    /// Moves the camera in a random direction a distance equal to the shake's strength
    /// </summary>
    /// <param name="shake"></param>
    void ShakeCamera(Shake shake)
    {
        float randomAngle = Random.Range(0f, 360f);  //Gets a random angle up to 90 degrees

        //Gets the x and y for that angle
        float yShift = shake.remainingStrength * Mathf.Sin(randomAngle);
        float xShift = shake.remainingStrength * Mathf.Cos(randomAngle);

        //Move the camera
        transform.position = GetBasePosition() + new Vector3(xShift, yShift);
    }
}
/// <summary>
/// Keeps track of Time and Strength
/// </summary>
class Shake
{
    public float startTime;
    public float startStrength;
    public float remainingTime;
    public float remainingStrength;

    public Shake(float time, float strength)
    {
        startTime = time;
        startStrength = strength;
        remainingTime = time;
        remainingStrength = strength;
    }
    /// <summary>
    /// Reduces time and strength of the shake
    /// </summary>
    /// <returns></returns>
    public bool Deteriorate()
    {
        remainingTime -= Time.deltaTime;
        if (remainingTime < 0f)
        {
            return true;
        }
        else
        {
            remainingStrength = startStrength * (remainingTime / startTime);
            return false;
        }
    }

}
