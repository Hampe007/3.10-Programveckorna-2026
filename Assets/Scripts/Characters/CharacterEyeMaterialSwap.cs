using System.Collections.Generic;
using UnityEngine;

public class CharacterEyeMaterialSwap : MonoBehaviour
{
    [SerializeField] Character character;
    [SerializeField] Renderer targetRenderer;
    [SerializeField] string targetRendererName = "Cube.001";
    [SerializeField] int eyesMaterialIndex = -1;
    [SerializeField] string eyesMaterialNameContains = "Eyes";
    [SerializeField] Material eyesNormal;
    [SerializeField] Material eyesAngry;
    [SerializeField] List<string> angryStates = new List<string>();

    string lastStateName;
    bool isAngry;
    Material[] runtimeMaterials;
    Material defaultEyesMaterial;

    void Awake()
    {
        if (character == null)
        {
            character = GetComponentInParent<Character>();
        }

        if (targetRenderer == null)
        {
            targetRenderer = FindTargetRenderer();
        }

        if (targetRenderer == null)
        {
            Debug.LogWarning($"{nameof(CharacterEyeMaterialSwap)} could not find a Renderer.", this);
            return;
        }

        runtimeMaterials = targetRenderer.materials;
        if (eyesMaterialIndex < 0)
        {
            eyesMaterialIndex = FindEyesMaterialIndex(runtimeMaterials);
        }

        if (eyesMaterialIndex < 0 || eyesMaterialIndex >= runtimeMaterials.Length)
        {
            Debug.LogWarning($"{nameof(CharacterEyeMaterialSwap)} could not resolve an eyes material index.", this);
            return;
        }

        defaultEyesMaterial = runtimeMaterials[eyesMaterialIndex];
        if (eyesNormal == null)
        {
            eyesNormal = defaultEyesMaterial;
        }
    }

    void LateUpdate()
    {
        if (character == null || targetRenderer == null || runtimeMaterials == null)
        {
            return;
        }

        if (character.state == null)
        {
            return;
        }

        string stateName = character.stateName;
        if (stateName == lastStateName)
        {
            return;
        }

        lastStateName = stateName;
        bool shouldBeAngry = angryStates.Contains(stateName);
        if (shouldBeAngry == isAngry)
        {
            return;
        }

        Material nextMaterial = shouldBeAngry ? eyesAngry : eyesNormal;
        if (nextMaterial == null)
        {
            return;
        }

        isAngry = shouldBeAngry;
        runtimeMaterials[eyesMaterialIndex] = nextMaterial;
        targetRenderer.materials = runtimeMaterials;
    }

    Renderer FindTargetRenderer()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].gameObject.name == targetRendererName)
            {
                return renderers[i];
            }
        }

        return null;
    }

    int FindEyesMaterialIndex(Material[] materials)
    {
        if (materials == null)
        {
            return -1;
        }

        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] != null && materials[i].name.Contains(eyesMaterialNameContains))
            {
                return i;
            }
        }

        return -1;
    }
}
