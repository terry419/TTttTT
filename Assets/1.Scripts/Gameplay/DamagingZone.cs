// --- 파일명: DamagingZone.cs ---
using UnityEngine;
using System.Collections.Generic;

public class DamagingZone : MonoBehaviour
{
    [Header("데미지 설정")]
    public float damagePerTick = 10f; // 1초마다 줄 데미지
    public float tickInterval = 1.0f; // 데미지를 주는 간격 (초)
    public float duration = 5.0f; // 장판이 지속되는 시간 (초)

    // 장판 안에 들어와 있는 몬스터 목록
    private List<MonsterController> targets = new List<MonsterController>();
    private float tickTimer;
    private float durationTimer;

    void OnEnable()
    {
        // 오브젝트가 활성화될 때마다 타이머 초기화
        tickTimer = 0f;
        durationTimer = duration;
        targets.Clear(); // 몬스터 목록 초기화
    }

    void Update()
    {
        // 장판 지속시간 관리
        durationTimer -= Time.deltaTime;
        if (durationTimer <= 0)
        {
            // PoolManager를 사용한다면 Release, 아니라면 Destroy
            if (PoolManager.Instance != null) PoolManager.Instance.Release(gameObject);
            else Destroy(gameObject);
            return;
        }

        // 데미지 틱 타이머 관리
        tickTimer += Time.deltaTime;
        if (tickTimer >= tickInterval)
        {
            tickTimer = 0f;
            ApplyDamageToTargets();
        }
    }

    // 장판 범위 안으로 몬스터가 들어왔을 때
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Monster")) // 몬스터 태그를 사용한다고 가정
        {
            MonsterController monster = other.GetComponent<MonsterController>();
            if (monster != null && !targets.Contains(monster))
            {
                targets.Add(monster);
            }
        }
    }

    // 장판 범위 밖으로 몬스터가 나갔을 때
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Monster"))
        {
            MonsterController monster = other.GetComponent<MonsterController>();
            if (monster != null && targets.Contains(monster))
            {
                targets.Remove(monster);
            }
        }
    }

    // 목록에 있는 모든 타겟에게 데미지를 주는 함수
    private void ApplyDamageToTargets()
    {
        // 리스트를 복사해서 순회 (데미지를 입고 죽어서 리스트에서 제거될 경우를 대비)
        List<MonsterController> currentTargets = new List<MonsterController>(targets);
        foreach (var monster in currentTargets)
        {
            if (monster != null && monster.gameObject.activeInHierarchy)
            {
                // 바로 이 부분! 몬스터의 TakeDamage를 호출하면 끝!
                monster.TakeDamage(damagePerTick);
            }
        }
    }
}