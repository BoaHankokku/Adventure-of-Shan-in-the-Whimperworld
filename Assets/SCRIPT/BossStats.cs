using UnityEngine;
using UnityEngine.UI;

public class BossStats : MonoBehaviour
{
    public Slider healthSlider;
    public Slider energySlider;

    // Colors for health and energy transition
    public Color lowHealthColor = Color.red;
    public Color highHealthColor = Color.red;
    public Color lowEnergyColor = Color.yellow;
    public Color highEnergyColor = Color.yellow;

    public void InitializeBars(float maxHealth, float maxEnergy)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
        }

        if (energySlider != null)
        {
            energySlider.maxValue = maxEnergy;
            energySlider.value = maxEnergy;
        }
    }

    public void UpdateStats(float currentHealth, float currentEnergy)
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
            UpdateSliderColor(healthSlider, lowHealthColor, highHealthColor);
        }

        if (energySlider != null)
        {
            energySlider.value = currentEnergy;
            UpdateSliderColor(energySlider, lowEnergyColor, highEnergyColor);
        }
    }

    private void UpdateSliderColor(Slider slider, Color lowColor, Color highColor)
    {
        if (slider.fillRect != null)
        {
            Image fillImage = slider.fillRect.GetComponentInChildren<Image>();
            if (fillImage != null)
            {
                fillImage.color = Color.Lerp(lowColor, highColor, slider.normalizedValue);
            }
        }
    }
}
