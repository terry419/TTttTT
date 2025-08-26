// ���ϸ�: SetupVerificationTests.cs (Editor ���� ���� ��ġ)
using NUnit.Framework;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;

public class SetupVerificationTests
{
    [Test]
    public void Log_Print_ShouldNotBeCalled_InReleaseBuild()
    {
        // Given
        Log.ResetCount();
        bool isDevelopmentBuild = EditorUserBuildSettings.development;

        // When
        // ������ ���� ���� �÷��׸� false�� �����Ͽ� ������ ���� ȯ���� ���
        EditorUserBuildSettings.development = false;
        Log.Print("This log should not appear or count in a release build.");

        // Then
        // Conditional �Ӽ����� ���� Log.Print �޼ҵ� ��ü�� �����Ͽ��� ���ܵǹǷ�
        // LogCount�� �������� �ʾƾ� �մϴ�.
        Assert.AreEqual(0, Log.LogCount, "Log.Print()�� ������ ���忡�� ȣ��Ǿ����ϴ�!");

        // ���� �������� ����
        EditorUserBuildSettings.development = isDevelopmentBuild;
        Log.ResetCount();
    }

    [Test]
    public void Addressables_DataEffectLabel_ShouldContainAssets()
    {
        // Given
        const string labelToTest = "data_effect";

        // Addressables ������ �ε��մϴ�.
        AddressableAssetSettings.CleanPlayerContent();
        AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);

        // When
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var entries = settings.groups.SelectMany(g => g.entries).Where(e => e.labels.Contains(labelToTest)).ToList();

        // Then
        Assert.Greater(entries.Count, 0, $"'{labelToTest}' ���̺��� ���� ������ �ϳ� �̻� �־�� �մϴ�. Card_Modules �׷� ������ Ȯ���ϼ���.");
        Log.Print($"[CHK0.5<Setup><Complete>] Addressables label '{labelToTest}' verified with {entries.Count} assets.");
    }
}