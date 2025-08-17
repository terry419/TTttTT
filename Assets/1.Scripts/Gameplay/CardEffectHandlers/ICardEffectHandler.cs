// --- ���� ��ġ: Assets/1.Scripts/Core/ICardEffectHandler.cs ---

using UnityEngine;

/// <summary>
/// ��� ī�� ȿ�� ó����(Handler)�� �����ؾ� �ϴ� �������̽��Դϴ�.
/// ���� ����(Strategy Pattern)�� �����Ͽ� �� ī�� ȿ���� ���� ������ ĸ��ȭ�մϴ�.
/// </summary>
public interface ICardEffectHandler
{
    /// <summary>
    /// ī�� ȿ���� �����մϴ�.
    /// </summary>
    /// <param name="cardData">������ ȿ���� ���ǵ� ī�� �������Դϴ�.</param>
    /// <param name="executor">PoolManager, PlayerController �� �ٽ� ������ �����ϴ� EffectExecutor�� �ν��Ͻ��Դϴ�.</param>
    /// <param name="spawnPoint">ȿ���� ������ ��ġ�Դϴ�. (��: ������ ��ġ, �÷��̾��� �߻� ����)</param>
    void Execute(CardDataSO cardData, EffectExecutor executor, Transform spawnPoint);
}
