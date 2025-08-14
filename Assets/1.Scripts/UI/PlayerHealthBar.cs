using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField]
    private Slider healthBarSlider;

    void LateUpdate()
    {
        transform.rotation = Quaternion.identity;
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (healthBarSlider != null && maxHealth > 0)
        {
            healthBarSlider.value = currentHealth / maxHealth;
        }
    }
}