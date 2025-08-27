// --- 파일 위치: Assets/1/Scripts/Gameplay/PlayerInitializer.cs ---

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 게임 시작 시 플레이어 오브젝트를 초기화하는 역할을 담당합니다. (v8.0 호환)
/// </summary>
public class PlayerInitializer : MonoBehaviour
{
    [Header("테스트용 시작 카드 목록")]
    [Tooltip("구버전 CardDataSO를 테스트할 때 사용합니다.")]
    [SerializeField] private List<CardDataSO> testStartingCards;

    // [추가] v8.0 NewCardDataSO를 테스트하기 위한 새로운 리스트
    [Header("v8.0 테스트용 시작 카드 목록")]
    [SerializeField] private List<NewCardDataSO> testStartingNewCards;

    void Start()
    {
        EventSystem.current.SetSelectedGameObject(null);

        var playerStats = GetComponent<CharacterStats>();
        var playerController = GetComponent<PlayerController>();
        var gameManager = ServiceLocator.Get<GameManager>();
        var playerSpriteRenderer = GetComponent<SpriteRenderer>();
        var debugManager = ServiceLocator.Get<DebugManager>();

        if (debugManager != null)
        {
            debugManager.RegisterPlayer(playerStats);
        }

        CharacterDataSO characterToLoad = gameManager.SelectedCharacter ?? ServiceLocator.Get<DataManager>().GetCharacter(CharacterIDs.Warrior);
        if (characterToLoad == null)
        {
            Debug.LogError("CRITICAL: 적용할 캐릭터 데이터를 찾을 수 없습니다!");
            return;
        }

        playerStats.stats = characterToLoad.baseStats;
        playerSpriteRenderer.sprite = characterToLoad.illustration;

        var cardManager = ServiceLocator.Get<CardManager>();
        var artifactManager = ServiceLocator.Get<ArtifactManager>();

        if (gameManager.isFirstRound)
        {
            Debug.Log("<color=lime>[PlayerInitializer] 첫 라운드입니다. 시작 아이템과 영구 스탯을 적용합니다.</color>");

            if (cardManager != null) cardManager.LinkToNewPlayer(playerStats);
            if (artifactManager != null) artifactManager.LinkToNewPlayer(playerStats);

            var progressionManager = ServiceLocator.Get<ProgressionManager>();
            CharacterPermanentStats permanentStats = progressionManager.GetPermanentStatsFor(characterToLoad.characterId);
            playerStats.ApplyPermanentStats(permanentStats);
            playerStats.ApplyAllocatedPoints(gameManager.AllocatedPoints, permanentStats);

            // [수정] EquipStartingItems 호출
            EquipStartingItems(characterToLoad, cardManager, artifactManager);

            gameManager.isFirstRound = false;
        }
        else
        {
            Debug.Log("<color=yellow>[PlayerInitializer] 이후 라운드입니다. 기존 카드/유물 정보를 유지(재계산)합니다.</color>");
            if (cardManager != null) cardManager.LinkToNewPlayer(playerStats);
            if (artifactManager != null) artifactManager.LinkToNewPlayer(playerStats);
        }

        playerStats.CalculateFinalStats();
        playerStats.currentHealth = playerStats.FinalHealth;
        if (cardManager != null) cardManager.StartCardSelectionLoop();
        if (playerController != null) playerController.StartAutoAttackLoop();
    }

    // [수정] v8.0 신규 카드 장착 로직 추가
    private void EquipStartingItems(CharacterDataSO characterData, CardManager cardManager, ArtifactManager artifactManager)
    {
        // 1. 구버전 카드 장착 로직 (기존과 동일)
        List<CardDataSO> cardsToEquip = new List<CardDataSO>();
        if (testStartingCards != null && testStartingCards.Count > 0)
        {
            cardsToEquip.AddRange(testStartingCards);
        }
        else if (characterData.startingCard != null)
        {
            cardsToEquip.Add(characterData.startingCard);
        }

        if (cardsToEquip.Count > 0 && cardManager != null)
        {
            foreach (var card in cardsToEquip)
            {
                if (card != null)
                {
                    cardManager.AddCard(card);
                    cardManager.Equip(card);
                }
            }
        }

        // 2. [추가] 신규 v8.0 카드 장착 로직
        if (testStartingNewCards != null && testStartingNewCards.Count > 0 && cardManager != null)
        {
            foreach (var newCard in testStartingNewCards)
            {
                if (newCard != null)
                {
                    cardManager.AddCard(newCard); // CardManager에 새 오버로드된 함수 호출
                    cardManager.Equip(newCard);   // CardManager에 새 오버로드된 함수 호출
                }
            }
        }

        // 3. 유물 장착 로직 (기존과 동일)
        if (characterData.startingArtifacts != null && characterData.startingArtifacts.Count > 0)
        {
            if (artifactManager != null)
            {
                foreach (var artifact in characterData.startingArtifacts)
                {
                    if (artifact != null) artifactManager.EquipArtifact(artifact);
                }
            }
        }
    }
}