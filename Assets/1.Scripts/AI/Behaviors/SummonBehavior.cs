// ���: ./TTttTT/Assets/1/Scripts/AI/Behaviors/SummonBehavior.cs
using UnityEngine;
using Cysharp.Threading.Tasks;

/// <summary>
/// [��� �ൿ ��ǰ] ������ �ð� ���� ĳ���� ��, �ϼ����� ��ȯ�ϴ� �ൿ�Դϴ�.
/// </summary>
[CreateAssetMenu(menuName = "Monster AI/Behaviors/Summon")]
public class SummonBehavior : MonsterBehavior
{
    [Header("��ȯ ����")]
    [Tooltip("��ȯ�� �ϼ����� MonsterDataSO ������ �����մϴ�.")]
    public MonsterDataSO minionToSummon;
    [Tooltip("�� ���� ��ȯ�� �ϼ����� ���Դϴ�.")]
    public int summonCount = 3;
    [Tooltip("��ȯ ����(ĳ����)�� �ɸ��� �ð�(��)�Դϴ�.")]
    public float summonCastTime = 1.5f;

    public override void OnEnter(MonsterController monster)
    {
        // ��ȯ�� �����ϸ�, �� �ڸ��� ����ϴ�.
        monster.rb.velocity = Vector2.zero;
        Log.Info(Log.LogCategory.AI_Behavior, $"{monster.name}��(��) �ϼ��� ��ȯ�� �����մϴ�. (ĳ���� �ð�: {summonCastTime}��)");
    }

    public override void OnExecute(MonsterController monster)
    {
        // ĳ���� �ð� ���ȿ��� ��� ���ڸ��� �����ֽ��ϴ�.
        monster.rb.velocity = Vector2.zero;

        // ĳ���� �ð��� �� �������� Ȯ���մϴ�.
        if (monster.stateTimer >= summonCastTime)
        {
            // �ð��� �� �Ǹ�, ��ȯ�� �����ϰ� ���� �ൿ���� �Ѿ�ϴ�.
            Log.Info(Log.LogCategory.AI_Behavior, $"{monster.name}��(��) {minionToSummon.monsterName} {summonCount}������ ��ȯ�մϴ�!");

            // �񵿱� �۾��̹Ƿ�, UniTask�� 'Forget'���� ó���մϴ�.
            SummonMinionsAsync(monster).Forget();

            // ��ȯ�� ��������, ���� �ൿ���� ��ȯ���� �˻��մϴ�.
            CheckTransitions(monster);
        }
    }

    private async UniTaskVoid SummonMinionsAsync(MonsterController summoner)
    {
        if (minionToSummon == null || !minionToSummon.prefabRef.RuntimeKeyIsValid())
        {
            Log.Error(Log.LogCategory.AI_Behavior, $"'{summoner.name}'��(��) ��ȯ�Ϸ��� �ϼ���({minionToSummon?.name})�� �����Ͱ� ��ȿ���� �ʽ��ϴ�.");
            return;
        }

        var poolManager = ServiceLocator.Get<PoolManager>();
        if (poolManager == null) return;

        string key = minionToSummon.prefabRef.AssetGUID;

        for (int i = 0; i < summonCount; i++)
        {
            // ��ȯ�� �ֺ��� ������ ��ġ�� ����մϴ�.
            Vector2 randomOffset = Random.insideUnitCircle * 2.0f; // 2���� �ݰ� ��
            Vector3 spawnPosition = summoner.transform.position + (Vector3)randomOffset;

            // Ǯ �Ŵ����� ���� �ϼ��� �ν��Ͻ��� �����ɴϴ�.
            GameObject minionInstance = await poolManager.GetAsync(key);
            if (minionInstance != null)
            {
                minionInstance.transform.position = spawnPosition;

                // �ϼ��� ���͸� �ʱ�ȭ�մϴ�.
                MonsterController mc = minionInstance.GetComponent<MonsterController>();
                if (mc != null)
                {
                    mc.countsTowardKillGoal = false;
                    mc.Initialize(minionToSummon);
                }
            }
        }
    }
}