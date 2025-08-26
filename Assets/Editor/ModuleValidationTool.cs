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

        // ���� �� ��ư���� OnGUI() ������ �Űܾ� �մϴ�! ����
        if (GUILayout.Button("Validate StatusEffectSO Assets"))
        {
            // ���߿� ������ ����
        }

        if (GUILayout.Button("Validate LifestealEffectSO Assets"))
        {
            // ���߿� ������ ����
        }

        if (GUILayout.Button("Validate ALL Modules"))
        {
            // ���߿� ������ ����
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

            // ���� ����
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
        } // �� foreach ���⼭ ��!

        Debug.Log($"<color=cyan>[Validation Complete]</color> AreaEffectSO: {validCount}/{totalCount} assets valid");
    }
}
