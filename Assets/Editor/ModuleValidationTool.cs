using UnityEngine;
using UnityEditor;

public class ModuleValidationTool : EditorWindow
{
    [MenuItem("Tools/v8.0/Validate Modules")]
    public static void ShowWindow()
    {
        GetWindow<ModuleValidationTool>("Module Validator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Module Validation Tool", EditorStyles.boldLabel);

        if (GUILayout.Button("Validate AreaEffectSO Assets"))
        {
            ValidateAreaEffectAssets();
        }

        // ▼▼▼ 이 버튼들을 OnGUI() 안으로 옮겨야 합니다! ▼▼▼
        if (GUILayout.Button("Validate StatusEffectSO Assets"))
        {
            // 나중에 구현할 예정
        }

        if (GUILayout.Button("Validate LifestealEffectSO Assets"))
        {
            // 나중에 구현할 예정
        }

        if (GUILayout.Button("Validate ALL Modules"))
        {
            // 나중에 구현할 예정
        }
    }

    private void ValidateAreaEffectAssets()
    {
        string[] guids = AssetDatabase.FindAssets("t:AreaEffectSO");
        int validCount = 0;
        int totalCount = guids.Length;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AreaEffectSO asset = AssetDatabase.LoadAssetAtPath<AreaEffectSO>(path);

            bool isValid = true;

            // 검증 로직
            if (asset.effectDuration <= 0)
            {
                Debug.LogError($"[Validation] {asset.name}: effectDuration must be > 0");
                isValid = false;
            }

            if (asset.effectExpansionSpeed <= 0)
            {
                Debug.LogError($"[Validation] {asset.name}: effectExpansionSpeed must be > 0");
                isValid = false;
            }

            if (asset.effectPrefabRef == null || !asset.effectPrefabRef.RuntimeKeyIsValid())
            {
                Debug.LogWarning($"[Validation] {asset.name}: effectPrefabRef is not assigned or invalid");
            }

            if (isValid) validCount++;
        } // ← foreach 여기서 끝!

        Debug.Log($"<color=cyan>[Validation Complete]</color> AreaEffectSO: {validCount}/{totalCount} assets valid");
    }
}
