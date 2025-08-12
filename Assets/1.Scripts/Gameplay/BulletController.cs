using UnityEngine;

/// <summary>
/// 총알의 행동(이동, 소멸)과 데이터(데미지)를 관리합니다.
/// </summary>
public class BulletController : MonoBehaviour
{
    public float speed = 20f; // 총알의 속도
    public float damage;      // 총알의 데미지 (PlayerController가 설정해 줌)
    public float lifetime = 3f; // 총알의 최대 생존 시간

    public GameObject bulletPrefab; // 이 총알의 원본 프리팹 (PoolManager.Release에 필요)

    void OnEnable()
    {
        // PoolManager를 통해 재사용될 때를 대비하여, 활성화될 때마다 소멸 타이머를 재시작합니다.
        // Invoke는 간단하지만, 성능이 중요한 경우 코루틴이나 Update에서 직접 시간을 빼는 것이 더 좋습니다.
        Invoke(nameof(Deactivate), lifetime);
    }

    void Update()
    {
        // 매 프레임 앞으로 이동합니다.
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    private void Deactivate()
    {
        PoolManager.Instance.Release(bulletPrefab, gameObject);
    }

    // 몬스터와 충돌했을 때 MonsterController가 이 메서드를 호출하지 않으므로,
    // 이 스크립트는 데미지 값을 가지고 있는 역할만 수행합니다.
    // 충돌 처리는 MonsterController의 OnTriggerEnter2D에서 담당합니다.
}
