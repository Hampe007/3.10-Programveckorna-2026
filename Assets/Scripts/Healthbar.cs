using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    public Slider Slider1;
    public Slider slider2;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Slider1.maxValue = CharacterTracker.instance.characters[0].maxHealth;
        slider2.maxValue = CharacterTracker.instance.characters[1].maxHealth;

        Slider1.value = CharacterTracker.instance.characters[0].health;
        slider2.value = CharacterTracker.instance.characters[1].health;

    }

    // Update is called once per frame
    void Update()
    {
        Slider1.value = CharacterTracker.instance.characters[0].health;
        slider2.value = CharacterTracker.instance.characters[1].health;
    }
}
