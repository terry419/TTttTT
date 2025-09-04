// --- 파일명: CampaignDataSO.cs ---
// 경로: Assets/1.Scripts/Data/CampaignDataSO.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CampaignData_", menuName = "GameData/CampaignData")]
public class CampaignDataSO : ScriptableObject
{
    [Tooltip("캠페인에 포함될 라운드 목록입니다. 순서대로 진행됩니다.")]
    public List<RoundDataSO> rounds;
}
