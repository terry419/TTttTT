using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI; // InputSystemUIInputModule ����� ���� �߰�
using UnityEditor.SceneManagement;

public class EventSystemUpgrader
{
    // �޴� ��� "Tools/Upgrade/Upgrade EventSystems to New Input System"�� �޴� �������� �߰��մϴ�.
    [MenuItem("Tools/Upgrade/Upgrade EventSystems to New Input System")]
    private static void UpgradeEventSystems()
    {
        if (!EditorUtility.DisplayDialog("EventSystem ���׷��̵�",
            "������Ʈ�� ��� ���� �ִ� EventSystem�� ���ο� Input System���� ��ü�Ͻðڽ��ϱ�? " +
            "�� �۾��� ��� ���� �����ϸ�, �ǵ��� �� �����ϴ�.", "����", "���"))
        {
            return;
        }

        // ���� ������ �ִ� ��� ���� ��θ� �����ɴϴ�.
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled) continue;

            // ���� ���ϴ�.
            EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);

            // ���� �ִ� ��� StandaloneInputModule�� ã���ϴ�.
            StandaloneInputModule[] oldModules = Object.FindObjectsOfType<StandaloneInputModule>();
            foreach (var module in oldModules)
            {
                GameObject eventSystemObject = module.gameObject;
                Debug.Log($"�� '{scene.path}'�� '{eventSystemObject.name}'���� EventSystem�� ���׷��̵��մϴ�.");

                // ���� ����� �ı��ϰ� ���ο� ����� �߰��մϴ�.
                Object.DestroyImmediate(module);
                eventSystemObject.AddComponent<InputSystemUIInputModule>();
            }

            // ���� �����մϴ�.
            EditorSceneManager.SaveOpenScenes();
        }

        EditorUtility.DisplayDialog("�Ϸ�", "��� ���� EventSystem ���׷��̵带 �Ϸ��߽��ϴ�.", "Ȯ��");
    }
}