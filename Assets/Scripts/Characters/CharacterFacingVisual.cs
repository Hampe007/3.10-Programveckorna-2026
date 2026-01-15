using UnityEngine;

public class CharacterFacingVisual : MonoBehaviour
{
    [SerializeField] Character character;
    [SerializeField] Transform visualRoot;
    [SerializeField] Vector3 flipEuler = new Vector3(0f, 180f, 0f);

    Quaternion baseRotation;

    void Awake()
    {
        if (character == null)
        {
            character = GetComponentInParent<Character>();
        }

        if (visualRoot == null)
        {
            visualRoot = transform;
        }

        baseRotation = visualRoot.rotation;
    }

    void LateUpdate()
    {
        if (character == null || visualRoot == null)
        {
            return;
        }

        visualRoot.rotation = baseRotation *
            (character.facingLeft ? Quaternion.Euler(flipEuler) : Quaternion.identity);
    }
}
