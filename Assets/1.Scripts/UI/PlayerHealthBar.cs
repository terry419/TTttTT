// ���� ���: Assets/1/Scripts/UI/PlayerHealthBar.cs (���� ������)
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField]
    private Slider healthBarSlider;


    private void OnEnable()
    {
        PlayerDataManager.OnHealthChanged += HandleHealthChanged;
    }

    private void OnDisable()
    {
        PlayerDataManager.OnHealthChanged -= HandleHealthChanged;
    }

    void LateUpdate()
    {
        transform.rotation = Quaternion.identity;
    }

    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        if (healthBarSlider != null && maxHealth > 0)
        {
            healthBarSlider.value = currentHealth / maxHealth;
        }
    }
}