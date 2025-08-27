using Cysharp.Threading.Tasks;

// NewCardDataSO의 모듈 목록을 실행하는 최종 Action 클래스
public class ModuleAction : ICardAction
{
    public async UniTask Execute(CardActionContext context)
    {
        var card = context.SourceCard;
        var resourceManager = ServiceLocator.Get<ResourceManager>();

        foreach (var moduleEntry in card.modules)
        {
            if (!moduleEntry.moduleReference.RuntimeKeyIsValid()) continue;

            // 모듈 SO 에셋을 ResourceManager를 통해 안전하게 로드
            CardEffectSO module = await resourceManager.LoadAsync<CardEffectSO>(moduleEntry.moduleReference.AssetGUID);

            if (module != null)
            {
                // [핵심] 신규 CardActionContext 정보를 구버전 EffectContext로 변환
                var effectContextForModule = new EffectContext
                {
                    Caster = context.Caster,
                    SpawnPoint = context.SpawnPoint,
                    Platform = context.SourceCard
                };

                // 모듈의 Execute 메소드에 올바른 타입의 컨텍스트를 전달하여 실행
                module.Execute(effectContextForModule);
            }
        }
    }
}