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
            // --- 신규 모듈형 AI 설정 ---
            EditorGUILayout.LabelField("신규 모듈형 AI 설정", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("initialBehavior"));

            // ▼▼▼ [5단계 추가] 새로 추가된 '글로벌 패시브 규칙' 리스트를 Inspector에 그려주는 코드 ▼▼▼
            // 두 번째 인자인 'true'는 리스트의 내용물(자식 요소)까지 모두 그려달라는 옵션입니다.
            EditorGUILayout.PropertyField(serializedObject.FindProperty("globalModifierRules"), true);

            EditorGUI.indentLevel--;
        }
        else
        {
            // --- 구버전 FSM AI 설정 ---
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("구버전(FSM) AI 설정", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("behaviorType"));
            // ... (이하 구버전 AI 설정 UI는 기존과 동일)
            EditorGUI.indentLevel--;
        }


        // (가독성을 위해 나머지 특수능력 UI는 아래로 이동)
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

        serializedObject.ApplyModifiedProperties();
    }
}