// CardNetwork.cs
public interface ICardServerHandler
{
    void Initialize(CardNetwork network);
    void HandleClientReady();
    void HandleUseAbility(int index);
    void HandleCancelAbility(int index);
    void HandleCancelCurrentCast();
    void HandleRequestPile(bool drawPile);
}