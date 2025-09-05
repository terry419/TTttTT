// ��õ ���: Assets/1.Scripts/Core/EffectContextPool.cs
using System.Collections.Generic;

public class EffectContextPool
{
    // ����(Stack)�� ����Ͽ� ���� �ֱٿ� ��ȯ�� ��ü�� ������ �����մϴ�.
    private readonly Stack<EffectContext> pool = new Stack<EffectContext>();

    /// <summary>
    /// Ǯ���� EffectContext �ν��Ͻ��� �����ɴϴ�. Ǯ�� ��������� ���� �����մϴ�.
    /// </summary>
    public EffectContext Get()
    {
        if (pool.Count > 0)
        {
            Log.Info(Log.LogCategory.PoolManager, $"Reusing existing context. Pool size: {pool.Count - 1}");
            return pool.Pop();
        }
        else
        {
            Log.Info(Log.LogCategory.PoolManager, "Pool is empty. Creating new context.");
            return new EffectContext();
        }
    }

    /// <summary>
    /// ����� ���� EffectContext �ν��Ͻ��� Ǯ�� ��ȯ�մϴ�.
    /// </summary>
    public void Return(EffectContext context)
    {
        context.Reset();
        pool.Push(context);
        Log.Info(Log.LogCategory.PoolManager, $"Context returned. Pool size: {pool.Count}");
    }
}