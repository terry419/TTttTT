// ���ϸ�: MonsterDataSO.cs
// ���: Assets/1.Scripts/Data/MonsterDataSO.cs
using UnityEngine;

/// <summary>
/// ���� �� ������ ��� �����͸� �����ϴ� ScriptableObject�Դϴ�.
/// ü��, �ӵ�, ���ݷ�, ������ �� ������ ��� �Ӽ��� �� ���� �ϳ��� ������ �� �ֽ��ϴ�.
/// </summary>
[CreateAssetMenu(fileName = "MonsterData_", menuName = "GameData/MonsterData")]
public class MonsterDataSO : ScriptableObject
{
    [Header("�⺻ ����")]
    [Tooltip("�����͸� ã�� ���� ���� ID�Դϴ�. (��: slime_normal, goblin_archer)")]
    public string monsterID;

    [Tooltip("���� ���� ǥ�õ� �̸��Դϴ�.")]
    public string monsterName;

    [Header("�ɷ�ġ")]
    public float maxHealth = 100f;
    public float moveSpeed = 3f;
    public float contactDamage = 10f;

    [Header("����")]
    [Tooltip("�� ���Ͱ� ����� �������� ���� �����ϼ���.")]
    public GameObject prefab; // [����] string���� �ٽ� GameObject�� ����

    // [Ȯ�� ����] ��ȹ���� ��޵� �پ��� ���� �ൿ ������ ���� ������
    // public enum MonsterBehaviorType { Chase, Flee, Patrol, ExplodeOnDeath }
    // public MonsterBehaviorType behaviorType;
}