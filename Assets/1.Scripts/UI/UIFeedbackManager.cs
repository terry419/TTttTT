using UnityEngine;

public class UIFeedbackManager : MonoBehaviour
{
    void OnEnable()
    {
        MonsterController.OnMonsterDamaged += HandleMonsterDamaged;
    }

    void OnDisable()
    {
        MonsterController.OnMonsterDamaged -= HandleMonsterDamaged;
    }

    private async void HandleMonsterDamaged(float damageAmount, Vector3 position)
    {
        if (ServiceLocator.Get<PoolManager>() == null) return;

        string key = "DamageTextCanvas"; // 키를 직접 사용
        GameObject textGO = await ServiceLocator.Get<PoolManager>().GetAsync(key);

        if (textGO == null) return;
        
        textGO.transform.position = position + Vector3.up * 0.5f;

        DamageText damageTextComponent = textGO.GetComponent<DamageText>();
        if (damageTextComponent != null)
        {
            damageTextComponent.ShowDamage(damageAmount);
        }
    }
}
