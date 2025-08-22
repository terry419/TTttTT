using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class FindMissingScripts
{
    // ����Ƽ ������ ��� �޴��� "Tools/Find Missing Scripts In All Scenes" �׸��� �߰��մϴ�.
    [MenuItem("Tools/Find Missing Scripts In All Scenes")]
    public static void FindAndSelectMissingScripts()
    {
        Debug.Log("--- ��� ������ Missing Script �˻縦 �����մϴ�. ---");

        // ���� �����ִ� ���� ��θ� ����صӴϴ�. (�˻簡 ������ ���ƿ��� ����)
        string originalScenePath = EditorSceneManager.GetActiveScene().path;

        // Build Settings�� ��ϵ� ��� �� ����� �����ɴϴ�.
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            // Ȱ��ȭ�� ���� �˻��մϴ�.
            if (scene.enabled)
            {
                // �ش� ���� ���ϴ�.
                EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);

                // ���� ���� �ִ� ��� ���� ������Ʈ�� �����ɴϴ�.
                GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

                foreach (GameObject go in allObjects)
                {
                    // ������Ʈ�� �پ��ִ� ��� ������Ʈ�� �����ɴϴ�.
                    Component[] components = go.GetComponents<Component>();
                    foreach (Component c in components)
                    {
                        // ������Ʈ�� null �̶��, �̰��� �ٷ� "Missing Script" �Դϴ�.
                        if (c == null)
                        {
                            // �ֿܼ� � ���� � ������Ʈ�� ������ �ִ��� ���� �޽����� ����մϴ�.
                            Debug.LogError($"[Missing Script �߰�!] ��: '{scene.path}' / ������Ʈ: '{go.name}'", go);
                        }
                    }
                }
            }
        }

        // �˻簡 �������� ���� �۾��ϴ� ������ ���ư��ϴ�.
        EditorSceneManager.OpenScene(originalScenePath);
        Debug.Log("--- ��� �� �˻� �Ϸ�. ---");
    }
}