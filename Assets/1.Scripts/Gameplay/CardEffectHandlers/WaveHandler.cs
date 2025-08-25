using UnityEngine;
using System;

/// <summary>
/// 'Wave' 타입의 카드 효과(파동, 장판 등)를 처리하는 클래스입니다.
/// </summary>
public class WaveHandler : ICardEffectHandler
{
    public void Execute(CardDataSO cardData, EffectExecutor executor, CharacterStats casterStats, Transform spawnPoint)
    {
        GameObject wavePrefab = cardData.effectPrefab;
        if (wavePrefab == null)
        { 
            Debug.LogError($"[WaveHandler] 오류: 웨이브 카드 '{cardData.cardName}'에 effectPrefab이 할당되지 않았습니다!");
            return;
        }

        // 풀 매니저에서 웨이브 오브젝트를 가져옵니다.
        GameObject waveGO = ServiceLocator.Get<PoolManager>().Get(wavePrefab);

        if (waveGO == null) return;

        // 효과 생성 위치를 지정합니다.
        waveGO.transform.position = spawnPoint.position;

        if (waveGO.TryGetComponent<DamagingZone>(out var zone))
        {
            float totalDamage = executor.CalculateTotalDamage(cardData, casterStats);
            string shotID = Guid.NewGuid().ToString();

            Debug.Log($"[디버그 최종 확인] DamagingZone 초기화 정보:" +
              $"\n - 카드 이름: <color=yellow>{cardData.name}</color>" +
              $"\n - 지속 대미지(effectDamagePerTick): <color=red>{cardData.effectDamagePerTick}</color>" +
              $"\n - 단일 대미지(baseDamage): {cardData.baseDamage}" +
              $"\n - 장판 모드(isSingleHitWaveMode): {!cardData.isEffectSingleHitWaveMode}");


            // DamagingZone을 카드 데이터에 맞게 초기화합니다.
            zone.Initialize(
                singleHitDmg: totalDamage,
                continuousDmgPerTick: cardData.effectDamagePerTick,
                tickInt: cardData.effectTickInterval,
                totalDur: cardData.effectDuration,
                expSpeed: cardData.effectExpansionSpeed,
                expDur: cardData.effectExpansionDuration,
                isWave: cardData.isEffectSingleHitWaveMode,
                shotID: shotID
            );
        }
        else
        {
            Debug.LogError($"[WaveHandler] 오류: '{wavePrefab.name}' 프리팹에 DamagingZone.cs 스크립트가 없습니다!");
        }
    }
}