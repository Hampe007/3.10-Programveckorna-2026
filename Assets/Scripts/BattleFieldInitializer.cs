using UnityEngine;

public class BattleFieldInitializer : MonoBehaviour
{
    [SerializeField] SessionPlayerInputBinder binder;
    [SerializeField] CharacterCreationManager creator;
    [SerializeField] Healthbar bars;
    void Start()
    {
        binder.CreatePlayerInputs();
        creator.CreatePlayers();
        bars.Initialize();
    }
}
