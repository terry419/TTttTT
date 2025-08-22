// --- 파일 위치: Assets/1.Scripts/Gameplay/PlayerInitializer.cs ---

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 게임 시작 시 플레이어 오브젝트를 초기화하는 역할을 담당합니다. (리팩토링 버전)
/// 이 클래스는 필요한 데이터와 컴포넌트를 가져와서, 각 컴포넌트가 스스로를 초기화하도록
/// 메시지를 전달하는 '지휘자(Coordinator)'의 역할에 집중합니다.
/// </summary>
public class PlayerInitializer : MonoBehaviour
{
    [Header("테스트용 시작 카드 목록")]
    [Tooltip("캐릭터 데이터(SO)에 시작 카드가 설정되어 있으면, 이 목록은 무시됩니다.")]
    [SerializeField] private List<CardDataSO> testStartingCards;

    void Start()
    {
        EventSystem.current.SetSelectedGameObject(null);

        // --- 1. 공통 초기화 (매 라운드 실행) ---
        var playerStats = GetComponent<CharacterStats>();
        var playerController = GetComponent<PlayerController>();
        var gameManager = ServiceLocator.Get<GameManager>();
        var playerSpriteRenderer = GetComponent<SpriteRenderer>();

        if (DebugManager.Instance != null)
        {
            DebugManager.Instance.RegisterPlayer(playerStats);
        }

        // GameManager에 저장된 캐릭터 정보를 불러옵니다.
        CharacterDataSO characterToLoad = gameManager.SelectedCharacter ?? ServiceLocator.Get<DataManager>().GetCharacter("warrior");
        if (characterToLoad == null)
        {
            Debug.LogError("CRITICAL: 적용할 캐릭터 데이터를 찾을 수 없습니다!");
            return;
        }

        // 캐릭터의 기본 능력치와 외형을 먼저 적용합니다.
        playerStats.stats = characterToLoad.baseStats;
        playerSpriteRenderer.sprite = characterToLoad.illustration;

        // --- 2. 분기: 첫 라운드 vs 이후 라운드 ---
        if (gameManager.isFirstRound)
        {
            var progressionManager = ProgressionManager.Instance;
            
            // 영구 스탯 및 분배 포인트 적용 (게임 시작 시 한 번만)
            CharacterPermanentStats permanentStats = progressionManager.GetPermanentStatsFor(characterToLoad.characterId);
            playerStats.ApplyPermanentStats(permanentStats);
            playerStats.ApplyAllocatedPoints(gameManager.AllocatedPoints, permanentStats);

            // 시작 아이템 장착 (게임 시작 시 한 번만)
            EquipStartingItems(characterToLoad);
            
            // 첫 라운드가 끝났음을 표시
            gameManager.isFirstRound = false;

            // [신규] CardManager에 플레이어 정보 연결 (첫 라운드 초기화 완료 후)
            if (ServiceLocator.Get<CardManager>() != null)
            {
                ServiceLocator.Get<CardManager>().LinkToNewPlayer(playerStats);
            }
        }
        else
        {
            // CardManager와 ArtifactManager에 새로운 플레이어 정보를 연결하고, 스탯 재계산을 요청합니다.
            if (ServiceLocator.Get<CardManager>() != null)
            {
                ServiceLocator.Get<CardManager>().LinkToNewPlayer(playerStats);
            }
            if (ServiceLocator.Get<ArtifactManager>() != null)
            {
                ServiceLocator.Get<ArtifactManager>().LinkToNewPlayer(playerStats);
            }
        }

        // --- 3. 공통 마무리 (매 라운드 실행) ---
        // 최종 스탯 계산 (카드, 유물 등 모든 보너스 합산)
        playerStats.CalculateFinalStats();
        
        // 체력을 최대로 회복
        playerStats.currentHealth = playerStats.finalHealth;

        // 게임플레이 루프 시작
        if (ServiceLocator.Get<CardManager>() != null) ServiceLocator.Get<CardManager>().StartCardSelectionLoop();
        if (playerController != null) playerController.StartAutoAttackLoop();
    }

    /// <summary>
    /// 캐릭터의 시작 카드와 유물을 장착합니다.
    /// </summary>
    private void EquipStartingItems(CharacterDataSO characterData)
    {
        // 시작 카드 결정 및 장착
        List<CardDataSO> cardsToEquip = new List<CardDataSO>();
        if (testStartingCards != null && testStartingCards.Count > 0)
        {
            cardsToEquip.AddRange(testStartingCards); // 1순위: 테스트용 카드
        }
        else if (characterData.startingCard != null)
        {
            cardsToEquip.Add(characterData.startingCard); // 2순위: 캐릭터 데이터의 시작 카드
        }

        if (cardsToEquip.Count > 0 && ServiceLocator.Get<CardManager>() != null)
        {
            foreach (var card in cardsToEquip)
            {
                if (card != null)
                {
                    ServiceLocator.Get<CardManager>().AddCard(card);
                    ServiceLocator.Get<CardManager>().Equip(card);
                }
            }
        }

        // 시작 유물 장착
        if (characterData.startingArtifacts != null && characterData.startingArtifacts.Count > 0)
        {
            if (ServiceLocator.Get<ArtifactManager>() != null)
            {
                foreach (var artifact in characterData.startingArtifacts)
                {
                    if (artifact != null) ServiceLocator.Get<ArtifactManager>().EquipArtifact(artifact);
                }
            }
        }
    }
}
