using UnityEngine;

/// <summary>
/// 총알의 행동(이동, 소멸)과 데이터(데미지)를 관리합니다.
/// </summary>
public class BulletController : MonoBehaviour
{
    private Vector2 _direction; // 총알의 이동 방향
    private float _speed;       // 총알의 속도
    public float damage;        // 총알의 데미지 (PlayerController가 설정해 줌)
    public float lifetime = 3f; // 총알의 최대 생존 시간

    /// <summary>
    /// 총알을 초기화하고 발사합니다.
    /// </summary>
    /// <param name="direction">총알의 이동 방향 (정규화된 벡터)</param>
    /// <param name="speed">총알의 속도</param>
    /// <param name="damage">총알이 줄 데미지</param>
    public void Initialize(Vector2 direction, float speed, float damage)
    {
        _direction = direction.normalized; // 방향 벡터 정규화
        _speed = speed;
        this.damage = damage; // 전달받은 데미지 설정

        // 총알의 초기 회전 설정 (선택 사항: 방향에 따라 총알 스프라이트 회전)
        // 예를 들어, Vector2.right가 기본 방향일 때
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 활성화될 때마다 소멸 타이머를 재시작합니다.
        // Invoke는 간단하지만, 성능이 중요한 경우 코루틴이나 Update에서 직접 시간을 빼는 것이 더 좋습니다.
        Invoke(nameof(Deactivate), lifetime);
    }

    void Update()
    {
        // 매 프레임 지정된 방향으로 이동합니다.
        transform.Translate(_direction * _speed * Time.deltaTime, Space.World);
    }

    private void Deactivate()
    {
        // PoolManager를 통해 오브젝트를 풀로 반환합니다.
        PoolManager.Instance.Release(gameObject);
    }

    // 몬스터와 충돌했을 때 MonsterController가 이 메서드를 호출하지 않으므로,
    // 이 스크립스는 데미지 값을 가지고 있는 역할만 수행합니다.
    // 충돌 처리는 MonsterController의 OnTriggerEnter2D에서 담당합니다.
}

