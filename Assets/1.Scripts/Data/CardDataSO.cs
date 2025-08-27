using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "CardData_", menuName = "GameData/CardData")]
public class CardDataSO : ScriptableObject
{
    [Header("기본 정보")]
    public string cardID;
    public string cardName;
    public Sprite cardIcon;

    [Header("카드 속성")]
    public CardType type;
    public CardRarity rarity;

    [Header("능력치 배율")]
    public float baseDamage;
    public float damageMultiplier;
    public float attackSpeedMultiplier;
    public float moveSpeedMultiplier;
    public float healthMultiplier;
    public float critRateMultiplier;
    public float critDamageMultiplier;
    [Range(0, 100)]
    public float lifestealPercentage;
    public string effectDescription;

    [Header("발동 조건")]
    public CardEffectType effectType;
    public TriggerType triggerType;
    public TargetingType targetingType;
    public float triggerValue;

    [Header("연계 효과")]
    public CardDataSO secondaryEffect;

    [Header("발사체 설정")]
    public AssetReferenceGameObject bulletPrefabRef;
    public float bulletSpeed = 10f;
    public int bulletPierceCount = 0;
    public int bulletPreloadCount = 100;

    [Header("특수 효과 설정")]
    public AssetReferenceGameObject effectPrefabRef;
    public int effectPreloadCount = 20;
    public StatusEffectDataSO statusEffectToApply;

    // 이하는 삭제되었으므로, 만약 남아있다면 지워주세요.
    // [Header("파동/장판 효과 설정")] ...

    [Header("기획 및 가중치")]
    public float selectionWeight = 1f;
    public float rewardAppearanceWeight;
    public string unlockCondition;

    // [핵심] 팩토리를 통해 자신의 행동(Action/Command)을 생성하는 메소드
    public ICardAction CreateAction()
    {
        return CardActionFactory.Create(this.effectType);
    }
}