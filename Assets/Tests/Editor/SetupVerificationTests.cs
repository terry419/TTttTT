// 파일명: SetupVerificationTests.cs (Editor 폴더 내에 위치)
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
        // 강제로 개발 빌드 플래그를 false로 설정하여 릴리즈 빌드 환경을 모방
        EditorUserBuildSettings.development = false;
        Log.Print("This log should not appear or count in a release build.");

        // Then
        // Conditional 속성으로 인해 Log.Print 메소드 자체가 컴파일에서 제외되므로
        // LogCount는 증가하지 않아야 합니다.
        Assert.AreEqual(0, Log.LogCount, "Log.Print()가 릴리즈 빌드에서 호출되었습니다!");

        // 원래 설정으로 복원
        EditorUserBuildSettings.development = isDevelopmentBuild;
        Log.ResetCount();
    }

    [Test]
    public void Addressables_DataEffectLabel_ShouldContainAssets()
    {
        // Given
        const string labelToTest = "data_effect";

        // Addressables 설정을 로드합니다.
        AddressableAssetSettings.CleanPlayerContent();
        AddressableAssetSettings.BuildPlayerContent(out AddressablesPlayerBuildResult result);

        // When
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var entries = settings.groups.SelectMany(g => g.entries).Where(e => e.labels.Contains(labelToTest)).ToList();

        // Then
        Assert.Greater(entries.Count, 0, $"'{labelToTest}' 레이블을 가진 에셋이 하나 이상 있어야 합니다. Card_Modules 그룹 설정을 확인하세요.");
        Log.Print($"[CHK0.5<Setup><Complete>] Addressables label '{labelToTest}' verified with {entries.Count} assets.");
    }
}