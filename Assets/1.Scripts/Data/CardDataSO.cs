using UnityEngine;

[CreateAssetMenu(fileName = "CardData_", menuName = "GameData/CardData")]
public class CardDataSO : ScriptableObject
{
    [Header("기본 정보")]
    public string cardID;      // 카드의 고유 ID (예: "warrior_basic_001")
    public string cardName;    // UI에 표시될 카드 이름
    public Sprite cardIcon;

    [Header("카드 속성")]
    public CardType type;      // 카드의 타입 (물리 또는 마법)

    public CardRarity rarity;  // 카드의 희귀도 (일반, 희귀, 영웅, 전설)


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
    [Tooltip("분열탄 개수, 주기, 확률 등 다용도로 사용될 값")]
    public float triggerValue; // 분열탄 개수, 주기, 확률 등 다용도로 사용될 값

    [Header("연계 효과")]
    [Tooltip("이 카드의 효과(총알 등)가 적에게 명중했을 때 발동할 2차 효과 카드입니다.")]
    public CardDataSO secondaryEffect; // [추가]


    [Header("발사체 설정")]
    [Tooltip("이 카드가 발사할 총알 프리팹을 직접 연결하세요.")]
    public GameObject bulletPrefab; // [수정] string에서 다시 GameObject로 변경

    [Header("발사 속도 (기동 값)")]
    [Tooltip("0 이하로 설정 시 EffectExecutor의 기본 속도(10f)를 사용")]
    public float bulletSpeed = 10f;  // 여기에 카드별 발사 속도 지정


    [Header("특수 효과 설정")]
    [Tooltip("파동, 번개 등 총알이 아닌 시각 효과 프리팹")]
    public GameObject effectPrefab;
    [Tooltip("독, 화상 등 적에게 적용할 상태 효과")]
    public StatusEffectDataSO statusEffectToApply;



    // 변경사항 1: [추가] 파동/장판 효과 설정 변수들
    [Header("파동/장판 효과 설정")]
    [Tooltip("파동/장판의 총 지속 시간 (초).")]
    public float effectDuration = 3f; // DamagingZone의 duration에 해당
    
    [Tooltip("파동/장판의 확장 속도 (초당).")]
     public float effectExpansionSpeed = 1f; // DamagingZone의 expansionSpeed에 해당
    
    [Tooltip("파동/장판이 확장하는 시간 (초). 파동으로 쓸 경우 Duration보다 조금 더 크게 입력.")]
    public float effectExpansionDuration = 3.1f; // DamagingZone의 expansionDuration에 해당
   
    [Tooltip("장판 모드일 때 틱 데미지 간격 (초). 파동 모드일 때는 100 이상으로 설정.")]
    public float effectTickInterval = 100.0f; // DamagingZone의 tickInterval에 해당
   
    [Tooltip("장판 모드일 때 틱 데미지. 파동 모드일 때는 0으로 설정.")]
    public float effectDamagePerTick = 0f; // DamagingZone의 damagePerTick에 해당
   
    [Tooltip("이 효과가 단일 피해 파동 모드인지 (true), 지속 피해 장판 모드인지 (false).")]
    public bool isEffectSingleHitWaveMode = true; // DamagingZone의 isSingleHitWaveMode에 해당


    [Header("기획 및 가중치")]
    [Tooltip("룰렛에서 선택될 확률 가중치입니다. 높을수록 잘 뽑힙니다.")]
    public float selectionWeight = 1f;

    [Tooltip("카드 보상으로 등장할 확률 가중치입니다.")]
    public float rewardAppearanceWeight;

    // 카드를 해금하기 위한 조건입니다.
    // 구체적인 해금 시스템이 결정되면 해당 로직이 구현될 예정입니다.
    public string unlockCondition;
}