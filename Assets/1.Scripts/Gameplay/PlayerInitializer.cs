using System.Collections.Generic;
using UnityEngine;

public class PlayerInitializer : MonoBehaviour
{
    [Header("v8.0 테스트용 시작 카드 목록")]
    [SerializeField] private List<NewCardDataSO> testStartingNewCards;

    void Start()
    {

        var playerStats = GetComponent<CharacterStats>();
        var playerController = GetComponent<PlayerController>();
        var gameManager = ServiceLocator.Get<GameManager>();

        CharacterDataSO characterToLoad = gameManager.SelectedCharacter ?? ServiceLocator.Get<DataManager>().GetCharacter(CharacterIDs.Warrior);
        if (characterToLoad == null)
        {
            Debug.LogError("[INIT-DEBUG] CRITICAL: 적용할 캐릭터 데이터를 찾을 수 없습니다! 여기서 중단됩니다.");
            return;
        }
        playerStats.stats = characterToLoad.baseStats;

        var cardManager = ServiceLocator.Get<CardManager>();
        var artifactManager = ServiceLocator.Get<ArtifactManager>();

        if (gameManager.isFirstRound)
        {
            if (cardManager != null) cardManager.LinkToNewPlayer(playerStats);
            if (artifactManager != null) artifactManager.LinkToNewPlayer(playerStats);

            var progressionManager = ServiceLocator.Get<ProgressionManager>();
            CharacterPermanentStats permanentStats = progressionManager.GetPermanentStatsFor(characterToLoad.characterId);
            playerStats.ApplyPermanentStats(permanentStats);
            playerStats.ApplyAllocatedPoints(gameManager.AllocatedPoints, permanentStats);

            if (characterToLoad.startingCard != null && cardManager != null)
            {
                CardInstance instanceToEquip = cardManager.AddCard(characterToLoad.startingCard);
                if (instanceToEquip != null)
                {
                    cardManager.Equip(instanceToEquip);
                }
            }

            if (characterToLoad.startingArtifacts != null && artifactManager != null)
            {
                foreach (var artifact in characterToLoad.startingArtifacts)
                {
                    if (artifact != null) artifactManager.EquipArtifact(artifact);
                }
            }

            // v8.0 테스트용 시작 카드 목록을 추가하고 장착합니다.
            if (testStartingNewCards != null && cardManager != null)
            {
                foreach (var cardData in testStartingNewCards)
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

        if (playerController != null) playerController.StartAutoAttackLoop();
    }
}