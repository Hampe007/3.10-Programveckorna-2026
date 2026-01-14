using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    public Slider healthP1;
    public Slider healthP2;
    public Slider chargeP1;
    public Slider chargeP2;
    public Slider webP1;
    public Slider webP2;
    public Slider teleP1;
    public Slider teleP2;

    public void Initialize()
    {
        healthP1.maxValue = CharacterTracker.instance.characters[0].maxHealth;
        healthP2.maxValue = CharacterTracker.instance.characters[1].maxHealth;

        healthP1.value = CharacterTracker.instance.characters[0].health;
        healthP2.value = CharacterTracker.instance.characters[1].health;

        if(CharacterTracker.instance.characters[0] is Fatboy)
        {
            chargeP1.gameObject.SetActive(true);
            chargeP1.maxValue = ((Fatboy)CharacterTracker.instance.characters[0]).maxCharge;
        }
        else
        {
            chargeP1.gameObject.SetActive(false);
        }

        if (CharacterTracker.instance.characters[1] is Fatboy)
        {
            chargeP2.gameObject.SetActive(true);
            chargeP2.maxValue = ((Fatboy)CharacterTracker.instance.characters[1]).maxCharge;
        }
        else
        {
            chargeP2.gameObject.SetActive(false);
        }

        if (CharacterTracker.instance.characters[0] is Spider)
        {
            webP1.gameObject.SetActive(true);
            webP1.maxValue = ((Spider)CharacterTracker.instance.characters[0]).projectileCooldown;
            teleP1.gameObject.SetActive(true);
            teleP1.maxValue = ((Spider)CharacterTracker.instance.characters[0]).projectileCooldown;
        }
        else
        {
            webP1.gameObject.SetActive(false);
            teleP1.gameObject.SetActive(false);
        }

        if (CharacterTracker.instance.characters[1] is Spider)
        {
            webP2.gameObject.SetActive(true);
            webP2.maxValue = ((Spider)CharacterTracker.instance.characters[1]).projectileCooldown;
            teleP2.gameObject.SetActive(true);
            teleP2.maxValue = ((Spider)CharacterTracker.instance.characters[1]).projectileCooldown;
        }
        else
        {
            webP2.gameObject.SetActive(false);
            teleP2.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        healthP1.value = CharacterTracker.instance.characters[0].health;
        healthP2.value = CharacterTracker.instance.characters[1].health;

        if(CharacterTracker.instance.characters[0] is Fatboy)
        {
            chargeP1.value = ((Fatboy)CharacterTracker.instance.characters[0]).charge;   
        }

        if (CharacterTracker.instance.characters[1] is Fatboy)
        {
            chargeP2.value = ((Fatboy)CharacterTracker.instance.characters[1]).charge;
        }

        if (CharacterTracker.instance.characters[0] is Spider)
        {
            webP1.value = ((Spider)CharacterTracker.instance.characters[0]).projectileCooldown - ((Spider)CharacterTracker.instance.characters[0]).webCooldown.timeLeft;
            teleP1.value = ((Spider)CharacterTracker.instance.characters[0]).projectileCooldown - ((Spider)CharacterTracker.instance.characters[0]).teleportCooldown.timeLeft;
        }

        if (CharacterTracker.instance.characters[1] is Spider)
        {
            webP2.value = ((Spider)CharacterTracker.instance.characters[1]).projectileCooldown - ((Spider)CharacterTracker.instance.characters[1]).webCooldown.timeLeft;
            teleP2.value = ((Spider)CharacterTracker.instance.characters[1]).projectileCooldown - ((Spider)CharacterTracker.instance.characters[1]).teleportCooldown.timeLeft;
        }
    }
}
