using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance { get; private set; }

    [System.Serializable]
    public class PreloadItem
    {
        public GameObject prefab;
        public int count;
    }

    [Header("사전 로드 설정 (수동)")]
    [SerializeField] private List<PreloadItem> monsterPreloads;
    [SerializeField] private List<PreloadItem> effectPreloads;

    [Header("참조")]
    private MonsterSpawner monsterSpawner;
    private HUDController hudController;

    private RoundDataSO currentRoundData;
    private float remainingTime;
    private int killCount;
    private bool isRoundActive = false;

    void Awake()
    {
        // --- [가설 검증] ---
        // 이미 Instance가 존재하는데, 이 스크립트가 붙은 객체와 다른 객체일 경우
        if (Instance != null && Instance != this)
        {
            Debug.LogError($"[가설 검증] 중복 RoundManager 발견! 기존 인스턴스(ID: {Instance.GetInstanceID()})가 이미 존재합니다. 새로 생성된 이 객체(ID: {GetInstanceID()})를 파괴합니다.");
            Destroy(gameObject); // 새로 생긴 중복 객체를 파괴
            return;
        }
        // --- [가설 검증 끝] ---

        // Instance가 null일 경우에만 새로 할당
        if (Instance == null)
        {
            Instance = this;
            // Gameplay 씬이 로드될 때마다 새로 생성되고 파괴되어야 하므로 DontDestroyOnLoad는 사용하지 않습니다.
            // 만약 부모 오브젝트에 DontDestroyOnLoad가 있다면 그것이 문제의 원인일 수 있습니다.
            // DontDestroyOnLoad(gameObject); 
        }

        Debug.LogWarning($"<color=orange>[가설 검증] Awake() 호출됨 | GameObject: {gameObject.name} (Instance ID: {GetInstanceID()}) | 'Instance' 참조가 이 객체로 설정되거나 유지됩니다.</color>");
    }

    void Start()
    {
        DoPreload();
    }

    private void DoPreload()
    {
        if (PoolManager.Instance == null)
        {
            Debug.LogError("[RoundManager] PoolManager를 찾을 수 없어 Preload를 진행할 수 없습니다.");
            return;
        }

        foreach (var item in monsterPreloads)
        {
            if (item.prefab != null && item.count > 0)
            {
                PoolManager.Instance.Preload(item.prefab, item.count);
            }
        }

        foreach (var item in effectPreloads)
        {
            if (item.prefab != null && item.count > 0)
            {
                PoolManager.Instance.Preload(item.prefab, item.count);
            }
        }
    }

    public IEnumerator StartRound(RoundDataSO roundDataToStart)
    {
        // --- [RoundManager-Debug] Step 1: StartRound 함수가 성공적으로 호출되었습니다. --- (기존 로그 보존)
        Debug.Log("--- [RoundManager-Debug] Step 1: StartRound 함수가 성공적으로 호출되었습니다. ---");

        // ★★★ 핵심 수정: HUDController와 MonsterSpawner를 찾을 때까지 안전하게 대기 ★★★
        while (hudController == null || monsterSpawner == null)
        {
            monsterSpawner = FindObjectOfType<MonsterSpawner>();
            hudController = FindObjectOfType<HUDController>();
            if (hudController == null || monsterSpawner == null)
            {
                yield return null;
            }
        }

        // --- [RoundManager-Debug] 참조 검색 시도... --- (기존 로그 보존)
        Debug.Log($"[RoundManager-Debug] 참조 검색 시도: " +
                  $"MonsterSpawner 찾음? {(monsterSpawner == null ? "아니오" : "예")}, " +
                  $"HUDController 찾음? {(hudController == null ? "아니오" : "예")}");

        // --- ### 디버그 ### ... --- (기존 로그 보존)
        Debug.Log($"### 디버그 ### RoundManager가 받은 데이터: {(roundDataToStart == null ? "null" : roundDataToStart.name)}");

        currentRoundData = roundDataToStart;
        // --- [RoundManager-Debug] Step 2... --- (기존 로그 보존)
        Debug.Log($"[RoundManager-Debug] Step 2: currentRoundData가 '{(currentRoundData == null ? "null" : currentRoundData.name)}'(으)로 설정되었습니다.");

        if (isRoundActive || currentRoundData == null)
        {
            // --- [RoundManager-Debug] Step 3... --- (기존 로그 보존)
            Debug.LogWarning($"[RoundManager-Debug] Step 3 (분기): 조건문(isRoundActive || currentRoundData == null)을 확인합니다. isRoundActive = {isRoundActive}, currentRoundData == null = {currentRoundData == null}");

            if (currentRoundData == null)
            {
                Debug.LogWarning("[RoundManager-Debug] Step 3.1 (실패): 시작할 라운드 데이터가 없습니다.");
                GameManager.Instance.ChangeState(GameManager.GameState.MainMenu);
            }
            else
            {
                Debug.LogWarning("[RoundManager-Debug] Step 3.2 (실패): 이미 라운드가 활성화되어 있어, 새로운 라운드를 시작하지 않습니다.");
            }
            yield break;
        }

        // --- [RoundManager-Debug] Step 4... --- (기존 로그 보존)
        Debug.Log($"[RoundManager-Debug] Step 4: 새로운 라운드 '{currentRoundData.name}'의 초기화를 시작합니다.");
        remainingTime = currentRoundData.roundDuration;
        killCount = 0;
        isRoundActive = true;
        // --- [RoundManager-Debug] Step 5... --- (기존 로그 보존)
        Debug.Log($"[RoundManager-Debug] Step 5: 라운드 상태 변수 초기화 완료. remainingTime={remainingTime}, killCount={killCount}, isRoundActive={isRoundActive}");

        if (monsterSpawner != null)
        {
            // --- [RoundManager-Debug] Step 6... --- (기존 로그 보존)
            Debug.Log("[RoundManager-Debug] Step 6: monsterSpawner가 존재합니다. monsterSpawner.StartSpawning을 호출합니다.");
            // ★★★ 핵심 수정: currentRoundData.waves를 인자로 전달 (CS1503 오류 해결) ★★★
            monsterSpawner.StartSpawning(currentRoundData.waves);
        }
        else
        {
            Debug.LogError("[RoundManager-Debug] Step 6 (실패): monsterSpawner 참조가 null입니다! FindObjectOfType으로도 찾을 수 없습니다.");
        }

        if (hudController != null)
        {
            // --- [RoundManager-Debug] Step 7... --- (기존 로그 보존)
            Debug.Log("[RoundManager-Debug] Step 7: hudController가 존재합니다. HUD를 업데이트하고 활성화합니다.");
            hudController.UpdateKillCount(killCount, currentRoundData.killGoal);
            hudController.UpdateTimer(remainingTime);
            hudController.gameObject.SetActive(true);
        }
        else
        {
            // --- [RoundManager-Debug] Step 7 (경고)... --- (기존 로그 보존)
            Debug.LogWarning("[RoundManager-Debug] Step 7 (경고): hudController 참조가 null입니다! FindObjectOfType으로도 찾을 수 없습니다.");
        }

        // --- [RoundManager-Debug] Step 8... --- (기존 로그 보존)
        Debug.Log("--- [RoundManager-Debug] Step 8: StartRound 함수가 모든 작업을 마치고 정상적으로 종료됩니다. ---");
    }

    public void RegisterKill()
    {
        Debug.Log($"<color=yellow>[가설 검증] RegisterKill 호출됨 | 이 함수의 주인: {gameObject.name} (Instance ID: {GetInstanceID()}) | isRoundActive: {isRoundActive}</color>");
        // --- ⬆️ 가설 검증용 디버그 2 ⬆️ ---

        if (!isRoundActive) return;
        killCount++;
        if (hudController != null)
        {
            hudController.UpdateKillCount(killCount, currentRoundData.killGoal);
        }

        if (killCount >= currentRoundData.killGoal)
        {
            EndRound(true);
        }
    }

    void Update()
    {
        if (!isRoundActive) return;
        remainingTime -= Time.deltaTime;
        if (hudController != null)
        {
            hudController.UpdateTimer(remainingTime);
        }

        if (remainingTime <= 0)
        {
            EndRound(false);
        }
    }

    public void EndRound(bool wasKillGoalReached)
    {
        if (!isRoundActive) return;
        isRoundActive = false;
        Debug.Log($"라운드 종료. 킬 수 달성: {wasKillGoalReached}");

        if (monsterSpawner != null)
        {
            // ★★★ 핵심 수정: StopSpawning() 함수 호출 확인 (CS1061 오류 해결) ★★★
            monsterSpawner.StopSpawning();
        }

        CleanupAllMonsters();

        if (wasKillGoalReached)
        {
            GenerateCardReward();
            GameManager.Instance.ChangeState(GameManager.GameState.Reward);
        }
        else
        {
            GameManager.Instance.ChangeState(GameManager.GameState.MainMenu);
        }
    }

    private void GenerateCardReward()
    {
        int numberOfChoices = 3;
        List<CardDataSO> allCards = DataManager.Instance.GetAllCards();
        if (allCards == null || allCards.Count < numberOfChoices)
        {
            Debug.LogError($"카드 데이터가 {numberOfChoices}개 미만이어서 보상을 생성할 수 없습니다.");
            return;
        }
        List<CardDataSO> rewardChoices = new List<CardDataSO>();
        List<CardDataSO> selectableCards = new List<CardDataSO>(allCards);
        for (int i = 0; i < numberOfChoices; i++)
        {
            if (selectableCards.Count == 0) break;
            float totalWeight = selectableCards.Sum(card => card.rewardAppearanceWeight);
            float randomPoint = Random.Range(0, totalWeight);
            float currentWeight = 0f;
            CardDataSO selectedCard = null;
            foreach (var card in selectableCards)
            {
                currentWeight += card.rewardAppearanceWeight;
                if (randomPoint <= currentWeight)
                {
                    selectedCard = card;
                    break;
                }
            }
            if (selectedCard == null && selectableCards.Count > 0)
            {
                selectedCard = selectableCards.Last();
            }
            if (selectedCard != null)
            {
                rewardChoices.Add(selectedCard);
                selectableCards.Remove(selectedCard);
            }
        }
        RewardManager.Instance.EnqueueReward(rewardChoices);
    }

    public void CleanupAllMonsters()
    {
        MonsterController[] activeMonsters = FindObjectsOfType<MonsterController>();
        foreach (var monster in activeMonsters)
        {
            if (monster != null && monster.gameObject.activeInHierarchy)
            {
                PoolManager.Instance.Release(monster.gameObject);
            }
        }
        Debug.Log($"모든 몬스터 ({activeMonsters.Length}마리)를 정리했습니다.");
    }
}