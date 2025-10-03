using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;
using UnityEngine.AddressableAssets; // AssetReferenceT 사용을 위해 추가

[CustomEditor(typeof(NewCardDataSO))]
public class CardDataSOEditor : Editor
{
    // 각 모듈 Editor 인스턴스를 관리하여, 인스펙터의 상태를 유지합니다.
    private readonly Dictionary<Object, Editor> moduleEditors = new Dictionary<Object, Editor>();

    public override void OnInspectorGUI()
    {
        // 현재 NewCardDataSO의 변경사항을 업데이트
        serializedObject.Update();

        // "m_Script", "modules", "basicInfo" 필드를 제외하고 나머지 기본 필드를 자동으로 그립니다.
        DrawPropertiesExcluding(serializedObject, "m_Script", "modules", "basicInfo");

        // basicInfo 필드를 수동으로 그립니다.
        SerializedProperty basicInfoProperty = serializedObject.FindProperty("basicInfo");
        if (basicInfoProperty != null)
        {
            Debug.Log("Drawing cardID...");
            EditorGUILayout.PropertyField(basicInfoProperty.FindPropertyRelative("cardID"), new
GUIContent("Card ID"));
            Debug.Log("Drawing cardName...");
            EditorGUILayout.PropertyField(basicInfoProperty.FindPropertyRelative("cardName"), new
GUIContent("Card Name"));
            Debug.Log("Drawing cardIllustration...");
            EditorGUILayout.PropertyField(basicInfoProperty.FindPropertyRelative("cardIllustration"), new
GUIContent("Card Illustration"));
            Debug.Log("Drawing type...");
            EditorGUILayout.PropertyField(basicInfoProperty.FindPropertyRelative("type"), new
GUIContent("Card Type"));
            Debug.Log("Drawing rarity...");
            EditorGUILayout.PropertyField(basicInfoProperty.FindPropertyRelative("rarity"), new
GUIContent("Card Rarity"));
            Debug.Log("Drawing effectDescription...");
            EditorGUILayout.PropertyField(basicInfoProperty.FindPropertyRelative("effectDescription"), new
 GUIContent("Effect Description"));
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("모듈 정보 설정", EditorStyles.boldLabel);

        // "modules" 리스트 프로퍼티를 그립니다.
        SerializedProperty modulesProperty = serializedObject.FindProperty("modules");

        modulesProperty.isExpanded = EditorGUILayout.Foldout(modulesProperty.isExpanded, "모듈 정보 설정",
 true, EditorStyles.foldoutHeader);

        if (modulesProperty.isExpanded)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(modulesProperty.FindPropertyRelative("Array.size"));

            // 리스트의 각 항목을 그립니다.
            for (int i = 0; i < modulesProperty.arraySize; i++)
            {
                SerializedProperty moduleEntryProperty = modulesProperty.GetArrayElementAtIndex(i);

                // ModuleEntry의 description 필드를 그립니다.
                SerializedProperty descriptionProperty =
moduleEntryProperty.FindPropertyRelative("description");
                EditorGUILayout.PropertyField(descriptionProperty);

                // ModuleEntry의 AssetReference 필드를 그립니다.
                SerializedProperty moduleRefProperty =
moduleEntryProperty.FindPropertyRelative("moduleReference");
                EditorGUILayout.PropertyField(moduleRefProperty, new GUIContent("모듈 에셋 (ModuleAsset)"));

                  // AssetReference에 실제 에셋이 할당되어 있는지 확인합니다.
                var referencedModuleGuid =
moduleRefProperty.FindPropertyRelative("m_AssetGUID").stringValue;

                if (!string.IsNullOrEmpty(referencedModuleGuid))
                {
                    var entry =
AddressableAssetSettingsDefaultObject.Settings.FindAssetEntry(referencedModuleGuid);
                    if (entry != null)
                    {
                        // [주의] .GetAsset() 대신 .MainAsset 속성을 사용합니다.
                        var asset = entry.MainAsset as CardEffectSO;

                        if (asset != null)
                        {
                            // 할당된 에셋을 그리기 위한 Foldout을 사용합니다.
                            bool isExpanded = EditorGUILayout.Foldout(
                                GetFoldoutState(asset),
                                $"'{asset.name}' 모듈 상세 정보",
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

        // 변경된 내용을 적용합니다.
        serializedObject.ApplyModifiedProperties();
    }

    // Foldout 상태 저장을 위한 Helper 메서드
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
        // 에디터가 비활성화될 때 생성된 모든 서브 에디터를 제거하여 메모리 누수를 방지합니다.
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