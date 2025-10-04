// ���� ���: Assets/Editor/CardDataSOEditor.cs
using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using System.Collections.Generic;
using UnityEngine.AddressableAssets; // AssetReferenceT�� ���� �߰�

[CustomEditor(typeof(NewCardDataSO))]
public class CardDataSOEditor : Editor
{
    // �� ����� Editor �ν��Ͻ��� �����Ͽ�, �ν����� ���¸� �����մϴ�.
    private readonly Dictionary<Object, Editor> moduleEditors = new Dictionary<Object, Editor>();

    public override void OnInspectorGUI()
    {
        // ���� NewCardDataSO�� ��������� ��� ����
        serializedObject.Update();

        // "m_Script"�� "modules" �ʵ带 ������ ��� �⺻ �ʵ�(baseDamage, projectileCount ��)�� �ڵ����� �׷��ݴϴ�.
        DrawPropertiesExcluding(serializedObject, "m_Script", "modules");

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("��� ���� ����", EditorStyles.boldLabel);

        // "modules" ����Ʈ ������Ƽ�� �����ɴϴ�.
        SerializedProperty modulesProperty = serializedObject.FindProperty("modules");

        modulesProperty.isExpanded = EditorGUILayout.Foldout(modulesProperty.isExpanded, "��� ���� ����", true, EditorStyles.foldoutHeader);

        if (modulesProperty.isExpanded)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(modulesProperty.FindPropertyRelative("Array.size"));

            // ����Ʈ�� �� ��� �׸��� ��ȸ�մϴ�.
            for (int i = 0; i < modulesProperty.arraySize; i++)
            {
                SerializedProperty moduleEntryProperty = modulesProperty.GetArrayElementAtIndex(i);

                // ModuleEntry�� description �ʵ带 �׸��ϴ�.
                SerializedProperty descriptionProperty = moduleEntryProperty.FindPropertyRelative("description");
                EditorGUILayout.PropertyField(descriptionProperty);

                // ModuleEntry�� AssetReference �ʵ带 �׸��ϴ�.
                SerializedProperty moduleRefProperty = moduleEntryProperty.FindPropertyRelative("moduleReference");
                EditorGUILayout.PropertyField(moduleRefProperty, new GUIContent("��� ���� (Module Asset)"));

                // AssetReference�� ���� ������ �Ҵ�Ǿ� �ִ��� Ȯ���մϴ�.
                var referencedModuleGuid = moduleRefProperty.FindPropertyRelative("m_AssetGUID").stringValue;

                if (!string.IsNullOrEmpty(referencedModuleGuid))
                {
                    var entry = AddressableAssetSettingsDefaultObject.Settings.FindAssetEntry(referencedModuleGuid);
                    if (entry != null)
                    {
                        // [����] .GetAsset() ��� .MainAsset �Ӽ��� ����մϴ�.
                        var asset = entry.MainAsset as CardEffectSO;

                        if (asset != null)
                        {
                            // �ζ��� �����͸� �׸��� ���� Foldout�� �����մϴ�.
                            bool isExpanded = EditorGUILayout.Foldout(
                                GetFoldoutState(asset),
                                $"�� '{asset.name}' ��� ���� ����",
                                true,
                                EditorStyles.foldoutHeader
                            );

                            SetFoldoutState(asset, isExpanded);

                            if (isExpanded)
                            {
                                EditorGUI.indentLevel++;

                                if (!moduleEditors.ContainsKey(asset))
                                {
                                    moduleEditors[asset] = CreateEditor(asset);
                                }

                                moduleEditors[asset].OnInspectorGUI();

                                EditorGUI.indentLevel--;
                            }
                        }
                    }
                }
                EditorGUILayout.Separator();
            }
            EditorGUI.indentLevel--;
        }

        // ����� ������ �ִٸ� �����մϴ�.
        serializedObject.ApplyModifiedProperties();
    }

    // Foldout ���� ������ ���� Helper �޼ҵ��
    private bool GetFoldoutState(Object asset)
    {
        return SessionState.GetBool(asset.GetInstanceID().ToString(), false);
    }

    private void SetFoldoutState(Object asset, bool state)
    {
        SessionState.SetBool(asset.GetInstanceID().ToString(), state);
    }

    private void OnDisable()
    {
        // �����Ͱ� ��Ȱ��ȭ�� �� ������ ��� �ζ��� �����͸� �����Ͽ� �޸� ������ �����մϴ�.
        foreach (var editor in moduleEditors.Values)
        {
            if (editor != null)
            {
                DestroyImmediate(editor);
            }
        }
        moduleEditors.Clear();
    }
}