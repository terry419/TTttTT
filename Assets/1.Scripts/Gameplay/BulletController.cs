using UnityEngine;
using System.Collections.Generic; // Added this line

/// <summary>
/// 총알의 행동(이동, 소멸)과 데이터(데미지)를 관리합니다.
/// </summary>
public class BulletController : MonoBehaviour
{
    private Vector2 _direction; // 총알의 이동 방향
    public float speed;       // 총알의 속도 (외부에서 접근 가능하도록 public으로 변경)
    public float damage;        // 총알의 데미지 (PlayerController가 설정해 줌)
    public string shotInstanceID; // [추가] 발사 인스턴스 고유 ID
    public float lifetime = 3f; // 총알의 최대 생존 시간
    public CardDataSO SourceCard { get; private set; } // [추가] 이 총알을 발사한 카드 데이터
    public ProjectileEffectSO SourceModule { get; private set; }

    public int _currentPierceCount;
    public HashSet<GameObject> _hitMonsters = new HashSet<GameObject>();


    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }


    /// <summary>
    /// 총알을 초기화하고 발사합니다.
    /// </summary>
    /// <param name="direction">총알의 이동 방향 (정규화된 벡터)</param>
    /// <param name="initialSpeed">총알의 초기 속도</param>
    /// <param name="damage">총알이 줄 데미지</param>
    /// <param name="shotID">[추가] 발사 ID 설정</param>

    public void Initialize(Vector2 direction, float initialSpeed, float damage, string shotID, CardDataSO cardData, int pierceCount)
    {
        _direction = direction.normalized; // 방향 벡터 정규화
        speed = initialSpeed; // 초기 속도 설정
        this.damage = damage; // 전달받은 데미지 설정
        this.shotInstanceID = shotID; // [추가] 발사 ID 설정
        this.SourceCard = cardData; // [추가] 카드 데이터 저장
        this._currentPierceCount = pierceCount; // 현재 관통 횟수 저장
        this._hitMonsters.Clear(); // 풀링을 위해 이전에 맞춘 몬스터 목록 초기화

        // 총알의 초기 회전 설정 (선택 사항: 방향에 따라 총알 스프라이트 회전)
        // 예를 들어, Vector2.right가 기본 방향일 때
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 활성화될 때마다 소멸 타이머를 재시작합니다.
        // Invoke는 간단하지만, 성능이 중요한 경우 코루틴이나 Update에서 직접 시간을 빼는 것이 더 좋습니다.
        Invoke(nameof(Deactivate), lifetime);
    }

    public void Initialize(Vector2 direction, float initialSpeed, float damage, string shotID, ProjectileEffectSO module)
    {
        _direction = direction.normalized;
        speed = initialSpeed;
        this.damage = damage;
        shotInstanceID = shotID;
        SourceCard = null; // 구버전 소스는 null로 초기화
        SourceModule = module;
        _currentPierceCount = module.pierceCount;
        _hitMonsters.Clear();

        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        CancelInvoke(nameof(Deactivate));
        Invoke(nameof(Deactivate), lifetime);
    }

    void Update()
    {
        // Rigidbody를 사용하므로 FixedUpdate에서 물리 이동 처리
    }

    private void FixedUpdate()
    {
        rb.velocity = _direction * speed;
    }
    private void Deactivate()
    {
        // TODO: 소멸 VFX (onExpireVFXRef) 재생 로직 추가
        ServiceLocator.Get<PoolManager>().Release(gameObject);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(Tags.Monster)) return;
        if (_hitMonsters.Contains(other.gameObject)) return; // 이미 맞춘 몬스터는 무시 (관통 후 재충돌 방지)

        if (other.TryGetComponent<MonsterController>(out var monster))
        {
            // [수정된 검사] 구버전 카드 또는 신버전 모듈 둘 중 하나라도 있으면 통과!
            if (SourceCard == null && SourceModule == null)
            {
                Debug.LogWarning("SourceCard와 SourceModule이 모두 null인 총알이 충돌했습니다.");
                return;
            }

            // 피해량 적용
            monster.TakeDamage(this.damage);
            _hitMonsters.Add(other.gameObject);

            // TODO: 치명타, 흡혈, 상태이상 등 모든 추가 효과 로직을 이곳으로 이전해야 합니다.

            // 관통 로직
            if (_currentPierceCount > 0)
            {
                _currentPierceCount--;
            }
            else
            {
                Deactivate(); // 관통 횟수를 모두 소진하면 소멸
            }
        }
    }
}