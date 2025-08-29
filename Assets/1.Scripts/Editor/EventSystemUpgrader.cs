using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI; // InputSystemUIInputModule 사용을 위해 추가
using UnityEditor.SceneManagement;

public class EventSystemUpgrader
{
    // 메뉴 경로 "Tools/Upgrade/Upgrade EventSystems to New Input System"에 메뉴 아이템을 추가합니다.
    [MenuItem("Tools/Upgrade/Upgrade EventSystems to New Input System")]
    private static void UpgradeEventSystems()
    {
        if (!EditorUtility.DisplayDialog("EventSystem 업그레이드",
            "프로젝트의 모든 씬에 있는 EventSystem을 새로운 Input System으로 교체하시겠습니까? " +
            "이 작업은 모든 씬을 저장하며, 되돌릴 수 없습니다.", "진행", "취소"))
        {
            return;
        }

        // 빌드 설정에 있는 모든 씬의 경로를 가져옵니다.
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled) continue;

            // 씬을 엽니다.
            EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);

            // 씬에 있는 모든 StandaloneInputModule을 찾습니다.
            StandaloneInputModule[] oldModules = Object.FindObjectsOfType<StandaloneInputModule>();
            foreach (var module in oldModules)
            {
                GameObject eventSystemObject = module.gameObject;
                Debug.Log($"씬 '{scene.path}'의 '{eventSystemObject.name}'에서 EventSystem을 업그레이드합니다.");

                // 기존 모듈을 파괴하고 새로운 모듈을 추가합니다.
                Object.DestroyImmediate(module);
                eventSystemObject.AddComponent<InputSystemUIInputModule>();
            }

            // 씬을 저장합니다.
            EditorSceneManager.SaveOpenScenes();
        }

        EditorUtility.DisplayDialog("완료", "모든 씬의 EventSystem 업그레이드를 완료했습니다.", "확인");
    }
}