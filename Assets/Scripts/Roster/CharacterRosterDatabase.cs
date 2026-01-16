using System;
using System.Collections.Generic;
using UnityEngine;

namespace LocalGame.Roster
{

    /// <summary>
    /// Maps 4-bit character IDs (0-15) to character prefabs.
    /// This is the single source of truth used by:
    /// - Character Select to populate slots
    /// - Game Scene to resolve IDs into prefabs to instantiate
    ///
    /// Per spec:
    /// - IDs must be unique
    /// - Valid range is 0..15
    /// - Random is NOT stored here (Random must resolve into a concrete ID before the match)
    /// </summary>

    [CreateAssetMenu(
        fileName = "CharacterRosterDatabase",
        menuName = "Game/Roster/Character Roster Database",
        order = 10)]
    public sealed class CharacterRosterDatabase : ScriptableObject
    {
        [Serializable]
        public sealed class Entry
        {
            [Tooltip("4-bit character ID (0-15). Must be unique.")]
            [Range(0, 15)]
            public byte characterId;

            [Tooltip("Prefab to spawn for this character ID.")]
            public GameObject characterPrefab;

            [Tooltip("Optional display name for UI (Character Select panels).")]
            public string displayName;

            [Tooltip("Optional portrait sprite for Character Select tiles.")]
            public Sprite characterPortrait;
        }

        [Header("Roster Entries (0-15)")]
        [SerializeField] private List<Entry> entries = new();

        private Dictionary<byte, Entry> _cache;

        public IReadOnlyList<Entry> Entries => entries;

        /// <summary>
        /// Strict lookup. Use this when failure should be fatal (dev-time / tests).
        /// </summary>
        public GameObject GetPrefabOrThrow(byte characterId)
        {
            EnsureCacheBuilt();

            if (characterId > 15)
                throw new ArgumentOutOfRangeException(nameof(characterId), "CharacterId must be within 0..15.");

            if (!_cache.TryGetValue(characterId, out var entry) || entry == null)
                throw new KeyNotFoundException($"No roster entry found for CharacterId={characterId}.");

            if (entry.characterPrefab == null)
                throw new InvalidOperationException($"Roster entry CharacterId={characterId} has no prefab assigned.");

            return entry.characterPrefab;
        }

        /// <summary>
        /// Safe lookup for runtime. No exceptions, no logs.
        /// </summary>
        public bool TryGetPrefab(byte characterId, out GameObject prefab)
        {
            prefab = null;

            if (characterId > 15)
                return false;

            EnsureCacheBuilt();

            if (!_cache.TryGetValue(characterId, out var entry) || entry == null)
                return false;

            if (entry.characterPrefab == null)
                return false;

            prefab = entry.characterPrefab;
            return true;
        }

        /// <summary>
        /// Safe lookup for runtime with a single clear log if something is wrong.
        /// This is where try/catch helps: you can call it from scene boot/spawning without risking crashes.
        /// </summary>
        public GameObject GetPrefabSafe(byte characterId, UnityEngine.Object logContext = null)
        {
            try
            {
                return GetPrefabOrThrow(characterId);
            }
            catch (Exception ex)
            {
                var ctx = logContext != null ? logContext : this;
                Debug.LogError(
                    $"[RosterDB:{name}] Failed to resolve prefab for CharacterId={characterId}. " +
                    $"Reason: {ex.GetType().Name}: {ex.Message}",
                    ctx);
                return null;
            }
        }

        /// <summary>
        /// Editor + runtime validation helper.
        /// Returns a list of problems (no exceptions).
        /// </summary>
        public List<string> ValidateRoster()
        {
            var problems = new List<string>();
            var seen = new HashSet<byte>();

            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (e == null)
                {
                    problems.Add($"Entry[{i}] is null.");
                    continue;
                }

                if (e.characterId > 15)
                    problems.Add($"Entry[{i}] has CharacterId={e.characterId} (out of range 0..15).");

                if (!seen.Add(e.characterId))
                    problems.Add($"Duplicate CharacterId={e.characterId} found (must be unique).");

                if (e.characterPrefab == null)
                    problems.Add($"Entry[{i}] CharacterId={e.characterId} has no prefab assigned.");
            }

            return problems;
        }

        private void EnsureCacheBuilt()
        {
            if (_cache != null) return;

            _cache = new Dictionary<byte, Entry>(capacity: 16);
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (e == null) continue;

                // First one wins; validation should prevent duplicates.
                if (!_cache.ContainsKey(e.characterId))
                    _cache.Add(e.characterId, e);
            }
        }

        private void OnEnable()
        {
            _cache = null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Try/catch here prevents editor weirdness if something goes sideways.
            try
            {
                _cache = null;

                var problems = ValidateRoster();
                if (problems.Count > 0)
                {
                    Debug.LogWarning(
                        $"[{name}] Roster validation found {problems.Count} issue(s):\n- " +
                        string.Join("\n- ", problems),
                        this);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[{name}] OnValidate crashed: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}",
                    this);
            }
        }
#endif
    }
}
