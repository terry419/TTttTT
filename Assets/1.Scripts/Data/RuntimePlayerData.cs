// 파일 경로: Assets/1.Scripts/Data/RuntimePlayerData.cs

[System.Serializable]
public class RuntimePlayerData
{
    // 최종 스탯
    public float FinalDamageBonus { get; internal set; }
    public float FinalAttackSpeed { get; internal set; }
    public float FinalMoveSpeed { get; internal set; }
    public float FinalHealth { get; internal set; }
    public float FinalCritRate { get; internal set; }
    public float FinalCritDamage { get; internal set; }

    // 현재 체력
    public float CurrentHealth { get; internal set; }
}