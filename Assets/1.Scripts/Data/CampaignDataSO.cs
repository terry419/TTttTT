// --- 파일명: CampaignDataSO.cs ---
// 경로: Assets/1.Scripts/Data/CampaignDataSO.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CampaignData_", menuName = "GameData/CampaignData")]
public class CampaignDataSO : ScriptableObject
{
    [Tooltip("이 캠페인을 구성하는 라운드들의 목록입니다. 순서대로 진행됩니다.")]
    public List<RoundDataSO> rounds;
}