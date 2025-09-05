// ���: ./TTttTT/Assets/1.Scripts/Editor/MonsterBehaviorEditor.cs
using UnityEngine;
using UnityEditor; // Unity ������ ����� ����ϱ� ���� �ʼ��Դϴ�.

/// <summary>
/// [2�ܰ� ����] MonsterBehavior ScriptableObject�� Inspector â�� Ŀ���͸���¡�ϴ� '������ Inspector'�Դϴ�.
/// �� ��ũ��Ʈ�� �ݵ�� 'Editor' ���� �ȿ� �־�� �մϴ�.
/// </summary>
[CustomEditor(typeof(MonsterBehavior), true)] // MonsterBehavior�� ����ϴ� ��� Ŭ������ �� Inspector�� �����մϴ�.
public class MonsterBehaviorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 1. �⺻ Inspector â�� ���� �״�� �׸��ϴ�. (Transitions ����Ʈ ���� ǥ�õ˴ϴ�)
        DrawDefaultInspector();

        // 2. ���� Inspector�� ���� �ִ� ���� ������ �����ɴϴ�.
        MonsterBehavior behavior = (MonsterBehavior)target;

        // 3. ������ transitions ����Ʈ�� ������ �ִ��� �˻��մϴ�.
        bool hasError = false;
        if (behavior.transitions != null)
        {
            foreach (var transition in behavior.transitions)
            {
                // ���� Decision�̳� NextBehavior �� �� �ϳ��� ����ִٸ�, ������ �ִ� ������ �Ǵ��մϴ�.
                if (transition.decision == null || transition.nextBehavior == null)
                {
                    hasError = true;
                    break; // �ϳ��� ã���� �� �̻� �˻��� �ʿ� ���� �ݺ��� ����ϴ�.
                }
            }
        }

        // 4. ���� �� �˻翡�� ������ �߰ߵǾ��ٸ� (hasError == true),
        //    Inspector â�� ���� ����� ��� ���ڸ� �׸��ϴ�.
        if (hasError)
        {
            EditorGUILayout.HelpBox("���: Transitions ��Ͽ� ����ִ� 'Decision' �Ǵ� 'Next Behavior' ������ �ֽ��ϴ�. �ݵ�� ��� ������ ä���ּ���.", MessageType.Warning);
        }
    }
}