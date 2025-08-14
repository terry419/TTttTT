[System.Serializable]
public class Wave
{
    // [수정] GameObject 대신 string으로 몬스터 종류를 지정
    public string monsterName;
    public int count;
    public float spawnInterval;
    public float delayAfterWave;
}