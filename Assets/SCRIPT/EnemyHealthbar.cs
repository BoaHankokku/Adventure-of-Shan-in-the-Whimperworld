using UnityEngine;
using UnityEngine.UI;

public class HealthbarBehaviour : MonoBehaviour
{
    public Slider Slider;
    public Color Low;
    public Color High;
    public Vector3 Offset;

    public void SetHealth(float health, float maxHealth)
    {
        if (Slider == null)
        {
            Debug.LogError("Health bar slider is not assigned!");
            return;
        }

        Slider.gameObject.SetActive(health < maxHealth);
        Slider.value = health;
        Slider.maxValue = maxHealth;

        // Make sure the health bar color changes based on health percentage
        if (Slider.fillRect != null)
        {
            Image fillImage = Slider.fillRect.GetComponentInChildren<Image>();
            if (fillImage != null)
            {
                fillImage.color = Color.Lerp(Low, High, Slider.normalizedValue);
            }
        }
    }

    void Update()
    {
        if (Slider != null)
        {
            // Ensure the health bar follows the enemy
            Slider.transform.position = Camera.main.WorldToScreenPoint(transform.parent.position + Offset);
        }
    }
}
