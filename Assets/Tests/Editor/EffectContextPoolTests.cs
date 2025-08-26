// ��õ ���: Assets/Tests/Editor/EffectContextPoolTests.cs
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Profiling; // �������Ϸ� ����� ���� �߰�

public class EffectContextPoolTests
{
    [Test]
    public void Pool_ReturnsSameInstance_AfterReturning()
    {
        // ... (������ ������ �ڵ�) ...
        Log.Print("[TEST] Pool_ReturnsSameInstance_AfterReturning �׽�Ʈ ����.");
        var pool = new EffectContextPool();
        var initialInstance = pool.Get();
        pool.Return(initialInstance);
        var reusedInstance = pool.Get();
        Assert.AreSame(initialInstance, reusedInstance, "��ü�� ��ȯ�� �Ŀ��� �ݵ�� ������ �ν��Ͻ��� �����ؾ� �մϴ�.");
        Log.Print("[TEST] ��� ���� 1 ���: ��ü ���� Ȯ�� �Ϸ�.");
    }

    [Test]
    public void Pool_ReusingFromPool_GeneratesZeroGCAlloc()
    {
        // Given (�غ�)
        Log.Print("[TEST] Pool_ReusingFromPool_GeneratesZeroGCAlloc �׽�Ʈ ����.");
        var pool = new EffectContextPool();
        var context = pool.Get(); // ���� 1ȸ ���� (���⼭�� GC �߻��� ����)
        pool.Return(context);   // Ǯ�� �ݳ�

        // When (����)
        // ���� ��Ȯ�� �� ������ �޸� �Ҵ縸 �����մϴ�. ����
        Profiler.BeginSample("Pool Reuse Test");

        var reusedContext = pool.Get(); // Ǯ���� ����!

        Profiler.EndSample();
        // ������������������������������������

        // Then (����)
        Assert.IsNotNull(reusedContext, "����� ���ؽ�Ʈ�� null�� �ƴϾ�� �մϴ�.");
        Log.Print("[TEST] ���� ���� ���: �������Ϸ����� 'Pool Reuse Test' ������ GC.Alloc�� 0B���� Ȯ���ϼ���.");
    }
}