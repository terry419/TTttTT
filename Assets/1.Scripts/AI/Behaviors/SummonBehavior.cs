// ���: ./TTttTT/Assets/1/Scripts/AI/Behaviors/SummonBehavior.cs
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System; // Serializable�� ���� �߰�

/// <summary>
/// [����] ��ȯ ����� �� �׸��� �����ϴ� ������ �����Դϴ�.
/// MonsterDataSO�� summonCount�� �ϳ��� ������ �����ϴ�.
/// </summary>
[Serializable]
public class SummonEntry
{
    [Tooltip("��ȯ�� �ϼ����� MonsterDataSO �����Դϴ�.")]
    public MonsterDataSO minionToSummon;
    [Tooltip("�� �ϼ����� �� ���� ��ȯ���� ���մϴ�.")]
    public int summonCount = 1;
}

/// <summary>
/// [��� �ൿ ��ǰ - ���� ���� ��ȯ ������] 
/// ��Ͽ� �ִ� ��� ������ �ϼ����� ���� ������ ����ŭ ���ÿ� ��ȯ�ϴ� �ൿ�Դϴ�.
/// </summary>
[CreateAssetMenu(menuName = "Monster AI/Behaviors/Summon")]
public class SummonBehavior : MonsterBehavior
{
    [Header("��ȯ ����")]
    [Tooltip("��ȯ ����(ĳ����)�� �ɸ��� �ð�(��)�Դϴ�.")]
    public float summonCastTime = 1.5f;

    [Header("��ȯ ���")]
    [Tooltip("�� �ൿ���� ��ȯ�� ��� �ϼ��ε��� ����Դϴ�.")]
    public List<SummonEntry> summonList;

    public override void OnEnter(MonsterController monster)
    {
        base.OnEnter(monster);
        monster.rb.velocity = Vector2.zero;
        Log.Info(Log.LogCategory.AI_Behavior, $"{monster.name}��(��) �ϼ��� ��ȯ�� �����մϴ�. (ĳ���� �ð�: {summonCastTime}��)");
    }

    public override void OnExecute(MonsterController monster)
    {
        monster.rb.velocity = Vector2.zero;

        if (monster.stateTimer >= summonCastTime)
        {
            Log.Info(Log.LogCategory.AI_Behavior, $"{monster.name}��(��) ��ȯ ��Ͽ� ���� �ϼ����� ��ȯ�մϴ�!");
            SummonMinionsAsync(monster).Forget();
            CheckTransitions(monster);
        }
    }

    private async UniTaskVoid SummonMinionsAsync(MonsterController summoner)
    {
        if (summonList == null || summonList.Count == 0)
        {
            Log.Error(Log.LogCategory.AI_Behavior, $"'{summoner.name}'��(��) ��ȯ�� �ϼ��� ���(Summon List)�� ����ֽ��ϴ�.");
            return;
        }

        var poolManager = ServiceLocator.Get<PoolManager>();
        if (poolManager == null) return;

        // ��Ͽ� �ִ� ��� �׸�(Entry)�� ���� �ݺ��մϴ�.
        foreach (var entry in summonList)
        {
            if (entry.minionToSummon == null || !entry.minionToSummon.prefabRef.RuntimeKeyIsValid())
            {
                Log.Warn(Log.LogCategory.AI_Behavior, $"��ȯ ����� �׸� �� �ϳ��� ��ȿ���� �ʾ� �ǳʶݴϴ�.");
                continue;
            }

            // �� �׸� ������ summonCount ��ŭ �ݺ��մϴ�.
            for (int i = 0; i < entry.summonCount; i++)
            {
                string key = entry.minionToSummon.prefabRef.AssetGUID;
                Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * 2.0f;
                Vector3 spawnPosition = summoner.transform.position + (Vector3)randomOffset;

                GameObject minionInstance = await poolManager.GetAsync(key);
                if (minionInstance != null)
                {
                    minionInstance.transform.position = spawnPosition;

                    MonsterController mc = minionInstance.GetComponent<MonsterController>();
                    if (mc != null)
                    {
                        mc.countsTowardKillGoal = false;
                        mc.Initialize(entry.minionToSummon, summoner.targetTransform);
                    }
                }
            }
        }
    }
}