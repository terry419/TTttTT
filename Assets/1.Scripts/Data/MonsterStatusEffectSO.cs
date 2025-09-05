// ���: ./TTttTT/Assets/1/Scripts/Data/MonsterStatusEffectSO.cs (�ű� ����)
using UnityEngine;

[CreateAssetMenu(fileName = "MSE_", menuName = "Monster AI/Monster Status Effect")]
public class MonsterStatusEffectSO : ScriptableObject
{
    [Header("�⺻ ����")]
    public string effectId;

    [Header("ȿ�� �Ӽ�")]
    public float duration;

    [Header("�ɷ�ġ ���� ȿ�� (%)")]
    public float moveSpeedBonus;
    public float contactDamageBonus;
    public float damageTakenBonus;
    // ... ���� ���Ϳ��� �ʿ��� ������ ����� ���⿡ �߰��մϴ� ...

    [Header("���� ����/ȸ�� ȿ��")]
    public float damageOverTime;
    public float healOverTime;
}