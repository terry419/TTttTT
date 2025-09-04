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

        // --- 헤더와 공통 필드를 수동으로 그립니다. ---
        EditorGUILayout.LabelField("기본 정보", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("monsterID"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("monsterName"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("능력치", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxHealth"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("moveSpeed"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("contactDamage"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("행동 패턴", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("behaviorType"));

        switch (data.behaviorType)
        {
            case MonsterBehaviorType.Patrol:
                // ... Patrol UI ...
                break;

            case MonsterBehaviorType.Flee:
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Flee 타입 전용 파라미터", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("fleeTriggerRadius"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("fleeSafeRadius"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("fleeSpeedMultiplier"));
                EditorGUI.indentLevel--;
                break;
        }

        if (data.behaviorType == MonsterBehaviorType.Patrol)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Patrol 타입 전용 파라미터", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("playerDetectionRadius"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("loseSightRadius"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("patrolRadius"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("patrolSpeedMultiplier"));
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("특수 능력", EditorStyles.boldLabel);


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

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("프리팹", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("prefabRef"));

        serializedObject.ApplyModifiedProperties();
    }
}