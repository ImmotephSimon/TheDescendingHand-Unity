public interface IAbilityManager
{
    bool Ready(IAttackAbility attack);
    void StartCooldown(IAttackAbility attack);
}