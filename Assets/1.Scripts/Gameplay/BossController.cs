// 파일 경로: Assets/1.Scripts/Gameplay/BossController.cs (전체 교체)
using UnityEngine;
using System.Text;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.Threading;

[RequireComponent(typeof(BossStats), typeof(Rigidbody2D))]
public class BossController : MonoBehaviour
{
    [Header("보스 공격 설정")]
    public Transform firePoint;

    [HideInInspector] public BossStats stats; // CharacterStats -> BossStats
    [HideInInspector] public Rigidbody2D rb;

    private List<CardInstance> _ownedCards = new List<CardInstance>();
    private CancellationTokenSource _attackLoopCts;

    private int maxEquipSlots;

    void Awake()
    {
        stats = GetComponent<BossStats>(); // CharacterStats -> BossStats
        rb = GetComponent<Rigidbody2D>();
    }

    void OnDestroy()
    {
        _attackLoopCts?.Cancel();
        _attackLoopCts?.Dispose();
    }

    public void Initialize(CharacterDataSO bossData)
    {
        if (bossData != null)
        {
            this.maxEquipSlots = bossData.maxEquipSlots;
            Debug.Log($"[BossController] '{bossData.name}' 보스 초기화. 장착 슬롯: {this.maxEquipSlots}");

            stats.stats = bossData.baseStats;
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && bossData.illustration != null)
            {
                spriteRenderer.sprite = bossData.illustration;
            }

            // CharacterDataSO로부터 시작 카드를 가져와 인스턴스를 생성합니다.
            foreach (var cardData in bossData.startingCards)
            {
                _ownedCards.Add(new CardInstance(cardData));
            }

            stats.CalculateFinalStats();
            stats.Heal(stats.FinalHealth); // 체력을 최대치로 설정합니다.


            var sb = new StringBuilder();
            sb.AppendLine($"--- 보스({gameObject.name}) 스탯 초기화 완료 ---");
            sb.AppendLine($"체력: {stats.GetCurrentHealth():F1} / {stats.FinalHealth:F1}");
            sb.AppendLine($"공격력 보너스: {stats.FinalDamageBonus:F2}%");
            sb.AppendLine($"공격 속도: {stats.FinalAttackSpeed:F2}");
            sb.AppendLine($"이동 속도: {stats.FinalMoveSpeed:F2}");
            sb.AppendLine($"치명타 확률: {stats.FinalCritRate:F2}%");
            sb.AppendLine($"치명타 피해: {stats.FinalCritDamage:F2}%");
            Debug.Log(sb.ToString());
        }
    }

    // PlayerController의 공격 로직과 동일 (변경 없음)
    public void StartAutoAttackLoop()
    {
        if (_attackLoopCts != null && !_attackLoopCts.IsCancellationRequested) return;
        _attackLoopCts = new CancellationTokenSource();
        AutoAttackLoop(_attackLoopCts.Token).Forget();
    }

    private async UniTaskVoid AutoAttackLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (_ownedCards.Count == 0)
            {
                // 공격할 카드가 없으면 1초 대기 후 다시 시도
                await UniTask.Delay(System.TimeSpan.FromSeconds(1), cancellationToken: token);
                continue;
            }

            CardInstance cardToUse = _ownedCards[Random.Range(0, _ownedCards.Count)];
            await PerformAttack(cardToUse);
            float interval = cardToUse.CardData.attackInterval / stats.FinalAttackSpeed;
            if (float.IsInfinity(interval) || interval <= 0) interval = 1f;
            await UniTask.Delay(System.TimeSpan.FromSeconds(interval), cancellationToken: token);
        }
    }

    private async UniTask PerformAttack(CardInstance cardInstance)
    {
        ICardAction action = cardInstance.CardData.CreateAction();
        var context = new CardActionContext(cardInstance, stats, firePoint);
        await action.Execute(context);
    }
}