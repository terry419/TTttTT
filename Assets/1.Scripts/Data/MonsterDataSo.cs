// 경로: ./TTttTT/Assets/1/Scripts/Data/MonsterDataSo.cs
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using System;

[Serializable]
public class GlobalModifierRule
{
    [Tooltip("이 패시브가 발동될 조건(Decision) 에셋입니다.")]
    public Decision condition;
    [Tooltip("조건이 충족되었을 때 적용할 몬스터 전용 상태 효과(MSE_SO) 에셋입니다.")]
    public MonsterStatusEffectSO effectToApply;
    [Tooltip("체크 시, 조건이 다시 거짓이 되면 적용했던 효과를 자동으로 제거합니다.")]
    public bool removeWhenConditionIsFalse = true;
}

[CreateAssetMenu(fileName = "MonsterData_", menuName = "GameData/MonsterData")]
public class MonsterDataSO : ScriptableObject
{
    [Header("--- [1] 기본 정보 ---")]
    public string monsterID;
    public string monsterName;
    public AssetReferenceGameObject prefabRef;

    [Header("--- [2] 기본 능력치 ---")]
    public float maxHealth = 100f;
    public float moveSpeed = 3f;
    public float contactDamage = 10f;

    [Header("--- [3] 신규 모듈형 AI 설정 ---")]
    [Tooltip("체크 시, 이 몬스터는 신규 모듈형 AI 시스템을 사용합니다.")]
    public bool useNewAI;
    [Tooltip("이 몬스터가 처음 시작할 행동 부품(Behavior) 에셋을 여기에 연결합니다.")]
    public MonsterBehavior initialBehavior;

    [Header("--- [4] 글로벌 패시브 효과 (Global Passives) ---")]
    [Tooltip("몬스터의 행동과 관계없이, 항상 조건이 맞으면 발동하는 패시브 스킬 목록입니다.")]
    public List<GlobalModifierRule> globalModifierRules;

    [Header("--- [5] 특수 능력 (공통) ---")]
    [Tooltip("체크 시, 사망 시 폭발합니다.")]
    public bool canExplodeOnDeath;
    public AssetReferenceGameObject explosionVfxRef;
    public float explosionDelay = 0.5f;
    public float explosionDamage = 20f;
    public float explosionRadius = 3f;

    [Tooltip("사망 시, 그 자리에 생성할 장판 효과(CreateZoneEffectSO) 에셋입니다.")]
    public CreateZoneEffectSO onDeathZoneEffect;
}