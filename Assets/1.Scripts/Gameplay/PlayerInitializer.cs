using System.Collections.Generic;
using UnityEngine;

public class PlayerInitializer : MonoBehaviour
{
    [Header("v8.0 테스트용 시작 카드 목록")]
    [SerializeField] private List<NewCardDataSO> testStartingNewCards;

    void Start()
    {

        var playerStats = GetComponent<CharacterStats>();
        GetComponent<SpriteRenderer>().sortingOrder = 20;
        var playerController = GetComponent<PlayerController>();
        var gameManager = ServiceLocator.Get<GameManager>();
        var playerDataManager = ServiceLocator.Get<PlayerDataManager>();

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

        if (playerDataManager.IsRunInitialized)
        {
            // [수정] CardManager와 ArtifactManager가 PlayerDataManager의 빈 리스트를 먼저 참조하도록 순서 변경
            if (cardManager != null) cardManager.LinkToNewPlayer(playerStats);
            if (artifactManager != null) artifactManager.LinkToNewPlayer(playerStats);

            var progressionManager = ServiceLocator.Get<ProgressionManager>();
            CharacterPermanentStats permanentStats = progressionManager.GetPermanentStatsFor(characterToLoad.characterId);
            playerStats.ApplyPermanentStats(permanentStats);
            playerStats.ApplyAllocatedPoints(gameManager.AllocatedPoints, permanentStats);

            List<NewCardDataSO> cardsToEquip = characterToLoad.startingCards;
            if (cardsToEquip == null || cardsToEquip.Count == 0)
            {
                cardsToEquip = testStartingNewCards;
            }

            if (cardsToEquip != null && cardManager != null)
            {
                foreach (var cardData in cardsToEquip)
                {
                    if (cardData != null)
                    {
                        CardInstance instance = cardManager.AddCard(cardData);
                        if (instance != null) cardManager.Equip(instance);
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

            // [추가] 모든 시작 아이템 지급이 끝났음을 PlayerDataManager에 알림
            playerDataManager.CompleteRunInitialization();
        }
        else
        {
            // 새 게임이 아닌 경우 (예: 테스트 씬 직접 실행)
            if (cardManager != null) cardManager.LinkToNewPlayer(playerStats);
            if (artifactManager != null) artifactManager.LinkToNewPlayer(playerStats);
        }

        playerStats.CalculateFinalStats();

        if (cardManager != null)
        {
            cardManager.StartCardSelectionLoop();
        }
    }
}