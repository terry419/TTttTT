// ���ϸ�: SmokeTests.cs (Editor ���� ���� ��ġ)
using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEngine;

public class SmokeTests
{
    [UnityTest]
    public IEnumerator CoreAttackLoop_SmokeTest()
    {
        // �� �׽�Ʈ�� 9�ܰ迡�� ������ �����Դϴ�.
        // 1. �� ����
        // 2. �׽�Ʈ�� ī�� ���� ����
        // 3. �α� ������ ����
        // 4. ���
        // 5. ����
        yield return null;
        Assert.Pass("Smoke test placeholder. To be implemented in Step 9.");
    }
}