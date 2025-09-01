using System.Collections.Generic;
using UnityEngine;

public class PlayerInitializer : MonoBehaviour
{
    [Header("v8.0 테스트용 시작 카드 목록")]
    [SerializeField] private List<NewCardDataSO> testStartingNewCards;

    void Start()
    {

        var playerStats = GetComponent<CharacterStats>();
        GetComponent<SpriteRenderer>().sortingOrder = 20; // [추가] 플레이어 렌더링 순서 설정
        var playerController = GetComponent<PlayerController>();
        var gameManager = ServiceLocator.Get<GameManager>();

        CharacterDataSO characterToLoad = gameManager.SelectedCharacter ?? ServiceLocator.Get<DataManager>().GetCharacter(CharacterIDs.Warrior);
        if (characterToLoad == null)
        {
            Debug.LogError("[INIT-DEBUG] CRITICAL: 적용할 캐릭터 데이터를 찾을 수 없습니다! 여기서 중단됩니다.");
            return;
        }

        // 캐릭터 외형(스프라이트) 변경 로직 추가
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && characterToLoad.illustration != null)
        {
            spriteRenderer.sprite = characterToLoad.illustration;
            Debug.Log($"[PlayerInitializer] 캐릭터 외형을 {characterToLoad.characterName}의 것으로 변경했습니다.");
        }
        else
        {
            if(spriteRenderer == null) Debug.LogWarning("[PlayerInitializer] SpriteRenderer 컴포넌트를 찾지 못해 외형을 변경할 수 없습니다.");
            if(characterToLoad.illustration == null) Debug.LogWarning($"[PlayerInitializer] {characterToLoad.characterName}의 illustration(스프라이트)이 설정되지 않아 외형을 변경할 수 없습니다.");
        }

        playerStats.stats = characterToLoad.baseStats;

        var cardManager = ServiceLocator.Get<CardManager>();
        var artifactManager = ServiceLocator.Get<ArtifactManager>();

        if (gameManager.isFirstRound)
        {
            if (cardManager != null)
            {
                cardManager.ClearAndResetDeck(); // [v9.0 수정] 새 게임 시작 시 카드 목록을 완전히 초기화
                cardManager.LinkToNewPlayer(playerStats);
            }
            if (artifactManager != null) artifactManager.LinkToNewPlayer(playerStats);

            var progressionManager = ServiceLocator.Get<ProgressionManager>();
            CharacterPermanentStats permanentStats = progressionManager.GetPermanentStatsFor(characterToLoad.characterId);
            playerStats.ApplyPermanentStats(permanentStats);
            playerStats.ApplyAllocatedPoints(gameManager.AllocatedPoints, permanentStats);

            // [v9.0 수정] 시작 카드 로직: CharacterDataSO 우선, 없으면 Initializer의 테스트 카드 사용
            List<NewCardDataSO> cardsToEquip = characterToLoad.startingCards;
            if (cardsToEquip == null || cardsToEquip.Count == 0)
            {
                Debug.Log("[PlayerInitializer] CharacterData에 시작 카드가 없어 테스트용 시작 카드를 사용합니다.");
                cardsToEquip = testStartingNewCards;
            }

            if (cardsToEquip != null && cardManager != null)
            {
                foreach (var cardData in cardsToEquip)
                {
                    if (cardData != null)
                    {
                        CardInstance instance = cardManager.AddCard(cardData);
                        if (instance != null)
                        {
                            cardManager.Equip(instance);
                        }
                    }
                }
            }

            if (characterToLoad.startingArtifacts != null && artifactManager != null)
            {
                foreach (var artifact in characterToLoad.startingArtifacts)
                {
                    if (artifact != null) artifactManager.EquipArtifact(artifact);
                }
            }

            gameManager.isFirstRound = false;
        }
        else
        {
            if (cardManager != null) cardManager.LinkToNewPlayer(playerStats);
            if (artifactManager != null) artifactManager.LinkToNewPlayer(playerStats);
        }

        playerStats.CalculateFinalStats();
        playerStats.currentHealth = playerStats.FinalHealth;

        if (cardManager != null) cardManager.StartCardSelectionLoop();
    }
}