// 파일 경로: Assets/1.Scripts/Core/PlayerDataManager.cs

using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    // --- 소유 데이터 (Private Fields) ---
    private RuntimePlayerData runtimeData;
    private List<CardInstance> ownedCards;
    private List<CardInstance> equippedCards;
    private List<ArtifactDataSO> ownedArtifacts;

    // --- 이벤트 (Events) ---
    public event Action OnInventoryChanged;
    public event Action OnStatsChanged;
    public event Action<float, float> OnHealthChanged;

    // --- 외부 매니저 참조 ---
    private CardManager cardManager;
    private CharacterStats characterStats;

    private void Awake()
    {
        if (ServiceLocator.IsRegistered<PlayerDataManager>())
        {
            Destroy(gameObject);
            return;
        }
        ServiceLocator.Register<PlayerDataManager>(this);
        DontDestroyOnLoad(gameObject);

        runtimeData = new RuntimePlayerData();
        ownedCards = new List<CardInstance>();
        equippedCards = new List<CardInstance>();
        ownedArtifacts = new List<ArtifactDataSO>();

        Debug.Log("[PlayerDataManager] 초기화 완료.");
    }

    private void OnDestroy()
    {
        UnlinkManagers(); // 게임 종료 시 메모리 누수 방지를 위해 구독 해제
    }

    // PlayerInitializer가 호출해 줄 초기 설정 및 연결 메서드
    public void LinkManagers(CardManager cm, CharacterStats cs)
    {
        UnlinkManagers(); // 연결 전, 기존 연결이 있다면 깨끗하게 정리

        this.cardManager = cm;
        this.characterStats = cs;

        // 다른 매니저들의 '방송'을 '구독' 시작
        if (cardManager != null) cardManager.OnInventoryChanged += HandleInventoryChanged;
        if (characterStats != null)
        {
            characterStats.OnFinalStatsCalculated.AddListener(HandleStatsChanged);
            characterStats.OnHealthChanged += HandleHealthChanged;
        }

        Debug.Log("[PlayerDataManager] CardManager 및 CharacterStats 이벤트 연결 완료.");

        // 연결 직후, 현재 상태를 한 번 동기화
        if (cs != null)
        {
            HandleInventoryChanged();
            HandleStatsChanged();
            HandleHealthChanged(cs.currentHealth, cs.FinalHealth);
        }
    }

    // 모든 이벤트 구독을 안전하게 해제하는 메서드
    private void UnlinkManagers()
    {
        if (cardManager != null) cardManager.OnInventoryChanged -= HandleInventoryChanged;
        if (characterStats != null)
        {
            characterStats.OnFinalStatsCalculated.RemoveListener(HandleStatsChanged);
            characterStats.OnHealthChanged -= HandleHealthChanged;
        }
    }

    // --- 이벤트 핸들러 (방송 수신 및 중앙 이벤트 발행) ---
    private void HandleInventoryChanged()
    {
        if (cardManager == null || characterStats == null) return;

        // CardManager로부터 최신 목록을 가져와 내부 데이터를 갱신
        this.ownedCards = cardManager.ownedCards;
        this.equippedCards = cardManager.equippedCards;

        // 중앙 이벤트 '발행'
        OnInventoryChanged?.Invoke();

        // 인벤토리가 바뀌면 스탯도 바뀌므로, 스탯 재계산을 '요청'
        characterStats.CalculateFinalStats();
    }

    private void HandleStatsChanged()
    {
        if (characterStats == null) return;

        // CharacterStats로부터 최신 계산 결과를 가져와 runtimeData에 '캐시'
        runtimeData.FinalDamageBonus = characterStats.FinalDamageBonus;
        runtimeData.FinalAttackSpeed = characterStats.FinalAttackSpeed;
        runtimeData.FinalMoveSpeed = characterStats.FinalMoveSpeed;
        runtimeData.FinalHealth = characterStats.FinalHealth;
        runtimeData.FinalCritRate = characterStats.FinalCritRate;
        runtimeData.FinalCritDamage = characterStats.FinalCritDamage;

        // 중앙 이벤트 '발행'
        OnStatsChanged?.Invoke();
    }

    private void HandleHealthChanged(float current, float max)
    {
        // CharacterStats로부터 최신 체력 정보를 가져와 runtimeData에 '캐시'
        runtimeData.CurrentHealth = current;

        // 중앙 이벤트 '발행'
        OnHealthChanged?.Invoke(current, max);
    }

    #region Queries (데이터 조회 API)

    public RuntimePlayerData GetRuntimeData()
    {
        return runtimeData;
    }

    public List<CardInstance> GetOwnedCards()
    {
        return new List<CardInstance>(ownedCards); // 외부 수정을 막기 위해 복사본 반환
    }

    public List<CardInstance> GetEquippedCards()
    {
        return new List<CardInstance>(equippedCards); // 외부 수정을 막기 위해 복사본 반환
    }

    #endregion
}