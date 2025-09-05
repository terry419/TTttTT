using UnityEngine;

public class UIFeedbackManager : MonoBehaviour
{
    void OnEnable()
    {
        MonsterStats.OnMonsterDamaged += HandleMonsterDamaged;
    }

    void OnDisable()
    {
        MonsterStats.OnMonsterDamaged -= HandleMonsterDamaged;
    }

    private async void HandleMonsterDamaged(float damageAmount, Vector3 position)
    {
        if (ServiceLocator.Get<PoolManager>() == null)
        {
            return;
        }

        string key = PrefabKeys.DamageTextCanvas; // PrefabKeys에서 키 사용
        GameObject textGO = await ServiceLocator.Get<PoolManager>().GetAsync(key);

        if (textGO == null)
        {
            return;
        }
        
        textGO.transform.position = position + Vector3.up * 0.5f;

        DamageText damageTextComponent = textGO.GetComponent<DamageText>();
        if (damageTextComponent != null)
        {
            damageTextComponent.ShowDamage(damageAmount);
        }
    }
}
