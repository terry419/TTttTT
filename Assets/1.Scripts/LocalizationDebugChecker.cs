using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

public class LocalizationDebugChecker : MonoBehaviour
{
    void Start()
    {
        // 이 스크립트가 붙어있는 게임 오브젝트에서 LocalizeStringEvent 컴포넌트를 찾습니다.
        var localizeEvent = GetComponent<LocalizeStringEvent>();
        if (localizeEvent != null)
        {
            // [디버그 4] LocalizeStringEvent가 어떤 테이블과 키를 참조하고 있는지 확인합니다.
            string tableName = localizeEvent.StringReference.TableReference;
            string entryName = localizeEvent.StringReference.TableEntryReference;
            Debug.Log($"[디버그 4] '{gameObject.name}' 오브젝트의 LocalizeStringEvent가 참조하는 키: [테이블: {tableName}], [키: {entryName}]");

            // [디버그 5] 이 컴포넌트의 문자열이 '변경'될 때마다 OnStringChanged 함수를 실행하도록 등록합니다.
            localizeEvent.OnUpdateString.AddListener(OnStringChanged);
            Debug.Log($"[디버그 5] '{gameObject.name}' 오브젝트에 문자열 변경 감시자를 성공적으로 등록했습니다.");
        }
        else
        {
            Debug.LogError($"[디버그 4-에러] '{gameObject.name}' 오브젝트에서 LocalizeStringEvent 컴포넌트를 찾을 수 없습니다.");
        }
    }

    // 문자열이 성공적으로 변경되어 UI에 적용될 때 이 함수가 호출됩니다.
    void OnStringChanged(string newValue)
    {
        // [디버그 6] 만약 이 로그가 보인다면, 모든 과정이 성공했고 최종적으로 UI가 업데이트 되었다는 뜻입니다.
        Debug.Log($"[디버그 6] 문자열이 성공적으로 변경되었습니다! 새로 적용된 값: '{newValue}'");
    }
}