// 경로: ./TTttTT/Assets/1/Scripts/AI/Behaviors/SummonBehavior.cs
using UnityEngine;
using Cysharp.Threading.Tasks;

/// <summary>
/// [고급 행동 부품] 지정된 시간 동안 캐스팅 후, 하수인을 소환하는 행동입니다.
/// </summary>
[CreateAssetMenu(menuName = "Monster AI/Behaviors/Summon")]
public class SummonBehavior : MonsterBehavior
{
    [Header("소환 설정")]
    [Tooltip("소환할 하수인의 MonsterDataSO 에셋을 연결합니다.")]
    public MonsterDataSO minionToSummon;
    [Tooltip("한 번에 소환할 하수인의 수입니다.")]
    public int summonCount = 3;
    [Tooltip("소환 동작(캐스팅)에 걸리는 시간(초)입니다.")]
    public float summonCastTime = 1.5f;

    public override void OnEnter(MonsterController monster)
    {
        // 소환을 시작하면, 그 자리에 멈춥니다.
        monster.rb.velocity = Vector2.zero;
        Log.Info(Log.LogCategory.AI_Behavior, $"{monster.name}이(가) 하수인 소환을 시작합니다. (캐스팅 시간: {summonCastTime}초)");
    }

    public override void OnExecute(MonsterController monster)
    {
        // 캐스팅 시간 동안에는 계속 제자리에 멈춰있습니다.
        monster.rb.velocity = Vector2.zero;

        // 캐스팅 시간이 다 지났는지 확인합니다.
        if (monster.stateTimer >= summonCastTime)
        {
            // 시간이 다 되면, 소환을 실행하고 다음 행동으로 넘어갑니다.
            Log.Info(Log.LogCategory.AI_Behavior, $"{monster.name}이(가) {minionToSummon.monsterName} {summonCount}마리를 소환합니다!");

            // 비동기 작업이므로, UniTask의 'Forget'으로 처리합니다.
            SummonMinionsAsync(monster).Forget();

            // 소환을 마쳤으니, 다음 행동으로 전환될지 검사합니다.
            CheckTransitions(monster);
        }
    }

    private async UniTaskVoid SummonMinionsAsync(MonsterController summoner)
    {
        if (minionToSummon == null || !minionToSummon.prefabRef.RuntimeKeyIsValid())
        {
            Log.Error(Log.LogCategory.AI_Behavior, $"'{summoner.name}'이(가) 소환하려는 하수인({minionToSummon?.name})의 데이터가 유효하지 않습니다.");
            return;
        }

        var poolManager = ServiceLocator.Get<PoolManager>();
        if (poolManager == null) return;

        string key = minionToSummon.prefabRef.AssetGUID;

        for (int i = 0; i < summonCount; i++)
        {
            // 소환사 주변의 무작위 위치를 계산합니다.
            Vector2 randomOffset = Random.insideUnitCircle * 2.0f; // 2미터 반경 내
            Vector3 spawnPosition = summoner.transform.position + (Vector3)randomOffset;

            // 풀 매니저를 통해 하수인 인스턴스를 가져옵니다.
            GameObject minionInstance = await poolManager.GetAsync(key);
            if (minionInstance != null)
            {
                minionInstance.transform.position = spawnPosition;

                // 하수인 몬스터를 초기화합니다.
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