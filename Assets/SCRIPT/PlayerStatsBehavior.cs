using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsBarBehaviour : MonoBehaviour
{
    public Slider healthSlider;
    public Slider manaSlider;
    public Slider expSlider;

    public Color lowHealthColor;
    public Color highHealthColor;
    public Color lowManaColor;
    public Color highManaColor;
    public Color lowExpColor;
    public Color highExpColor;

    // Sets health bar values and colors
    public void SetHealth(float health, float maxHealth)
    {
        healthSlider.value = health;
        healthSlider.maxValue = maxHealth;
        healthSlider.fillRect.GetComponentInChildren<Image>().color = Color.Lerp(lowHealthColor, highHealthColor, healthSlider.normalizedValue);
    }

    // Sets mana bar values and colors
    public void SetMana(float mana, float maxMana)
    {
        manaSlider.value = mana;
        manaSlider.maxValue = maxMana;
        manaSlider.fillRect.GetComponentInChildren<Image>().color = Color.Lerp(lowManaColor, highManaColor, manaSlider.normalizedValue);
    }

    // Sets experience bar values and colors
    public void SetExp(float exp, float maxExp)
    {
        expSlider.value = exp;
        expSlider.maxValue = maxExp;
        expSlider.fillRect.GetComponentInChildren<Image>().color = Color.Lerp(lowExpColor, highExpColor, expSlider.normalizedValue);
    }
}
