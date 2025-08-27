using Cysharp.Threading.Tasks;

// NewCardDataSO�� ��� ����� �����ϴ� ���� Action Ŭ����
public class ModuleAction : ICardAction
{
    public async UniTask Execute(CardActionContext context)
    {
        var card = context.SourceCard;
        var resourceManager = ServiceLocator.Get<ResourceManager>();

        foreach (var moduleEntry in card.modules)
        {
            if (!moduleEntry.moduleReference.RuntimeKeyIsValid()) continue;

            // ��� SO ������ ResourceManager�� ���� �����ϰ� �ε�
            CardEffectSO module = await resourceManager.LoadAsync<CardEffectSO>(moduleEntry.moduleReference.AssetGUID);

            if (module != null)
            {
                // [�ٽ�] �ű� CardActionContext ������ ������ EffectContext�� ��ȯ
                var effectContextForModule = new EffectContext
                {
                    Caster = context.Caster,
                    SpawnPoint = context.SpawnPoint,
                    Platform = context.SourceCard
                };

                // ����� Execute �޼ҵ忡 �ùٸ� Ÿ���� ���ؽ�Ʈ�� �����Ͽ� ����
                module.Execute(effectContextForModule);
            }
        }
    }
}