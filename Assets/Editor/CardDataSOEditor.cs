// 파일 경로: Assets/Editor/CardDataSOEditor.cs
using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using System.Collections.Generic;
using UnityEngine.AddressableAssets; // AssetReferenceT를 위해 추가

[CustomEditor(typeof(NewCardDataSO))]
public class CardDataSOEditor : Editor
{
    // 각 모듈의 Editor 인스턴스를 저장하여, 인스펙터 상태를 유지합니다.
    private readonly Dictionary<Object, Editor> moduleEditors = new Dictionary<Object, Editor>();

    public override void OnInspectorGUI()
    {
        // 원본 NewCardDataSO의 변경사항을 기록 시작
        serializedObject.Update();

        // "m_Script"와 "modules" 필드를 제외한 모든 기본 필드(baseDamage, projectileCount 등)를 자동으로 그려줍니다.
        DrawPropertiesExcluding(serializedObject, "m_Script", "modules");

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("모듈 조립 슬롯", EditorStyles.boldLabel);

        // "modules" 리스트 프로퍼티를 가져옵니다.
        SerializedProperty modulesProperty = serializedObject.FindProperty("modules");

        modulesProperty.isExpanded = EditorGUILayout.Foldout(modulesProperty.isExpanded, "모듈 조립 슬롯", true, EditorStyles.foldoutHeader);

        if (modulesProperty.isExpanded)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(modulesProperty.FindPropertyRelative("Array.size"));

            // 리스트의 각 모듈 항목을 순회합니다.
            for (int i = 0; i < modulesProperty.arraySize; i++)
            {
                SerializedProperty moduleEntryProperty = modulesProperty.GetArrayElementAtIndex(i);

                // ModuleEntry의 description 필드를 그립니다.
                SerializedProperty descriptionProperty = moduleEntryProperty.FindPropertyRelative("description");
                EditorGUILayout.PropertyField(descriptionProperty);

                // ModuleEntry의 AssetReference 필드를 그립니다.
                SerializedProperty moduleRefProperty = moduleEntryProperty.FindPropertyRelative("moduleReference");
                EditorGUILayout.PropertyField(moduleRefProperty, new GUIContent("모듈 에셋 (Module Asset)"));

                // AssetReference에 실제 에셋이 할당되어 있는지 확인합니다.
                var referencedModuleGuid = moduleRefProperty.FindPropertyRelative("m_AssetGUID").stringValue;

                if (!string.IsNullOrEmpty(referencedModuleGuid))
                {
                    var entry = AddressableAssetSettingsDefaultObject.Settings.FindAssetEntry(referencedModuleGuid);
                    if (entry != null)
                    {
                        // [수정] .GetAsset() 대신 .MainAsset 속성을 사용합니다.
                        var asset = entry.MainAsset as CardEffectSO;

                        if (asset != null)
                        {
                            // 인라인 에디터를 그리기 위한 Foldout을 생성합니다.
                            bool isExpanded = EditorGUILayout.Foldout(
                                GetFoldoutState(asset),
                                $"└ '{asset.name}' 모듈 내용 편집",
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

        // 변경된 사항이 있다면 적용합니다.
        serializedObject.ApplyModifiedProperties();
    }

    // Foldout 상태 저장을 위한 Helper 메소드들
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
        // 에디터가 비활성화될 때 생성된 모든 인라인 에디터를 정리하여 메모리 누수를 방지합니다.
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