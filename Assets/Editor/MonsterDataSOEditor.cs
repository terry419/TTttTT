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

        EditorGUILayout.LabelField("기본 정보", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("monsterID"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("monsterName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("prefabRef"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("능력치", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxHealth"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("moveSpeed"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("contactDamage"));

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("--- AI 시스템 ---", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("useNewAI"));

        if (data.useNewAI)
        {
            EditorGUILayout.LabelField("신규 모듈형 AI 설정", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("initialBehavior"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("globalModifierRules"), true);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("--- 특수 능력 (공통) ---", EditorStyles.boldLabel);

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

        EditorGUILayout.PropertyField(serializedObject.FindProperty("onDeathZoneEffect"));

        serializedObject.ApplyModifiedProperties();
    }
}