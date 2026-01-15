using System.Collections.Generic;
using UnityEngine;

public class CharacterVisibilityByState : MonoBehaviour
{
    [SerializeField] Character character;
    [SerializeField] bool autoFindRenderers = true;
    [SerializeField] bool includeInactiveRenderers = true;
    [SerializeField] List<Renderer> targetRenderers = new List<Renderer>();
    [SerializeField] List<string> hiddenStates = new List<string>();

    bool isHidden;

    void Awake()
    {
        if (character == null)
        {
            character = GetComponentInParent<Character>();
        }

        if (autoFindRenderers && targetRenderers.Count == 0)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(includeInactiveRenderers);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] is MeshRenderer || renderers[i] is SkinnedMeshRenderer)
                {
                    targetRenderers.Add(renderers[i]);
                }
            }
        }
    }

    void LateUpdate()
    {
        if (character == null || targetRenderers.Count == 0)
        {
            return;
        }

        if (character.state == null)
        {
            SetHidden(false);
            return;
        }

        string stateName = character.stateName;
        bool shouldHide = StateIsHidden(stateName);
        if (shouldHide != isHidden)
        {
            SetHidden(shouldHide);
        }
    }

    bool StateIsHidden(string stateName)
    {
        for (int i = 0; i < hiddenStates.Count; i++)
        {
            if (hiddenStates[i] == stateName)
            {
                return true;
            }
        }
        return false;
    }

    void SetHidden(bool hidden)
    {
        isHidden = hidden;
        for (int i = 0; i < targetRenderers.Count; i++)
        {
            if (targetRenderers[i] != null)
            {
                targetRenderers[i].enabled = !hidden;
            }
        }
    }
}
