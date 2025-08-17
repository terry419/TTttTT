// --- ���� ��ġ: Assets/1.Scripts/Core/EffectExecutor.cs ---
// --- ����: �� ������ ���� EffectExecutor.cs�� ��ü�ؾ� �մϴ�. ---

using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Dictionary ����� ���� �߰�
using System;

/// <summary>
/// ī���� �ð�/���� ȿ���� �����ϴ� �߾� Ŭ�����Դϴ�. (�����丵 ����)
/// ���� ������ ����Ͽ� �� CardEffectType�� �´� �ڵ鷯�� ã�� ������ �����մϴ�.
/// </summary>
public class EffectExecutor : MonoBehaviour
{
    public static EffectExecutor Instance { get; private set; }

    // �ڵ鷯���� ������ �� �ֵ��� public���� �����ϰų� ������Ƽ�� �����մϴ�.
    public PoolManager poolManager { get; private set; }
    public PlayerController playerController { get; private set; }
    public CharacterStats playerStats { get; private set; }

    // �� ī�� ȿ�� Ÿ�Կ� �´� �ڵ鷯�� �����ϴ� ��ųʸ�
    private Dictionary<CardEffectType, ICardEffectHandler> effectHandlers;

    void Awake()
    {
        Instance = this;
        InitializeHandlers(); // �ڵ鷯 ��ųʸ� �ʱ�ȭ
    }

    void Start()
    {
        // �ٸ� �Ŵ��� �ν��Ͻ��� �����մϴ�.
        poolManager = PoolManager.Instance;
        playerController = PlayerController.Instance;
        if (playerController != null)
        {
            playerStats = playerController.GetComponent<CharacterStats>();
        }
    }

    /// <summary>
    /// ī�� ȿ�� Ÿ�԰� �ڵ鷯 Ŭ������ �����Ͽ� ��ųʸ��� �ʱ�ȭ�մϴ�.
    /// ���ο� ȿ���� �߰��� �� �� ���� �� �ٸ� �߰��ϸ� �˴ϴ�.
    /// </summary>
    private void InitializeHandlers()
    {
        effectHandlers = new Dictionary<CardEffectType, ICardEffectHandler>
        {
            { CardEffectType.SingleShot, new SingleShotHandler() },
            { CardEffectType.SplitShot, new SplitShotHandler() },
            { CardEffectType.Wave, new WaveHandler() }
            // ��: { CardEffectType.Lightning, new LightningHandler() }
        };
    }

    /// <summary>
    /// ī���� ȿ���� �����մϴ�. (�÷��̾� �ڽſ��� �ߵ�)
    /// </summary>
    public void Execute(CardDataSO cardData, float actualDamageDealt = 0f)
    {
        if (cardData == null || playerController == null || playerStats == null)
        {
            Debug.LogError("[EffectExecutor] �ʼ� ������Ʈ(CardData, PlayerController, PlayerStats) �� �ϳ��� null�Դϴ�!");
            return;
        }

        // OnHit Ÿ���� ���� ȿ�� ���� �÷��̾� ��ġ���� ��� �ߵ��Ǿ�� �ϹǷ� ���⼭ ó���մϴ�.
        if (cardData.triggerType == TriggerType.OnHit && cardData.lifestealPercentage > 0 && actualDamageDealt > 0)
        {
            playerController.Heal(actualDamageDealt * cardData.lifestealPercentage);
        }

        // �÷��̾��� �߻� ������ �������� ȿ���� �����մϴ�.
        Execute(cardData, playerController.firePoint);
    }

    /// <summary>
    /// Ư�� ��ġ���� ī���� ȿ���� �����մϴ�. (���� ��ġ���� 2�� ȿ�� �ߵ� ��)
    /// </summary>
    public void Execute(CardDataSO cardData, Transform spawnPoint)
    {
        if (cardData == null) return;

        // ī�� �������� effectType�� �´� �ڵ鷯�� ��ųʸ����� ã���ϴ�.
        if (effectHandlers.TryGetValue(cardData.effectType, out ICardEffectHandler handler))
        {
            // ã�� �ڵ鷯���� ������ �����մϴ�.
            handler.Execute(cardData, this, spawnPoint);
        }
        else
        {
            Debug.LogError($"[EffectExecutor] '{cardData.effectType}' Ÿ�Կ� ���� �ڵ鷯�� ��ϵ��� �ʾҽ��ϴ�!");
        }
    }

    // --- �ڵ鷯���� �������� ����ϴ� ���� �޼��� ---

    /// <summary>
    /// �÷��̾��� ���� �ɷ�ġ�� ī�� ������ �����Ͽ� �� �������� ����մϴ�.
    /// </summary>
    public float CalculateTotalDamage(CardDataSO cardData)
    {
        float totalRatio = 1
                        + playerStats.cardDamageRatio
                        + playerStats.artifactDamageRatio
                        + playerStats.boosterDamageRatio
                        + cardData.damageMultiplier;

        return playerStats.finalDamage * totalRatio;
    }

    /// <summary>
    /// Ÿ���� Ÿ�Կ� ���� ��ǥ�� ã�� �߻� ������ ����մϴ�.
    /// </summary>
    public float GetTargetingAngle(TargetingType targetingType)
    {
        Transform target = TargetingSystem.FindTarget(targetingType, playerController.transform);

        if (target != null)
        {
            Vector2 directionToTarget = (target.position - playerController.firePoint.position).normalized;
            return Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
        }
        else
        {
            return playerController.firePoint.eulerAngles.z;
        }
    }
}
