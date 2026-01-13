using UnityEngine;

public class BattleFieldInitializer : MonoBehaviour
{
    [SerializeField] SessionPlayerInputBinder binder;
    [SerializeField] CharacterCreationManager creator;
    void Start()
    {
        binder.CreatePlayerInputs();
        creator.CreatePlayers();
    }
}
