// --- ���� ��ġ: Assets/1.Scripts/Gameplay/CardEffectHandlers/WaveHandler.cs ---

using UnityEngine;
using System;

/// <summary>
/// 'Wave' Ÿ���� ī�� ȿ��(�ĵ�, ���� ��)�� ó���ϴ� Ŭ�����Դϴ�.
/// </summary>
public class WaveHandler : ICardEffectHandler
{
    public void Execute(CardDataSO cardData, EffectExecutor executor, Transform spawnPoint)
    {
        GameObject wavePrefab = cardData.effectPrefab;
        if (wavePrefab == null)
        {
            Debug.LogError($"[WaveHandler] ����: �ĵ� ī�� '{cardData.cardName}'�� effectPrefab�� �Ҵ���� �ʾҽ��ϴ�!");
            return;
        }

        // Ǯ �Ŵ������� �ĵ� ������Ʈ�� �����ɴϴ�.
        GameObject waveGO = executor.poolManager.Get(wavePrefab);
        if (waveGO == null) return;

        // ȿ�� ���� ��ġ�� �����մϴ�.
        waveGO.transform.position = spawnPoint.position;

        if (waveGO.TryGetComponent<DamagingZone>(out var zone))
        {
            float totalDamage = executor.CalculateTotalDamage(cardData);
            string shotID = Guid.NewGuid().ToString();

            // DamagingZone�� ī�� �����Ϳ� �°� �ʱ�ȭ�մϴ�.
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
            Debug.LogError($"[WaveHandler] ����: '{wavePrefab.name}' �����տ� DamagingZone.cs ��ũ��Ʈ�� �����ϴ�!");
        }
    }
}
