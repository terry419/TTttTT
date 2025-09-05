// ���: ./TTttTT/Assets/1/Scripts/AI/Behaviors/ChargeBehavior.cs
using UnityEngine;

/// <summary>
/// [��� �ൿ ��ǰ - 4�ܰ� ������] ������� ��� �ǵ���� �ݿ��� ���� �����Դϴ�.
/// </summary>
[CreateAssetMenu(menuName = "Monster AI/Behaviors/Charge")]
public class ChargeBehavior : MonsterBehavior
{
    private enum State { Aiming, ChargeWindup, Charging }

    [Header("���� �ܰ�")]
    public float aimingDuration = 1.0f;

    [Header("�غ� �ܰ�")]
    public float windupDuration = 0.5f;
    [Tooltip("�غ� �ܰ迡���� �̵� �ӵ� �����Դϴ�.")]
    public float windupSpeedMultiplier = 0.3f;

    [Header("���� �ܰ�")]
    [Tooltip("������ �ƴ�, ���� ���� '�ӵ�'�� ���� �Է��մϴ�.")]
    public float chargeSpeed = 15f;
    [Tooltip("�ִ� ���� �Ÿ��Դϴ�.")]
    public float chargeDistance = 5f;
    [Header("���� ����")]
    [Tooltip("�÷��̾��� �̵��� �󸶳� �� �̷����� �������� �����մϴ�. (0 = ���� ����, 1 = 1�� �ڸ� ����)")]
    [Range(0f, 2f)]
    public float chargePlayerPredictionFactor = 0.5f;

    public override void OnEnter(MonsterController monster)
    {
        monster.chargeState = (int)State.Aiming;
        monster.rb.velocity = Vector2.zero;
        monster.chargeDistanceRemaining = chargeDistance;
        Log.Info(Log.LogCategory.AI_Behavior, $"{monster.name}��(��) ������ �����մϴ�. (���� �ܰ� ����)");
    }

    public override void OnExit(MonsterController monster)
    {
        // � �����ε� �� �ൿ�� �ߴܵǸ�(�ٸ� �ൿ���� ��ȯ�Ǹ�),
        // ������ ���̾ ������� �����ϰ� �ǵ��������ϴ�.
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
                    Log.Info(Log.LogCategory.AI_Behavior, $"{monster.name} ���� �Ϸ�. (�غ� �ܰ� ����)");
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

                        // 2. ���� ����� ���� '�̷� ���� ��ġ'�� ����մϴ�.
                        Vector3 predictedPosition = monster.playerTransform.position + (Vector3)playerVelocity * chargePlayerPredictionFactor;

                        // 3. ���� �� ��ġ���� '�̷� ���� ��ġ'�� ���ϴ� ������ ���� ���� �������� �����մϴ�.
                        monster.chargeDirection = (predictedPosition - monster.transform.position).normalized;

                        Log.Info(Log.LogCategory.AI_Behavior, $"{monster.name}��(��) �÷��̾��� ���� ��ġ({predictedPosition.x:F1}, {predictedPosition.y:F1})�� ���� ������ �غ��մϴ�.");
                    }
                    else
                    {
                        monster.chargeDirection = monster.transform.right;
                    }

                    // ���� ���� ����, �ڽ��� '����'���� ����ϴ�.
                    monster.SetLayer("ChargingMonster");
                    Log.Info(Log.LogCategory.AI_Behavior, $"{monster.name} �غ� �Ϸ�. (���� �ܰ� ����!)");
                }
                break;

            case State.Charging:
                // Inspector���� �Է��� chargeSpeed ���� �״�� ����մϴ�.
                monster.rb.velocity = monster.chargeDirection * this.chargeSpeed;
                monster.chargeDistanceRemaining -= this.chargeSpeed * 0.2f;

                if (monster.chargeDistanceRemaining <= 0)
                {
                    Log.Info(Log.LogCategory.AI_Behavior, $"{monster.name} ���� �Ϸ�.");
                    monster.rb.velocity = Vector2.zero;

                    // ������ ��������, ���� �ൿ���� �Ѿ�� �˻��մϴ�.
                    CheckTransitions(monster);
                }
                break;
        }
    }
}