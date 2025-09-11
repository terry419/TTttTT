using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "RoundData_", menuName = "GameData/RoundData")]
public class RoundDataSO : ScriptableObject
{
    public float roundDuration = 180f;
    public int killGoal = 100;
    public List<Wave> waves;

    [Header("Boss Stage (Optional)")]
    [Tooltip("If this round is a boss stage, link the corresponding BossStageDataSO here.")]
    public BossStageDataSO bossStageData;
}
