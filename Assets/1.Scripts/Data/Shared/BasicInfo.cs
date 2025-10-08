// 추천 경로: Assets/1.Scripts/Data/Shared/BasicInfo.cs
using System;
using UnityEngine;
using UnityEngine.Localization;

/// <summary>
/// 카드의 이름, 아이콘, 타입 등 UI 표시에 필요한 기본 정보를 담는 공용 클래스입니다.
/// </summary>
[Serializable]
public class BasicInfo
{
    [Tooltip("카드의 고유 ID (예: warrior_basic_001)")]
    public string cardID;
    [Tooltip("UI에 표시될 카드의 이름 (로컬라이징)")]
    public LocalizedString cardName;
    [Tooltip("카드 중앙에 표시될 메인 일러스트")]
    public Sprite cardIllustration;
    [Tooltip("카드의 타입 (물리 또는 마법)")]
    public CardType type;
    [Tooltip("카드의 희귀도")]
    public CardRarity rarity;
    [Tooltip("카드 효과 설명 텍스트")]
    public LocalizedString effectDescription;
}