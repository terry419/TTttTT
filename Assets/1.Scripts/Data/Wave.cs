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
    [Tooltip("�� ���̺��� Ÿ���� �����ϼ���. (Spread: ������, Burst: �� ����)")]
    public SpawnType spawnType; // [�߰�] ���� Ÿ�� ���� �ʵ�. �� �κ��� ��� �� ��° ������ �߻��߾�.

    public string monsterName;
    public int count;

    [Tooltip("SpawnType�� Spread�� ���� ���˴ϴ�. ���͸� ��� �����ϴ� �� �ɸ��� �� �ð��Դϴ�.")]
    public float duration = 10f;

    [Tooltip("�� ���̺갡 ���� �� ���� ���̺갡 ���۵Ǳ������ ��� �ð��Դϴ�.")]
    public float delayAfterWave;
}