// ���� ���: Assets/1.Scripts/UI/PlayerHealthBar.cs

using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField]
    private Slider healthBarSlider;

    // Ư�� ���� ������Ʈ�� �ƴ�, ���� EntityStats�� �����ϵ��� ����
    private EntityStats entityStats;

    // Awake���� �ڽŰ� ���� ���� ������Ʈ�� �ִ� �θ� Ŭ����(EntityStats)�� ã�ƵӴϴ�.
    void Awake()
    {
        entityStats = GetComponentInParent<EntityStats>();
    }

    private void OnEnable()
    {
        if (entityStats != null)
        {
            // EntityStats�� ü�� ���� �̺�Ʈ�� �����մϴ�.
            entityStats.OnHealthChanged += HandleHealthChanged;
            // Ȱ��ȭ�� �� ���� ü������ UI�� ��� ������Ʈ�մϴ�.
            HandleHealthChanged(entityStats.CurrentHealth, entityStats.FinalHealth);
        }
    }

    private void OnDisable()
    {
        if (entityStats != null)
        {
            // ������ �����մϴ�.
            entityStats.OnHealthChanged -= HandleHealthChanged;
        }
    }

    private void HandleHealthChanged(float currentHealth, float maxHealth)
    {
        if (healthBarSlider != null && maxHealth > 0)
        {
            healthBarSlider.value = currentHealth / maxHealth;
        }
    }
}