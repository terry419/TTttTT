// ���� ���: Assets/1.Scripts/UI/PlayerHealthBar.cs (����)
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField]
    private Slider healthBarSlider;

    // UI ������Ʈ�� Ȱ��ȭ�� �� �̺�Ʈ�� �����մϴ�.
    private void OnEnable()
    {
        // PlayerDataManager�� static �̺�Ʈ�� HandleHealthChanged �Լ��� ���� �����մϴ�.
        PlayerDataManager.OnHealthChanged += HandleHealthChanged;
        Debug.Log("[PlayerHealthBar] OnHealthChanged �̺�Ʈ ���� ����.");
    }

    // UI ������Ʈ�� ��Ȱ��ȭ�� �� �̺�Ʈ ������ �����մϴ�. (�޸� ���� ����)
    private void OnDisable()
    {
        PlayerDataManager.OnHealthChanged -= HandleHealthChanged;
        Debug.Log("[PlayerHealthBar] OnHealthChanged �̺�Ʈ ���� ����.");
    }

    // �̺�Ʈ�� �߻����� ���� ȣ��Ǵ� �Լ�
    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        if (healthBarSlider != null && maxHealth > 0)
        {
            healthBarSlider.value = currentHealth / maxHealth;
            Debug.Log($"[PlayerHealthBar] ü�� UI ������Ʈ: {currentHealth:F1}/{maxHealth:F1}");
        }
    }
}