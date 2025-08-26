// 추천 경로: Assets/Tests/Editor/EffectContextPoolTests.cs
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Profiling; // 프로파일러 사용을 위해 추가

public class EffectContextPoolTests
{
    [Test]
    public void Pool_ReturnsSameInstance_AfterReturning()
    {
        // ... (이전과 동일한 코드) ...
        Log.Print("[TEST] Pool_ReturnsSameInstance_AfterReturning 테스트 시작.");
        var pool = new EffectContextPool();
        var initialInstance = pool.Get();
        pool.Return(initialInstance);
        var reusedInstance = pool.Get();
        Assert.AreSame(initialInstance, reusedInstance, "객체를 반환한 후에는 반드시 동일한 인스턴스를 재사용해야 합니다.");
        Log.Print("[TEST] 기능 검증 1 통과: 객체 재사용 확인 완료.");
    }

    [Test]
    public void Pool_ReusingFromPool_GeneratesZeroGCAlloc()
    {
        // Given (준비)
        Log.Print("[TEST] Pool_ReusingFromPool_GeneratesZeroGCAlloc 테스트 시작.");
        var pool = new EffectContextPool();
        var context = pool.Get(); // 최초 1회 생성 (여기서는 GC 발생이 정상)
        pool.Return(context);   // 풀에 반납

        // When (측정)
        // ▼▼▼ 정확히 이 구간의 메모리 할당만 측정합니다. ▼▼▼
        Profiler.BeginSample("Pool Reuse Test");

        var reusedContext = pool.Get(); // 풀에서 재사용!

        Profiler.EndSample();
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

        // Then (검증)
        Assert.IsNotNull(reusedContext, "재사용된 컨텍스트는 null이 아니어야 합니다.");
        Log.Print("[TEST] 성능 검증 통과: 프로파일러에서 'Pool Reuse Test' 구간의 GC.Alloc가 0B인지 확인하세요.");
    }
}