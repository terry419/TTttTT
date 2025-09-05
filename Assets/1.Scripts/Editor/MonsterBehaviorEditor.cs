// 경로: ./TTttTT/Assets/1.Scripts/Editor/MonsterBehaviorEditor.cs
using UnityEngine;
using UnityEditor; // Unity 에디터 기능을 사용하기 위해 필수입니다.

/// <summary>
/// [2단계 수정] MonsterBehavior ScriptableObject의 Inspector 창을 커스터마이징하는 '지능형 Inspector'입니다.
/// 이 스크립트는 반드시 'Editor' 폴더 안에 있어야 합니다.
/// </summary>
[CustomEditor(typeof(MonsterBehavior), true)] // MonsterBehavior를 상속하는 모든 클래스에 이 Inspector를 적용합니다.
public class MonsterBehaviorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 1. 기본 Inspector 창을 먼저 그대로 그립니다. (Transitions 리스트 등이 표시됩니다)
        DrawDefaultInspector();

        // 2. 현재 Inspector가 보고 있는 원본 에셋을 가져옵니다.
        MonsterBehavior behavior = (MonsterBehavior)target;

        // 3. 에셋의 transitions 리스트에 문제가 있는지 검사합니다.
        bool hasError = false;
        if (behavior.transitions != null)
        {
            foreach (var transition in behavior.transitions)
            {
                // 만약 Decision이나 NextBehavior 둘 중 하나라도 비어있다면, 문제가 있는 것으로 판단합니다.
                if (transition.decision == null || transition.nextBehavior == null)
                {
                    hasError = true;
                    break; // 하나라도 찾으면 더 이상 검사할 필요 없이 반복을 멈춥니다.
                }
            }
        }

        // 4. 만약 위 검사에서 문제가 발견되었다면 (hasError == true),
        //    Inspector 창에 직접 노란색 경고 상자를 그립니다.
        if (hasError)
        {
            EditorGUILayout.HelpBox("경고: Transitions 목록에 비어있는 'Decision' 또는 'Next Behavior' 슬롯이 있습니다. 반드시 모든 슬롯을 채워주세요.", MessageType.Warning);
        }
    }
}