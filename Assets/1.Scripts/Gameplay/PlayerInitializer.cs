using UnityEngine;
using System.Collections.Generic;

public class PlayerInitializer : MonoBehaviour
{
    [Header("테스트용 시작 카드 목록")]
    [Tooltip("캐릭터 데이터(SO)에 시작 카드가 설정되어 있으면, 이 목록은 무시됩니다.")]
    [SerializeField] private List<CardDataSO> testStartingCards;

    void Start()
    {
        CharacterStats playerStats = GetComponent<CharacterStats>();
        SpriteRenderer playerSpriteRenderer = GetComponent<SpriteRenderer>();

        GameManager gameManager = GameManager.Instance;
        DataManager dataManager = DataManager.Instance;
        ProgressionManager progressionManager = ProgressionManager.Instance;

        CharacterDataSO characterToLoad = gameManager.SelectedCharacter ?? dataManager.GetCharacter("warrior");

        // [수정] 장착할 카드를 결정하는 더 안전한 로직
        List<CardDataSO> cardsToEquip = new List<CardDataSO>();
        if (characterToLoad != null && characterToLoad.startingCard != null)
        {
            // 1순위: 캐릭터 데이터에 설정된 시작 카드를 사용
            cardsToEquip.Add(characterToLoad.startingCard);
        }
        else if (testStartingCards != null && testStartingCards.Count > 0)
        {
            // 2순위: 캐릭터 데이터에 카드가 없으면, Inspector의 테스트용 카드를 사용
            cardsToEquip.AddRange(testStartingCards);
        }

        if (characterToLoad != null)
        {
            playerStats.stats = characterToLoad.baseStats;
            playerSpriteRenderer.sprite = characterToLoad.illustration;

            CharacterPermanentStats permanentStats = progressionManager.GetPermanentStatsFor(characterToLoad.characterId);
            ApplyPermanentStats(playerStats, permanentStats);

            int allocatedPoints = gameManager.AllocatedPoints;
            if (allocatedPoints > 0)
            {
                ApplyAllocatedPoints(playerStats, allocatedPoints, permanentStats);
            }
        }
        else
        {
            Debug.LogError("CRITICAL: 적용할 캐릭터 데이터를 찾을 수 없습니다!");
        }

        // 결정된 카드 목록을 장착
        if (cardsToEquip.Count > 0 && CardManager.Instance != null)
        {
            foreach (var card in cardsToEquip)
            {
                if (card != null)
                {
                    CardManager.Instance.AddCard(card);
                    CardManager.Instance.Equip(card);
                }
            }
            Debug.Log($"[PlayerInitializer] {cardsToEquip.Count}개의 시작 카드를 장착했습니다.");
        }

        playerStats.CalculateFinalStats();
        playerStats.currentHealth = playerStats.finalHealth;

        // 시작 유물 장착 로직
        if (characterToLoad != null && characterToLoad.startingArtifacts != null && characterToLoad.startingArtifacts.Count > 0)
        {
            if (ArtifactManager.Instance != null)
            {
                foreach (var artifact in characterToLoad.startingArtifacts)
                {
                    if (artifact != null) ArtifactManager.Instance.EquipArtifact(artifact);
                }
            }
        }

        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.StartAutoAttackLoop();
            playerController.StartCardTriggerLoop();
        }
    }

    private void ApplyPermanentStats(CharacterStats playerStats, CharacterPermanentStats permanentStats)
    {
        if (permanentStats == null) return;
        playerStats.boosterDamageRatio += permanentStats.investedRatios.GetValueOrDefault(StatType.Attack, 0f);
        playerStats.boosterAttackSpeedRatio += permanentStats.investedRatios.GetValueOrDefault(StatType.AttackSpeed, 0f);
        playerStats.boosterMoveSpeedRatio += permanentStats.investedRatios.GetValueOrDefault(StatType.MoveSpeed, 0f);
        playerStats.boosterHealthRatio += permanentStats.investedRatios.GetValueOrDefault(StatType.Health, 0f);
        playerStats.boosterCritDamageRatio += permanentStats.investedRatios.GetValueOrDefault(StatType.CritMultiplier, 0f);
    }

    private void ApplyAllocatedPoints(CharacterStats playerStats, int points, CharacterPermanentStats permStats)
    {
        List<StatType> availableStats = permStats.GetUnlockedStats();
        if (availableStats.Count == 0) return;

        for (int i = 0; i < points; i++)
        {
            StatType targetStat = availableStats[Random.Range(0, availableStats.Count)];
            float weight = GetWeightForStat(targetStat);

            switch (targetStat)
            {
                case StatType.Attack: playerStats.boosterDamageRatio += weight; break;
                case StatType.AttackSpeed: playerStats.boosterAttackSpeedRatio += weight; break;
                case StatType.MoveSpeed: playerStats.boosterMoveSpeedRatio += weight; break;
                case StatType.Health: playerStats.boosterHealthRatio += weight; break;
                case StatType.CritMultiplier: playerStats.boosterCritDamageRatio += weight; break;
            }
        }
    }

    private float GetWeightForStat(StatType stat)
    {
        return stat == StatType.Health ? 0.02f : 0.01f;
    }
}