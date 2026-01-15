using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimatorDriver : MonoBehaviour
{
    [Serializable]
    struct StateMapping
    {
        public string stateName;
        public string animState;
    }

    [SerializeField] Character character;
    [SerializeField] Animator animator;
    [SerializeField, Min(0f)] float crossFadeTime = 0.05f;
    [SerializeField] List<StateMapping> mappings = new List<StateMapping>();

    string lastStateName;
    readonly HashSet<string> missingStates = new HashSet<string>();

    void Awake()
    {
        if (character == null)
        {
            character = GetComponentInParent<Character>();
        }
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    void LateUpdate()
    {
        if (character == null || animator == null || character.state == null)
        {
            return;
        }

        string stateName = character.stateName;
        if (string.IsNullOrEmpty(stateName) || stateName == lastStateName)
        {
            return;
        }

        lastStateName = stateName;
        string animState = ResolveAnimState(stateName);
        int stateHash = Animator.StringToHash(animState);
        if (!animator.HasState(0, stateHash))
        {
            if (missingStates.Add(animState))
            {
                Debug.LogWarning($"Animator state '{animState}' not found on {animator.name}.", animator);
            }
            return;
        }

        animator.CrossFadeInFixedTime(stateHash, crossFadeTime);
    }

    string ResolveAnimState(string stateName)
    {
        for (int i = 0; i < mappings.Count; i++)
        {
            if (mappings[i].stateName == stateName && !string.IsNullOrEmpty(mappings[i].animState))
            {
                return mappings[i].animState;
            }
        }

        return stateName;
    }
}
