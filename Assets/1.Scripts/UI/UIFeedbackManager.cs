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

    private void HandleMonsterDamaged(float damageAmount, Vector3 position)
    {

        // DataManager ��� PrefabProvider�� PoolManager�� ���� ����ϴ� ���� �� ȿ�����Դϴ�.
        if (ServiceLocator.Get<PoolManager>() == null || ServiceLocator.Get<PrefabProvider>() == null) return;

        GameObject damageTextPrefab = ServiceLocator.Get<PrefabProvider>().GetPrefab("DamageTextCanvas");
        if (damageTextPrefab == null) return;

        GameObject textGO = ServiceLocator.Get<PoolManager>().Get(damageTextPrefab);
        if (textGO == null) return;

        textGO.transform.position = position + Vector3.up * 0.5f;

        DamageText damageTextComponent = textGO.GetComponent<DamageText>();
        if (damageTextComponent != null)
        {
            damageTextComponent.ShowDamage(damageAmount);
        }
    }
}