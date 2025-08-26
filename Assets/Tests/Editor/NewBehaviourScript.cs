using UnityEngine;

public class PoolProfileTest : MonoBehaviour
{
    void Start()
    {
        var pool = new EffectContextPool();

        // �������Ϸ� ����
        UnityEngine.Profiling.Profiler.BeginSample("Pool_Get_Return");

        for (int i = 0; i < 10000; i++)
        {
            var ctx = pool.Get();
            pool.Return(ctx);
        }

        UnityEngine.Profiling.Profiler.EndSample();
    }
}
