// --- ϸ: CampaignDataSO.cs ---
// : Assets/1.Scripts/Data/CampaignDataSO.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CampaignData_", menuName = "GameData/CampaignData")]
public class CampaignDataSO : ScriptableObject
{
    [Tooltip(" ķ ϴ  Դϴ.  ˴ϴ.")]
    public List<RoundDataSO> rounds;
}