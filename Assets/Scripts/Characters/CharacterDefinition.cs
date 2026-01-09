using UnityEngine;

[CreateAssetMenu(menuName = "Fighter/Character Definition", fileName = "CharacterDefinition")]
public class CharacterDefinition : ScriptableObject
{
    [SerializeField] string characterId = "new-character";
    [SerializeField] string displayName = "New Fighter";
    [SerializeField] Sprite portrait;
    [SerializeField] GameObject fighterPrefab;
    [SerializeField] Color uiColor = Color.white;

    public string CharacterId => characterId;
    public string DisplayName => displayName;
    public Sprite Portrait => portrait;
    public GameObject FighterPrefab => fighterPrefab;
    public Color UIColor => uiColor;
}
