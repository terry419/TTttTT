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

    private void Awake()
    {
        // ServiceLocator를 통한 싱글톤 및 DontDestroyOnLoad 설정
        if (ServiceLocator.IsRegistered<PlayerDataManager>())
        {
            Destroy(gameObject);
            return;
        }
        ServiceLocator.Register<PlayerDataManager>(this);
        DontDestroyOnLoad(gameObject);

        // 데이터 컨테이너 초기화
        runtimeData = new RuntimePlayerData();
        ownedCards = new List<CardInstance>();
        equippedCards = new List<CardInstance>();
        ownedArtifacts = new List<ArtifactDataSO>();

        Debug.Log("[PlayerDataManager] 초기화 완료.");
    }

    // --- 주요 API (Public Methods) ---

    #region Commands (데이터 변경)
    public void AcquireCard(NewCardDataSO cardData)
    {
        Debug.Log($"[PlayerDataManager] AcquireCard: {cardData.name}");
        // TODO: 카드 추가 로직 구현
    }

    public void EquipCard(CardInstance card)
    {
        Debug.Log($"[PlayerDataManager] EquipCard: {card.CardData.name}");
        // TODO: 카드 장착 로직 구현
    }

    // ... Unequip, Swap, AddArtifact, ApplyDamage, Heal 등 다른 Command 메서드들도 필요에 따라 추가 ...

    #endregion

    #region Queries (데이터 조회)

    public RuntimePlayerData GetRuntimeData()
    {
        return runtimeData;
    }

    public List<CardInstance> GetOwnedCards()
    {
        return new List<CardInstance>(ownedCards); // 복사본 반환
    }

    public List<CardInstance> GetEquippedCards()
    {
        return new List<CardInstance>(equippedCards); // 복사본 반환
    }

    #endregion
}