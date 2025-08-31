// ���: ./TTttTT/Assets/1/Scripts/Data/CardEffects/ApplyEffectToCasterModule.cs
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;

/// <summary>
/// [�ű�] ���� ������ �ڽ�(Caster)���Ը� ��� ������ ȿ��(����, ����/��� ���� �� ȸ��, VFX)�� �����ϴ� ���� ����Դϴ�.
/// </summary>
[CreateAssetMenu(fileName = "Module_ApplyEffectToCaster_", menuName = "GameData/v8.0/Modules/ApplyEffectToCaster")]
public class ApplyEffectToCasterModule : CardEffectSO
{
    [Header("[ �ߵ� ���� ]")]
    [Tooltip("ȿ�� �ߵ� ������ �����մϴ�.")]
    public EffectTrigger Trigger = EffectTrigger.OnFire;

    [Tooltip("ȿ���� ������ ����� Ȯ�� (0.0 ~ 100.0)")]
    [Range(0f, 100f)] public float ApplicationChance = 100f;

    [Header("[ ȿ�� ���� - ���� ]")]
    [Tooltip("�� ȿ���� �ĺ��� ���� ID�� �����մϴ�. (��: �ż�, ����, ����)")]
    public string StatusEffectID;

    [Tooltip("���� ���� �� ���� ȿ���� ���� �ð�(��). 0���� ū ���� �ݵ�� �Է��ؾ� �մϴ�.")]
    public float Duration = 5.0f;

    [Tooltip("���� ȿ�� ��ø ����� �����մϴ�.")]
    public StackingBehavior StackingBehavior = StackingBehavior.RefreshDuration;

    [Header("[ ȿ�� ���� - �÷��̾� ���� ���� (%) ]")]
    [Tooltip("�÷��̾��� ���� ���ݷ� ���ʽ��� �����մϴ�.")]
    public float FinalDamageBonus;
    [Tooltip("�÷��̾��� ���� ���� �ӵ��� �����մϴ�.")]
    public float FinalAttackSpeedBonus;
    [Tooltip("�÷��̾��� ���� �̵� �ӵ��� �����մϴ�.")]
    public float FinalMoveSpeedBonus;
    [Tooltip("�÷��̾��� ���� �ִ� ü���� �����մϴ�.")]
    public float FinalHealthBonus;
    [Tooltip("�÷��̾��� ���� ġ��Ÿ Ȯ���� �����մϴ�.")]
    public float FinalCritRateBonus;
    [Tooltip("�÷��̾��� ���� ġ��Ÿ ���ط��� �����մϴ�.")]
    public float FinalCritDamageBonus;

    [Header("[ ȿ�� ���� - ���� ���� (DoT) ]")]
    [Tooltip("���ط� ��� ��� (Flat: ���� ��ġ, MaxHealthPercentage: �ִ� ü�� ���)")]
    public DamageType DamageType = DamageType.Flat;
    [Tooltip("�ʴ� ���� ���ط�. (ƽ ������ 1�ʷ� ����)")]
    public float DamageAmount;
    [Tooltip("üũ ��, �ڽ��� FinalDamageBonus ������ DoT ���ط��� ������ �ݴϴ�.")]
    public bool ScalesWithDmgBonus = false;

    [Header("[ ȿ�� ���� - ȸ�� ]")]
    [Tooltip("ȸ���� �̷������ �ð� (��). 0�� ��� ��� ȸ���˴ϴ�.")]
    public float HealDuration;
    [Tooltip("ȸ���� ��� ��� (Flat: ���� ��ġ, MaxHealthPercentage: �ִ� ü�� ���)")]
    public HealType HealType = HealType.Flat;
    [Tooltip("�� ȸ����.")]
    public float HealAmount;

    [Header("[ ȿ�� ���� - VFX ]")]
    [Tooltip("ȿ���� ó�� ����Ǵ� ���� 1ȸ ����� VFX�� ��巹���� �ּ�.")]
    public AssetReferenceGameObject OnApplyVFX;
    [Tooltip("ȿ���� ���ӵǴ� ���� �÷��̾�� �پ ��� ����� VFX�� ��巹���� �ּ�.")]
    public AssetReferenceGameObject LoopingVFX;
    [Tooltip("ȿ���� ���ӽð��� ���� ������� ���� 1ȸ ����� VFX�� ��巹���� �ּ�.")]
    public AssetReferenceGameObject OnExpireVFX;

    public override void Execute(EffectContext context)
    {
        // 1. ��ȿ�� �˻�: �����ڰ� ������ ������ �� �����ϴ�.
        if (context.Caster == null)
        {
            Debug.LogWarning($"<color=yellow>[{GetType().Name}]</color> '{this.name}' ���� �ߴ�: Caster�� ���� ȿ���� ������ ����� �����ϴ�.");
            return;
        }

        Debug.Log($"<color=lime>[{GetType().Name}]</color> '{this.name}' ���� �õ�. ���: {context.Caster.name}");

        // 2. Ȯ�� üũ
        if (Random.Range(0f, 100f) > ApplicationChance)
        {
            Debug.Log($"<color=yellow>[{GetType().Name}]</color> Ȯ��({ApplicationChance}%) üũ�� �����Ͽ� ȿ���� ������� �ʾҽ��ϴ�.");
            return;
        }

        // 3. ���� ���ʽ� ������ ����
        var statBonuses = new Dictionary<StatType, float>();
        if (FinalDamageBonus != 0) statBonuses.Add(StatType.Attack, FinalDamageBonus);
        if (FinalAttackSpeedBonus != 0) statBonuses.Add(StatType.AttackSpeed, FinalAttackSpeedBonus);
        if (FinalMoveSpeedBonus != 0) statBonuses.Add(StatType.MoveSpeed, FinalMoveSpeedBonus);
        if (FinalHealthBonus != 0) statBonuses.Add(StatType.Health, FinalHealthBonus);
        if (FinalCritRateBonus != 0) statBonuses.Add(StatType.CritRate, FinalCritRateBonus);
        if (FinalCritDamageBonus != 0) statBonuses.Add(StatType.CritMultiplier, FinalCritDamageBonus);

        // 4. ��� �����͸� ��� StatusEffectInstance�� �����մϴ�.
        var effectInstance = new StatusEffectInstance(
            target: context.Caster.gameObject,
            id: StatusEffectID,
            duration: Duration,
            bonuses: statBonuses,
            dotAmount: DamageAmount,
            dotType: DamageType,
            scales: ScalesWithDmgBonus,
            healAmount: HealAmount,
            healDuration: HealDuration,
            healType: HealType,
            stacking: StackingBehavior,
            caster: context.Caster,
            onApplyVFX: OnApplyVFX,
            loopingVFX: LoopingVFX,
            onExpireVFX: OnExpireVFX
        );

        // 5. StatusEffectManager���� ȿ�� ������ ���� ��û�մϴ�.
        ServiceLocator.Get<StatusEffectManager>()?.ApplyStatusEffect(context.Caster.gameObject, effectInstance);
        Debug.Log($"<color=cyan>[{GetType().Name}]</color> '{context.Caster.name}'���� '{StatusEffectID}' ȿ�� ������ ��û�߽��ϴ�.");
    }
}