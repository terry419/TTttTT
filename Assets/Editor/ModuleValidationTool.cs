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

        if (GUILayout.Button("Validate ProjectileEffectSO Assets"))
        {
            ValidateProjectileEffectAssets();
        }

        if (GUILayout.Button("Validate LifestealEffectSO Assets"))
        {
            ValidateLifestealEffectAssets();
        }

        if (GUILayout.Button("Validate ApplyStatusEffectSO Assets"))
        {
            ValidateApplyStatusEffectAssets();
        }

        if (GUILayout.Button("Validate ApplyBuffToCasterSO Assets"))
        {
            ValidateApplyBuffToCasterAssets();
        }

        if (GUILayout.Button("Validate RandomEffectSO Assets"))
        {
            ValidateRandomEffectAssets();
        }

        if (GUILayout.Button("Validate ConditionalEffectSO Assets"))
        {
            ValidateConditionalEffectAssets();
        }

        GUILayout.Space(10);

        if (GUILayout.Button(" Validate ALL v8.0 Modules"))
        {
            ValidateAllModules();
        }
    }

    private void ValidateAllModules()
    {
        Debug.Log("<color=yellow>[VALIDATION START]</color> Validating all v8.0 modules...");

        ValidateAreaEffectAssets();
        ValidateProjectileEffectAssets();
        ValidateLifestealEffectAssets();
        ValidateApplyStatusEffectAssets();
        ValidateApplyBuffToCasterAssets();
        ValidateRandomEffectAssets();
        ValidateConditionalEffectAssets();

        Debug.Log("<color=green>[VALIDATION COMPLETE]</color> All v8.0 modules validated!");
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
        }

        Debug.Log($"<color=cyan>[Validation]</color> AreaEffectSO: {validCount}/{totalCount} assets valid");
    }

    private void ValidateProjectileEffectAssets()
    {
        string[] guids = AssetDatabase.FindAssets("t:ProjectileEffectSO");
        int validCount = 0;
        int totalCount = guids.Length;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ProjectileEffectSO asset = AssetDatabase.LoadAssetAtPath<ProjectileEffectSO>(path);

            bool isValid = true;

            if (asset.ricochetCount < 0)
            {
                Debug.LogError($"[Validation] {asset.name}: ricochetCount cannot be negative");
                isValid = false;
            }

            if (asset.pierceCount < 0)
            {
                Debug.LogError($"[Validation] {asset.name}: pierceCount cannot be negative");
                isValid = false;
            }

            if (asset.speedMultiplier <= 0)
            {
                Debug.LogError($"[Validation] {asset.name}: speedMultiplier must be > 0");
                isValid = false;
            }

            if (asset.sequentialPayloads != null)
            {
                foreach (var payload in asset.sequentialPayloads)
                {
                    if (payload.onBounceNumber < 0)
                    {
                        Debug.LogError($"[Validation] {asset.name}: SequentialPayload onBounceNumber cannot be negative");
                        isValid = false;
                    }
                }
            }

            if (isValid) validCount++;
        }

        Debug.Log($"<color=cyan>[Validation]</color> ProjectileEffectSO: {validCount}/{totalCount} assets valid");
    }

    private void ValidateLifestealEffectAssets()
    {
        string[] guids = AssetDatabase.FindAssets("t:LifestealEffectSO");
        int validCount = 0;
        int totalCount = guids.Length;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            LifestealEffectSO asset = AssetDatabase.LoadAssetAtPath<LifestealEffectSO>(path);

            bool isValid = true;

            if (asset.lifestealPercentage < 0 || asset.lifestealPercentage > 100)
            {
                Debug.LogError($"[Validation] {asset.name}: lifestealPercentage must be between 0-100");
                isValid = false;
            }

            if (isValid) validCount++;
        }

        Debug.Log($"<color=cyan>[Validation]</color> LifestealEffectSO: {validCount}/{totalCount} assets valid");
    }

    private void ValidateApplyStatusEffectAssets()
    {
        string[] guids = AssetDatabase.FindAssets("t:ApplyStatusEffectSO");
        int validCount = 0;
        int totalCount = guids.Length;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ApplyStatusEffectSO asset = AssetDatabase.LoadAssetAtPath<ApplyStatusEffectSO>(path);

            bool isValid = true;

            if (asset.statusToApply == null)
            {
                Debug.LogWarning($"[Validation] {asset.name}: statusToApply is not assigned");
                // Warning이므로 isValid는 true 유지
            }

            if (isValid) validCount++;
        }

        Debug.Log($"<color=cyan>[Validation]</color> ApplyStatusEffectSO: {validCount}/{totalCount} assets valid");
    }

    private void ValidateApplyBuffToCasterAssets()
    {
        string[] guids = AssetDatabase.FindAssets("t:ApplyBuffToCasterSO");
        int validCount = 0;
        int totalCount = guids.Length;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ApplyBuffToCasterSO asset = AssetDatabase.LoadAssetAtPath<ApplyBuffToCasterSO>(path);

            bool isValid = true;

            if (asset.buffToApply == null)
            {
                Debug.LogWarning($"[Validation] {asset.name}: buffToApply is not assigned");
            }

            if (isValid) validCount++;
        }

        Debug.Log($"<color=cyan>[Validation]</color> ApplyBuffToCasterSO: {validCount}/{totalCount} assets valid");
    }

    private void ValidateRandomEffectAssets()
    {
        string[] guids = AssetDatabase.FindAssets("t:RandomEffectSO");
        int validCount = 0;
        int totalCount = guids.Length;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            RandomEffectSO asset = AssetDatabase.LoadAssetAtPath<RandomEffectSO>(path);

            bool isValid = true;

            if (asset.effectPool == null || asset.effectPool.Count == 0)
            {
                Debug.LogWarning($"[Validation] {asset.name}: effectPool is empty");
            }

            if (isValid) validCount++;
        }

        Debug.Log($"<color=cyan>[Validation]</color> RandomEffectSO: {validCount}/{totalCount} assets valid");
    }

    private void ValidateConditionalEffectAssets()
    {
        string[] guids = AssetDatabase.FindAssets("t:ConditionalEffectSO");
        int validCount = 0;
        int totalCount = guids.Length;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ConditionalEffectSO asset = AssetDatabase.LoadAssetAtPath<ConditionalEffectSO>(path);

            bool isValid = true;

            if (asset.effectToTrigger == null || !asset.effectToTrigger.RuntimeKeyIsValid())
            {
                Debug.LogWarning($"[Validation] {asset.name}: effectToTrigger is not assigned or invalid");
            }

            if (isValid) validCount++;
        }

        Debug.Log($"<color=cyan>[Validation]</color> ConditionalEffectSO: {validCount}/{totalCount} assets valid");
    }
}
