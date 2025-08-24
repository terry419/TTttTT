// --- 파일 위치: Assets/1/Scripts/Gameplay/PlayerInitializer.cs ---

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 게임 시작 시 플레이어 오브젝트를 초기화하는 역할을 담당합니다. (최종 수정 버전)
/// </summary>
public class PlayerInitializer : MonoBehaviour
{
    [Header("테스트용 시작 카드 목록")]
    [Tooltip("캐릭터 데이터(SO)에 시작 카드가 설정되어 있으면, 이 목록은 무시됩니다.")]
    [SerializeField] private List<CardDataSO> testStartingCards;

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

            // 매니저들에게 새로운 플레이어 정보를 연결합니다.
            if (cardManager != null) cardManager.LinkToNewPlayer(playerStats);
            if (artifactManager != null) artifactManager.LinkToNewPlayer(playerStats);

            // 첫 라운드에만 영구 스탯 적용 및 시작 아이템을 지급합니다.
            var progressionManager = ServiceLocator.Get<ProgressionManager>();
            CharacterPermanentStats permanentStats = progressionManager.GetPermanentStatsFor(characterToLoad.characterId);
            playerStats.ApplyPermanentStats(permanentStats);
            playerStats.ApplyAllocatedPoints(gameManager.AllocatedPoints, permanentStats);
            EquipStartingItems(characterToLoad, cardManager, artifactManager);

            gameManager.isFirstRound = false;
        }
        else
        {
            // ▼▼▼ [핵심 수정] 로그를 먼저 출력하고, 이후에 LinkToNewPlayer를 호출합니다. ▼▼▼
            Debug.Log("<color=yellow>[PlayerInitializer] 이후 라운드입니다. 기존 카드/유물 정보를 유지(재계산)합니다.</color>");

            // 이후 라운드에서는 새 플레이어 정보만 연결해주면, LinkToNewPlayer가 알아서 기존 상태를 복원합니다.
            if (cardManager != null) cardManager.LinkToNewPlayer(playerStats);
            if (artifactManager != null) artifactManager.LinkToNewPlayer(playerStats);
        }

        // 공통 마무리 (매 라운드 실행)
        playerStats.CalculateFinalStats();
        playerStats.currentHealth = playerStats.FinalHealth;
        if (cardManager != null) cardManager.StartCardSelectionLoop();
        if (playerController != null) playerController.StartAutoAttackLoop();
    }

    private void EquipStartingItems(CharacterDataSO characterData, CardManager cardManager, ArtifactManager artifactManager)
    {
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