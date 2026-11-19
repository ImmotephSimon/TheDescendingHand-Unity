using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public static class BakeHitWindows
{
    [MenuItem("Tools/Enemies/Bake Hit Windows")]
    private static void Bake()
    {
        foreach (var guid in AssetDatabase.FindAssets("t:EnemyDefinition"))
        {
            var enemy = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(
                AssetDatabase.GUIDToAssetPath(guid));

            if (enemy.WorldModel == null) continue;

            var animator = enemy.WorldModel.GetComponentInChildren<Animator>(true);
            if (animator == null || animator.runtimeAnimatorController is not AnimatorController controller)
            {
                Debug.LogWarning($"{enemy.name}: no AnimatorController on world model", enemy);
                continue;
            }

            foreach (var attack in enemy.Attacks)
            {
                if (attack == null) continue;
                BakeAttack(enemy, attack, controller);
            }
        }
        AssetDatabase.SaveAssets();
    }

    private static void BakeAttack(EnemyDefinition enemy, EnemyAttackDefinition attack, AnimatorController controller)
    {
        string stateName;
        try { stateName = attack.animation.StateName(); }
        catch { return; }

        var state = controller.layers[0].stateMachine.states
            .Select(s => s.state).FirstOrDefault(s => s.name == stateName);

        if (state == null || state.motion is not AnimationClip clip)
        {
            Debug.LogWarning($"{enemy.name}/{attack.name}: no clip on state '{stateName}'", attack);
            return;
        }

        float start = -1f, end = -1f;
        foreach (var e in AnimationUtility.GetAnimationEvents(clip))
        {
            if (e.functionName == "HitboxEnable") start = e.time;
            if (e.functionName == "HitboxDisable") end = e.time;
        }

        if (start < 0f && end < 0f) return; // no hitbox attack, e.g. card

        if (start < 0f || end < 0f)
        {
            Debug.LogWarning($"{enemy.name}/{attack.name}: '{clip.name}' has only one hitbox event", attack);
            return;
        }

        attack.swingDuration = clip.length / Mathf.Max(state.speed, 0.0001f);
        attack.hitWindow.start = start / clip.length;
        attack.hitWindow.end = end / clip.length;
        EditorUtility.SetDirty(attack);
    }
}