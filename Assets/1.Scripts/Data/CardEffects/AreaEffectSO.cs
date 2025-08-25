using UnityEngine;

[CreateAssetMenu(fileName = "AreaEffect_", menuName = "GameData/Card Effects/Area Effect")]
public class AreaEffectSO : CardEffectSO
{
    [Header("광역 효과 설정")]
    [Tooltip("생성할 장판/파동의 프리팹")]
    public GameObject effectPrefab;

    [Tooltip("이 효과가 단일 피해 파동(true)인지, 지속 피해 장판(false)인지 결정")]
    public bool isSingleHitWaveMode = true;

    [Header("단일 파동 설정")]
    [Tooltip("파동 모드일 때의 단일 피해량")]
    public float singleHitDamage = 50f;

    [Header("지속 장판 설정")]
    [Tooltip("장판의 총 지속 시간")]
    public float duration = 3f;
    [Tooltip("장판의 확장 속도")]
    public float expansionSpeed = 1f;
    [Tooltip("장판이 최대 크기까지 확장되는 데 걸리는 시간")]
    public float expansionDuration = 0.5f;
    [Tooltip("장판의 틱당 피해량")]
    public float damagePerTick = 5f;
    [Tooltip("피해를 주는 간격(초)")]
    public float tickInterval = 1f;
    
    public override void Execute(EffectContext context)
    {
        // 이 로직은 6단계(EffectExecutor)에서 최종 구현됩니다.
        Debug.Log($"<color=lime>[AreaEffect]</color> '{this.name}' 실행. (로직 구현 대기중)");
    }
}