// ���� ���: Assets/1.Scripts/Data/RuntimePlayerData.cs

[System.Serializable]
public class RuntimePlayerData
{
    // ���� ����
    public float FinalDamageBonus { get; internal set; }
    public float FinalAttackSpeed { get; internal set; }
    public float FinalMoveSpeed { get; internal set; }
    public float FinalHealth { get; internal set; }
    public float FinalCritRate { get; internal set; }
    public float FinalCritDamage { get; internal set; }

    // ���� ü��
    public float CurrentHealth { get; internal set; }
}