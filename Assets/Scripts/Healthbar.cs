using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    public Slider Slider1;
    public Slider slider2;
    public Slider slider3;
    public Slider slider4;

    public void Initialize()
    {
        Slider1.maxValue = CharacterTracker.instance.characters[0].maxHealth;
        slider2.maxValue = CharacterTracker.instance.characters[1].maxHealth;

        Slider1.value = CharacterTracker.instance.characters[0].health;
        slider2.value = CharacterTracker.instance.characters[1].health;

        if(CharacterTracker.instance.characters[0] is Fatboy)
        {
            slider3.gameObject.SetActive(true);
            slider3.maxValue = ((Fatboy)CharacterTracker.instance.characters[0]).maxCharge;
        }
        else
        {
            slider3.gameObject.SetActive(false);
        }

        if (CharacterTracker.instance.characters[1] is Fatboy)
        {
            slider4.gameObject.SetActive(true);
            slider4.maxValue = ((Fatboy)CharacterTracker.instance.characters[1]).maxCharge;
        }
        else
        {
            slider4.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Slider1.value = CharacterTracker.instance.characters[0].health;
        slider2.value = CharacterTracker.instance.characters[1].health;

        if(CharacterTracker.instance.characters[0] is Fatboy)
        {
            slider3.value = ((Fatboy)CharacterTracker.instance.characters[0]).charge;   
        }

        if (CharacterTracker.instance.characters[1] is Fatboy)
        {
            slider4.value = ((Fatboy)CharacterTracker.instance.characters[1]).charge;
        }
    }
}
