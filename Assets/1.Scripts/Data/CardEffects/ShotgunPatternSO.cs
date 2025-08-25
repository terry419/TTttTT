using UnityEngine;

[CreateAssetMenu(fileName = "Pattern_Shotgun_", menuName = "GameData/Card Effects/Shotgun Pattern")]
public class ShotgunPatternSO : CardEffectSO
{
    [Header("샷건 패턴 설정")]
    [Tooltip("총 발사할 투사체 개수")]
    public int projectileCount;
    [Tooltip("투사체가 퍼질 각도")]
    public float spreadAngle;
    [Tooltip("true이면 샷건의 여러 발 중 한 발만 동일한 적에게 피해를 줍니다.")]
    public bool isSingleHit = true;
    
    public override void Execute(EffectContext context)
    {
        // 이 로직은 6단계(EffectExecutor)에서 최종 구현됩니다.
        Debug.Log($"<color=lime>[ShotgunPattern]</color> '{this.name}' 실행. (로직 구현 대기중)");
    }
}