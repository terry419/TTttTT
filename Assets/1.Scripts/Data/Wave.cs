// --- ���ϸ�: Wave.cs ---
using UnityEngine;

// [�߰�] ���� ����� �����ϴ� enum. �� �κ��� ��� ù ��° ������ �߻��߾�.
public enum SpawnType
{
    Spread, // ������ �ð� ���� ������ ����
    Burst   // �� ���� �͸��� ����
}

[System.Serializable]
public class Wave
{
    [Tooltip("�� ���̺꿡�� ������ ������ ������(SO)�� ���� ���⿡ �����ϼ���.")]
    // [����] string ��� MonsterDataSO�� ���� �����մϴ�.
    public MonsterDataSO monsterData;

    [Tooltip("������ ������ ��")]
    public int count;

    [Tooltip("SpawnType�� Spread�� ��, ù ���ͺ��� ������ ���ͱ��� �����Ǵ� �� �ɸ��� �� �ð��Դϴ�.")]
    public float duration = 10f;

    [Tooltip("�� ���̺갡 ���� �� ���� ���̺갡 ���۵Ǳ������ ��� �ð��Դϴ�.")]
    public float delayAfterWave;

    [Tooltip("���� ��� (Spread: �ð���, Burst: ����)")]
    public SpawnType spawnType;
}