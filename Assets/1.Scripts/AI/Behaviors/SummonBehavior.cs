// 경로: ./TTttTT/Assets/1/Scripts/AI/Behaviors/SummonBehavior.cs
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System; // Serializable을 위해 추가

/// <summary>
/// [수정] 소환 목록의 각 항목을 정의하는 데이터 구조입니다.
/// MonsterDataSO와 summonCount를 하나의 쌍으로 묶습니다.
/// </summary>
[Serializable]
public class SummonEntry
{
    [Tooltip("소환할 하수인의 MonsterDataSO 에셋입니다.")]
    public MonsterDataSO minionToSummon;
    [Tooltip("이 하수인을 몇 마리 소환할지 정합니다.")]
    public int summonCount = 1;
}

/// <summary>
/// [고급 행동 부품 - 다중 동시 소환 최종본] 
/// 목록에 있는 모든 종류의 하수인을 각자 지정된 수만큼 동시에 소환하는 행동입니다.
/// </summary>
[CreateAssetMenu(menuName = "Monster AI/Behaviors/Summon")]
public class SummonBehavior : MonsterBehavior
{
    [Header("소환 설정")]
    [Tooltip("소환 동작(캐스팅)에 걸리는 시간(초)입니다.")]
    public float summonCastTime = 1.5f;

    [Header("소환 목록")]
    [Tooltip("이 행동으로 소환될 모든 하수인들의 목록입니다.")]
    public List<SummonEntry> summonList;

    public override void OnEnter(MonsterController monster)
    {
        base.OnEnter(monster);
        monster.rb.velocity = Vector2.zero;
        Log.Info(Log.LogCategory.AI_Behavior, $"{monster.name}이(가) 하수인 소환을 시작합니다. (캐스팅 시간: {summonCastTime}초)");
    }

    public override void OnExecute(MonsterController monster)
    {
        monster.rb.velocity = Vector2.zero;

        if (monster.stateTimer >= summonCastTime)
        {
            Log.Info(Log.LogCategory.AI_Behavior, $"{monster.name}이(가) 소환 목록에 따라 하수인을 소환합니다!");
            SummonMinionsAsync(monster).Forget();
            CheckTransitions(monster);
        }
    }

    private async UniTaskVoid SummonMinionsAsync(MonsterController summoner)
    {
        if (summonList == null || summonList.Count == 0)
        {
            Log.Error(Log.LogCategory.AI_Behavior, $"'{summoner.name}'이(가) 소환할 하수인 목록(Summon List)이 비어있습니다.");
            return;
        }

        var poolManager = ServiceLocator.Get<PoolManager>();
        if (poolManager == null) return;

        // 목록에 있는 모든 항목(Entry)에 대해 반복합니다.
        foreach (var entry in summonList)
        {
            if (entry.minionToSummon == null || !entry.minionToSummon.prefabRef.RuntimeKeyIsValid())
            {
                Log.Warn(Log.LogCategory.AI_Behavior, $"소환 목록의 항목 중 하나가 유효하지 않아 건너뜁니다.");
                continue;
            }

            // 각 항목에 지정된 summonCount 만큼 반복합니다.
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