using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class FindMissingScripts
{
    // 유니티 에디터 상단 메뉴에 "Tools/Find Missing Scripts In All Scenes" 항목을 추가합니다.
    [MenuItem("Tools/Find Missing Scripts In All Scenes")]
    public static void FindAndSelectMissingScripts()
    {
        Debug.Log("--- 모든 씬에서 Missing Script 검사를 시작합니다. ---");

        // 현재 열려있던 씬의 경로를 기억해둡니다. (검사가 끝나고 돌아오기 위함)
        string originalScenePath = EditorSceneManager.GetActiveScene().path;

        // Build Settings에 등록된 모든 씬 목록을 가져옵니다.
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            // 활성화된 씬만 검사합니다.
            if (scene.enabled)
            {
                // 해당 씬을 엽니다.
                EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);

                // 현재 씬에 있는 모든 게임 오브젝트를 가져옵니다.
                GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

                foreach (GameObject go in allObjects)
                {
                    // 오브젝트에 붙어있는 모든 컴포넌트를 가져옵니다.
                    Component[] components = go.GetComponents<Component>();
                    foreach (Component c in components)
                    {
                        // 컴포넌트가 null 이라면, 이것이 바로 "Missing Script" 입니다.
                        if (c == null)
                        {
                            // 콘솔에 어떤 씬의 어떤 오브젝트에 문제가 있는지 에러 메시지를 출력합니다.
                            Debug.LogError($"[Missing Script 발견!] 씬: '{scene.path}' / 오브젝트: '{go.name}'", go);
                        }
                    }
                }
            }
        }

        // 검사가 끝났으니 원래 작업하던 씬으로 돌아갑니다.
        EditorSceneManager.OpenScene(originalScenePath);
        Debug.Log("--- 모든 씬 검사 완료. ---");
    }
}