/* 경로: TTttTT/Assets/Editor/MonsterDataSOEditor.cs */
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MonsterDataSO))]
public class MonsterDataSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        MonsterDataSO data = (MonsterDataSO)target;

        // --- 기본 정보, 능력치 ---
        EditorGUILayout.LabelField("기본 정보", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("monsterID"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("monsterName"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("능력치", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxHealth"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("moveSpeed"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("contactDamage"));

        // --- 행동 패턴 ---
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("행동 패턴", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("behaviorType"));

        switch (data.behaviorType)
        {
            case MonsterBehaviorType.Patrol:
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Patrol 타입 전용 파라미터", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("playerDetectionRadius"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("loseSightRadius"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("patrolRadius"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("patrolSpeedMultiplier"));
                EditorGUI.indentLevel--;
                break;
        }

        // --- 특수 능력 ---
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("--- 특수 능력 ---", EditorStyles.boldLabel);
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("canFlee"));
        if (data.canFlee)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fleeCondition"));
            
            // FleeCondition 값에 따라 다른 UI를 보여줍니다.
            switch (data.fleeCondition)
            {
                case FleeCondition.PlayerProximity:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("fleeTriggerRadius"));
                    break;
                case FleeCondition.LowHealth:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("fleeOnHealthPercentage"));
                    break;
                case FleeCondition.Outnumbered:
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("allyCheckRadius"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("fleeWhenAlliesLessThan"));
                    break;
            }
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fleeSafeRadius"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fleeSpeedMultiplier"));
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("canExplodeOnDeath"));
        if (data.canExplodeOnDeath)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("explosionVfxRef"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("explosionDelay"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("explosionDamage"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("explosionRadius"));
            EditorGUI.indentLevel--;
        }

        // ... 프리팹 및 마무리 ...
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("프리팹", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("prefabRef"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("--- [신규] 모듈형 AI 설정 ---", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("useNewAI"));
        if (data.useNewAI)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("initialBehavior"));
        }

        serializedObject.ApplyModifiedProperties();
    }
}