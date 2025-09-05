// 추천 경로: Assets/1.Scripts/Core/EffectContextPool.cs
using System.Collections.Generic;

public class EffectContextPool
{
    // 스택(Stack)을 사용하여 가장 최근에 반환된 객체를 빠르게 재사용합니다.
    private readonly Stack<EffectContext> pool = new Stack<EffectContext>();

    /// <summary>
    /// 풀에서 EffectContext 인스턴스를 가져옵니다. 풀이 비어있으면 새로 생성합니다.
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
    /// 사용이 끝난 EffectContext 인스턴스를 풀에 반환합니다.
    /// </summary>
    public void Return(EffectContext context)
    {
        context.Reset();
        pool.Push(context);
        Log.Info(Log.LogCategory.PoolManager, $"Context returned. Pool size: {pool.Count}");
    }
}