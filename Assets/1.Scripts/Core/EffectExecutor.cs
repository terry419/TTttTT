using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Dictionary 사용을 위해 추가
using System;

/// <summary>
/// 카드의 행동/능력 효과를 실행하는 중앙 클래스입니다. (플레이어 전용)
/// 전달된 카드 데이터를 기반으로 각 CardEffectType에 맞는 핸들러를 찾아 실행합니다。
/// </summary>
public class EffectExecutor : MonoBehaviour
{
    private Dictionary<CardEffectType, ICardEffectHandler> effectHandlers;

    void Awake()
    {
        Debug.Log($"[생명주기] {GetType().Name} (ID: {gameObject.GetInstanceID()}) - Awake() 시작. (프레임: {Time.frameCount})");
        ServiceLocator.Register<EffectExecutor>(this);
        DontDestroyOnLoad(gameObject);
        InitializeHandlers(); // 핸들러 초기화
    }

    /// <summary>
    /// 카드 효과 타입과 핸들러 클래스를 매핑하여 딕셔너리를 초기화합니다.
    /// 새로운 효과를 추가할 때 이 곳에 추가하면 됩니다.
    /// </summary>
    private void InitializeHandlers()
    {
        effectHandlers = new Dictionary<CardEffectType, ICardEffectHandler>
        {
            { CardEffectType.SingleShot, new SingleShotHandler() },
            { CardEffectType.SplitShot, new SplitShotHandler() },
            { CardEffectType.Wave, new WaveHandler() }
            // 예: { CardEffectType.Lightning, new LightningHandler() }
        };
    }

    /// <summary>
    /// 카드의 효과를 실행합니다. (플레이어 자신으로부터 발동)
    /// </summary>
    /// <param name="cardData">실행할 카드의 데이터</param>
    /// <param name="actualDamageDealt">실제로 가한 데미지 (OnHit 효과 계산용)</param>
    public void Execute(CardDataSO cardData, float actualDamageDealt = 0f)
    {
        PlayerController currentPC = PlayerController.Instance;
        CharacterStats currentPS = null;
        if (currentPC != null)
        {
            currentPS = currentPC.GetComponent<CharacterStats>();
        }

        if (cardData == null || currentPC == null || currentPS == null)
        {
            Debug.LogError("[EffectExecutor] 필수 컴포넌트(CardData, PlayerController, PlayerStats) 중 하나가 null입니다! " +
                           $"CardData: {(cardData == null ? "NULL" : "OK")}, " +
                           $"PlayerController: {(currentPC == null ? "NULL" : "OK")}, " +
                           $"PlayerStats: {(currentPS == null ? "NULL" : "OK")}");
            return;
        }

        if (cardData.triggerType == TriggerType.OnHit && cardData.lifestealPercentage > 0 && actualDamageDealt > 0)
        {
            currentPC.Heal(actualDamageDealt * cardData.lifestealPercentage);
        }

        Execute(cardData, currentPC.firePoint);
    }

    /// <summary>
    /// 특정 위치에서 카드의 효과를 실행합니다. (예: 몬스터 위치에서 2차 효과 발동 등)
    /// </summary>
    /// <param name="cardData">실행할 카드의 데이터</param>
    /// <param name="spawnPoint">효과가 발동될 위치 (Transform)</param>
    public void Execute(CardDataSO cardData, Transform spawnPoint)
    {
        if (cardData == null) return;

        if (effectHandlers.TryGetValue(cardData.effectType, out ICardEffectHandler handler))
        {
            handler.Execute(cardData, this, spawnPoint);
        }
        else
        {
            Debug.LogError($"[EffectExecutor] '{cardData.effectType}' 타입에 대한 핸들러가 등록되어 있지 않습니다!");
        }
    }

    /// <summary>
    /// 플레이어의 현재 능력치와 카드 데이터를 사용하여 총 데미지를 계산합니다.
    /// </summary>
    public float CalculateTotalDamage(CardDataSO cardData)
    {
        CharacterStats currentPS = PlayerController.Instance?.GetComponent<CharacterStats>();
        if (currentPS == null)
        {
            Debug.LogError("[EffectExecutor] CalculateTotalDamage: PlayerStats를 찾을 수 없습니다!");
            return 0f;
        }

        // 카드의 기본 대미지가 0 이하라면 공격용 카드가 아니므로 0을 반환합니다.
        if (cardData.baseDamage <= 0)
        {
            return 0f;
        }

        // 제안하신 공식의 (1 + 가중치 합) 부분을 계산합니다.
        // 1. 카드로 인한 스탯 증가 가중치 합 (모든 장착 카드)
        float cardBonus = currentPS.cardDamageRatio;

        // 2. 유물로 인한 스탯 증가 가중치 합
        float artifactBonus = currentPS.artifactDamageRatio;

        // 3. 유전자 증폭제(영구 스탯)로 인한 스탯 증가 가중치 합
        float boosterBonus = currentPS.boosterDamageRatio;

        // 4. 버프/디버프로 인한 스탯 증가 가중치 합
        float buffBonus = currentPS.buffDamageRatio;

        // 모든 가중치를 합산합니다.
        float totalBonusRatio = cardBonus + artifactBonus + boosterBonus + buffBonus;

        // 최종 대미지 = 카드의 기본 대미지 * (1 + 모든 보너스 가중치의 합)
        float finalDamage = cardData.baseDamage * (1 + totalBonusRatio);

        return finalDamage;
    }

    /// <summary>
    /// 타겟팅 타입에 따라 목표물을 찾아 발사 각도를 계산합니다。
    /// </summary>
    public float GetTargetingAngle(TargetingType targetingType)
    {
        PlayerController currentPC = PlayerController.Instance;
        if (currentPC == null)
        {
            Debug.LogError("[EffectExecutor] GetTargetingAngle: PlayerController를 찾을 수 없습니다!");
            return 0f;
        }

        Transform target = TargetingSystem.FindTarget(targetingType, currentPC.transform);

        if (target != null)
        {
            Vector2 directionToTarget = (target.position - currentPC.firePoint.position).normalized;
            return Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
        }
        else
        {
            return currentPC.firePoint.eulerAngles.z;
        }
    }

    private void OnDestroy()
    {
        Debug.Log($"[생명주기] {GetType().Name} (ID: {gameObject.GetInstanceID()}) - OnDestroy() 시작. (프레임: {Time.frameCount})");
    }
}
