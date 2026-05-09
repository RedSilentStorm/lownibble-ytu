public interface ITurnTaker
{
    float GetSpeed();
    void Initialize(CharacterData data, BattleEntity enemyTarget);
    void StartTurn();
}