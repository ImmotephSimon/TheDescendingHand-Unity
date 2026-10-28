public interface IAbilityManager
{
    bool Ready(IEnemyAttack attack);
    void StartCooldown(IEnemyAttack attack);
}