using UnityEngine;

[CreateAssetMenu(fileName = "CardData_", menuName = "GameData/CardData")]
public class CardDataSO : ScriptableObject
{
    [Header("기본 정보")]
    public string cardID;      // 카드의 고유 ID (예: "warrior_basic_001")
    public string cardName;    // UI에 표시될 카드 이름

    [Header("카드 속성")]
    public CardType type;      // 카드의 타입 (물리 또는 마법)
    /*
    CardType 열거형 정의:
    Physical = 0,
    Magical = 1
    */

    public CardRarity rarity;  // 카드의 희귀도 (일반, 희귀, 영웅, 전설)

    /*
    CardRarity 열거형 정의:
    Common    = 0,
    Rare      = 1,
    Epic      = 2,
    Legendary = 3
    */

    [Header("능력치 배율")]
    public float damageMultiplier;            // 공격력에 적용될 배율
    public float attackSpeedMultiplier;       // 공격 속도에 적용될 배율
    public float moveSpeedMultiplier;         // 이동 속도에 적용될 배율
    public float healthMultiplier;            // 체력에 적용될 배율
    public float critRateMultiplier;          // 치명타 확률에 적용될 배율 (선택 사항)
    public float critDamageMultiplier;        // 치명타 피해에 적용될 배율
    public float lifestealPercentage;         // 흡혈 효과의 회복량 비율 (0.0 ~ 1.0)
    public string effectDescription;          // 카드의 효과에 대한 설명 텍스트

    [Header("발동 조건")]
    public CardEffectType effectType; // 이 카드가 발동하는 특수 효과의 종류
    public TriggerType triggerType;  // 카드가 발동되는 조건 (IV. 카드 시스템의 TriggerType 열거형 참조)
    public TargetingType targetingType;


    [Header("발사체 설정")]
    [Tooltip("이 카드가 발사할 총알 프리팹을 직접 연결하세요.")]
    public GameObject bulletPrefab; // [수정] string에서 다시 GameObject로 변경


    [Header("기획 미정 필드")]
    // 보상으로 카드가 등장할 때의 가중치입니다.
    // 구체적인 로직은 RewardManager 등에서 구현될 예정입니다.
    public float rewardAppearanceWeight;

    // 카드를 해금하기 위한 조건입니다.
    // 구체적인 해금 시스템이 결정되면 해당 로직이 구현될 예정입니다.
    public string unlockCondition;
}