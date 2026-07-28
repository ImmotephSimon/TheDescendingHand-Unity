using UnityEngine;

[CreateAssetMenu]
public class BalanceCurves : ScriptableObject
{
    public AnimationCurve ExpectedPlayerLife;

    private void OnEnable()
    {
        if (ExpectedPlayerLife.length == 0)
        {
            ExpectedPlayerLife = new AnimationCurve(
                new Keyframe(1, 20),
                new Keyframe(100, 218)
            );

            var keys = ExpectedPlayerLife.keys;

            float slope = (keys[1].value - keys[0].value) /
                          (keys[1].time - keys[0].time);

            keys[0].outTangent = slope;
            keys[1].inTangent = slope;

            ExpectedPlayerLife.keys = keys;
        }
    }
}