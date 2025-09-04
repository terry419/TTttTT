/* ���: TTttTT/Assets/Editor/MonsterDataSOEditor.cs */
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MonsterDataSO))]
public class MonsterDataSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        MonsterDataSO data = (MonsterDataSO)target;

        // --- ����� ���� �ʵ带 �������� �׸��ϴ�. ---
        EditorGUILayout.LabelField("�⺻ ����", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("monsterID"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("monsterName"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("�ɷ�ġ", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxHealth"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("moveSpeed"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("contactDamage"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("�ൿ ����", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("behaviorType"));

        switch (data.behaviorType)
        {
            case MonsterBehaviorType.Patrol:
                // ... Patrol UI ...
                break;

            case MonsterBehaviorType.Flee:
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Flee Ÿ�� ���� �Ķ����", EditorStyles.boldLabel);
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
            EditorGUILayout.LabelField("Patrol Ÿ�� ���� �Ķ����", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("playerDetectionRadius"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("loseSightRadius"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("patrolRadius"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("patrolSpeedMultiplier"));
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Ư�� �ɷ�", EditorStyles.boldLabel);


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
        EditorGUILayout.LabelField("������", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("prefabRef"));

        serializedObject.ApplyModifiedProperties();
    }
}