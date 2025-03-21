using UnityEngine;
using UnityEngine.UI;

public class EnergyBarBehaviour : MonoBehaviour
{
    public Slider Slider;
    public Color LowEnergyColor;
    public Color HighEnergyColor;
    public Vector3 Offset;

    public void SetEnergy(float energy, float maxEnergy)
    {
        if (Slider == null)
        {
            Debug.LogError("Energy bar slider is not assigned!");
            return;
        }

        Slider.gameObject.SetActive(energy < maxEnergy);
        Slider.value = energy;
        Slider.maxValue = maxEnergy;

        // Change color based on energy percentage
        if (Slider.fillRect != null)
        {
            Image fillImage = Slider.fillRect.GetComponentInChildren<Image>();
            if (fillImage != null)
            {
                fillImage.color = Color.Lerp(LowEnergyColor, HighEnergyColor, Slider.normalizedValue);
            }
        }
    }

    void Update()
    {
        if (Slider != null)
        {
            // Ensure the energy bar follows the enemy
            Slider.transform.position = Camera.main.WorldToScreenPoint(transform.parent.position + Offset);
        }
    }
}
