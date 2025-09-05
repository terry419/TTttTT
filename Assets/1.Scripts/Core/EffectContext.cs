// 경로: ./TTttTT/Assets/1.Scripts/Core/EffectContext.cs

using UnityEngine;
using System.Collections.Generic;

public class EffectContext
{
    // --- 입력 데이터 (Executor가 채워줌) ---
    public CharacterStats Caster;           // 효과 시전자
    public Transform SpawnPoint;            // 효과 발현 위치 (총구 등)
    public List<Vector2> FiringDirections;  // 계산된 최종 발사 방향 목록
    public NewCardDataSO Platform;          // [추가] 이 효과를 발동시킨 원본 카드(플랫폼)

    public float BaseDamageOverride = 0f; // 덮어쓸 기본 데미지. 0이면 사용 안 함.
    public CardInstance SourceCardInstance;

    // --- 실행 중 변경되는 데이터 (모듈이 채우거나 참조) ---
    public MonsterController HitTarget;     // 현재 피격된 대상
    public Vector3 HitPosition;             // 피격이 발생한 위치
    public float DamageDealt;               // 입힌 최종 피해량
    public int CurrentBounceCount;          // 현재 리코셰 횟수
    public bool IsCritical;                 // 치명타 여부
    public bool IsKill;                     // 킬 여부

    /// <summary>
    /// EffectContextPool에 반환될 때 호출되어 모든 데이터를 초기화합니다.
    /// </summary>
    public void Reset()
    {
        Log.Info(Log.LogCategory.PoolManager, "[EffectContext] Resetting context for pooling.");
        Caster = null;
        SpawnPoint = null;
        HitTarget = null;
        Platform = null; // [추가] 플랫폼 정보 초기화

        BaseDamageOverride = 0f;
        SourceCardInstance = null;

        if (FiringDirections != null) FiringDirections.Clear();
        else FiringDirections = new List<Vector2>();

        HitPosition = Vector3.zero;
        DamageDealt = 0f;
        CurrentBounceCount = 0;
        IsCritical = false;
        IsKill = false;
    }
}