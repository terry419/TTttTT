// ���� ���: Assets/1.Scripts/Gameplay/BossStats.cs

using UnityEngine;

// EntityStats�� ��ӹ޾� ���� ���� ����� �����մϴ�.
public class BossStats : EntityStats
{
    // Die() �߻� �޼��带 ������ �°� �����մϴ�.
    protected override void Die()
    {
        Debug.Log($"[BossStats] ����({gameObject.name})�� ����߽��ϴ�.");
        // ���� ���⿡ ���� ��� ���� ����(��: ���� �¸�)�� �߰��� �� �ֽ��ϴ�.
        gameObject.SetActive(false);
    }
}