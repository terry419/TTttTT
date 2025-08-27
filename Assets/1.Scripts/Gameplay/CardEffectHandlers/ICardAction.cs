using Cysharp.Threading.Tasks;

public interface ICardAction
{
    UniTask Execute(CardActionContext context);
}