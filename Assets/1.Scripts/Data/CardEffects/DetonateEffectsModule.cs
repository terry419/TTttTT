using UnityEngine;

using System.Linq;



/// <summary>

/// [v2.2] 대상에게 걸린 지속 피해(DoT) 효과들을 즉시 폭발시켜 큰 피해를 주는 특수 모듈입니다.

/// </summary>

[CreateAssetMenu(fileName = "Module_DetonateEffects_", menuName = "GameData/v8.0/Modules/DetonateEffects")]

public class DetonateEffectsModule : CardEffectSO

{

    [Header("[ 폭발 설정 ]")]

    [Tooltip("폭발시킬 DoT 효과의 남은 피해량 총합에 곱해질 피해 배율 (%). 100 입력 시 남은 피해량 그대로, 200 입력 시 2배의 피해를 줍니다.")]

    public float DamageMultiplier = 100f;



    [Tooltip("체크 시, 폭발시킨 DoT 효과들을 대상에게서 즉시 제거합니다.")]

    public bool ConsumeEffects = true;



    [Tooltip("특정 ID를 가진 DoT 효과만 폭발시킵니다. 비워두면 모든 종류의 DoT 효과를 대상으로 합니다.")]

    public string StatusEffectIDFilter;



    public override void Execute(EffectContext context)

    {

        // 1. 유효성 검사: 피격 대상이 없으면 실행할 수 없습니다.

        if (context.HitTarget == null)

        {

            Debug.LogWarning($"<color=yellow>[{GetType().Name}]</color> '{this.name}' 실행 중단: HitTarget이 없어 효과를 폭발시킬 대상이 없습니다.");

            return;

        }



        Debug.Log($"<color=lime>[{GetType().Name}]</color> '{this.name}' 실행 시도. 대상: {context.HitTarget.name}");



        var statusManager = ServiceLocator.Get<StatusEffectManager>();

        if (statusManager == null)

        {

            Debug.LogError($"<color=red>[{GetType().Name}]</color> StatusEffectManager를 찾을 수 없어 실행을 중단합니다.");

            return;

        }



        // 2. 대상에게 걸린 모든 활성 효과를 가져옵니다.

        var allEffects = statusManager.GetActiveEffectsOn(context.HitTarget.gameObject);



        // 3. 폭발시킬 효과 필터링:

        // - 지속 피해(DoT) 효과만 대상으로 하고 (DamagePerSecond > 0)

        // - StatusEffectIDFilter가 비어있거나, ID가 일치하는 효과만 골라냅니다.

        var effectsToDetonate = allEffects.Where(e =>

      e.DamagePerSecond > 0 &&

      (string.IsNullOrEmpty(StatusEffectIDFilter) || e.EffectId == StatusEffectIDFilter)

    ).ToList();



        // 폭발시킬 효과가 없으면 로그를 남기고 종료합니다.

        if (effectsToDetonate.Count == 0)

        {

            Debug.Log($"[{GetType().Name}] '{context.HitTarget.name}'에게서 폭발시킬 DoT 효과(ID: {StatusEffectIDFilter})를 찾지 못했습니다.");

            return;

        }



        // 4. 최종 피해량 계산:

        // 각 효과의 (초당 피해량 * 남은 시간)을 모두 더하여 남은 잠재적 피해량 총합을 구합니다.

        float potentialDamage = effectsToDetonate.Sum(e => e.DamagePerSecond * e.RemainingDuration);

        // 여기에 최종 배율을 곱합니다.

        float finalDamage = potentialDamage * (DamageMultiplier / 100f);



        // 5. 피해 적용 및 로그 출력

        context.HitTarget.TakeDamage(finalDamage);

        Debug.Log($"<color=red>[{GetType().Name}]</color> '{context.HitTarget.name}'에게 {effectsToDetonate.Count}개의 DoT를 폭발시켜 <b>{finalDamage:F1}</b>의 피해를 입혔습니다! (원본 피해량: {potentialDamage:F1})");



        // 6. 효과 소모 (옵션)

        if (ConsumeEffects)

        {

            statusManager.ConsumeEffects(effectsToDetonate);

            Debug.Log($"<color=yellow>[{GetType().Name}]</color> 대상의 DoT 효과 {effectsToDetonate.Count}개를 소모했습니다.");

        }

    }

}