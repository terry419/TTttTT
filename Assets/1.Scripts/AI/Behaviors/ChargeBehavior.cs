// 경로: ./TTttTT/Assets/1/Scripts/AI/Behaviors/ChargeBehavior.cs
using UnityEngine;

/// <summary>
/// [고급 행동 부품 - 4단계 최종본] 사장님의 모든 피드백을 반영한 최종 버전입니다.
/// </summary>
[CreateAssetMenu(menuName = "Monster AI/Behaviors/Charge")]
public class ChargeBehavior : MonsterBehavior
{
    private enum State { Aiming, ChargeWindup, Charging }

    [Header("조준 단계")]
    public float aimingDuration = 1.0f;

    [Header("준비 단계")]
    public float windupDuration = 0.5f;
    [Tooltip("준비 단계에서의 이동 속도 배율입니다.")]
    public float windupSpeedMultiplier = 0.3f;

    [Header("돌진 단계")]
    [Tooltip("배율이 아닌, 최종 돌진 '속도'를 직접 입력합니다.")]
    public float chargeSpeed = 15f;
    [Tooltip("최대 돌진 거리입니다.")]
    public float chargeDistance = 5f;
    [Header("예측 설정")]
    [Tooltip("플레이어의 이동을 얼마나 먼 미래까지 예측할지 결정합니다. (0 = 예측 안함, 1 = 1초 뒤를 예측)")]
    [Range(0f, 2f)]
    public float chargePlayerPredictionFactor = 0.5f;

    public override void OnEnter(MonsterController monster)
    {
        monster.chargeState = (int)State.Aiming;
        monster.rb.velocity = Vector2.zero;
        monster.chargeDistanceRemaining = chargeDistance;
        Log.Info(Log.LogCategory.AI_Behavior, $"{monster.name}이(가) 돌진을 시작합니다. (조준 단계 진입)");
    }

    public override void OnExit(MonsterController monster)
    {
        // 어떤 이유로든 이 행동이 중단되면(다른 행동으로 전환되면),
        // 몬스터의 레이어를 원래대로 안전하게 되돌려놓습니다.
        monster.ResetLayer();
    }

    public override void OnExecute(MonsterController monster)
    {
        State currentState = (State)monster.chargeState;

        switch (currentState)
        {
            case State.Aiming:
                monster.rb.velocity = Vector2.zero;
                if (monster.stateTimer >= aimingDuration)
                {
                    monster.chargeState = (int)State.ChargeWindup;
                    Log.Info(Log.LogCategory.AI_Behavior, $"{monster.name} 조준 완료. (준비 단계 진입)");
                }
                break;

            case State.ChargeWindup:
                if (monster.playerTransform != null)
                {
                    Vector2 direction = (monster.playerTransform.position - monster.transform.position).normalized;
                    monster.rb.velocity = direction * monster.monsterStats.FinalMoveSpeed * windupSpeedMultiplier;
                }
                if (monster.stateTimer >= aimingDuration + windupDuration)
                {
                    monster.chargeState = (int)State.Charging;
                    if (monster.playerTransform != null)
                    {
                        Vector2 playerVelocity = monster.playerRigidbody != null ? monster.playerRigidbody.velocity : Vector2.zero;

                        // 2. 예측 계수를 곱해 '미래 예상 위치'를 계산합니다.
                        Vector3 predictedPosition = monster.playerTransform.position + (Vector3)playerVelocity * chargePlayerPredictionFactor;

                        // 3. 현재 내 위치에서 '미래 예상 위치'를 향하는 방향을 최종 돌진 방향으로 결정합니다.
                        monster.chargeDirection = (predictedPosition - monster.transform.position).normalized;

                        Log.Info(Log.LogCategory.AI_Behavior, $"{monster.name}이(가) 플레이어의 예상 위치({predictedPosition.x:F1}, {predictedPosition.y:F1})를 향해 돌진을 준비합니다.");
                    }
                    else
                    {
                        monster.chargeDirection = monster.transform.right;
                    }

                    // 돌진 시작 직전, 자신을 '유령'으로 만듭니다.
                    monster.SetLayer("ChargingMonster");
                    Log.Info(Log.LogCategory.AI_Behavior, $"{monster.name} 준비 완료. (돌진 단계 진입!)");
                }
                break;

            case State.Charging:
                // Inspector에서 입력한 chargeSpeed 값을 그대로 사용합니다.
                monster.rb.velocity = monster.chargeDirection * this.chargeSpeed;
                monster.chargeDistanceRemaining -= this.chargeSpeed * 0.2f;

                if (monster.chargeDistanceRemaining <= 0)
                {
                    Log.Info(Log.LogCategory.AI_Behavior, $"{monster.name} 돌진 완료.");
                    monster.rb.velocity = Vector2.zero;

                    // 돌진이 끝났으니, 다음 행동으로 넘어갈지 검사합니다.
                    CheckTransitions(monster);
                }
                break;
        }
    }
}