using System;

public interface IStatReader
{


    (float added, float additive, float multiplicative) GetScaling(GameTag modStatMovement);
    float GetStat(GameTag stat);
    void Listen(GameTag stat, Action<float> listener);
    void StopListening(GameTag stat, Action<float> notification);
}