
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class HitBoxSamplerWindow : EditorWindow
{
    private EnemyDefinition[] definitions;
    private Transform hitBox;
    private float fps = 10f;

    [MenuItem("Tools/Animation/Hitbox Sampler")]
    private static void Open() => GetWindow<HitBoxSamplerWindow>("Hitbox Sampler");

    private void OnEnable() => RefreshDefinitions();

    private void RefreshDefinitions()
    {
        definitions = AssetDatabase.FindAssets("t:EnemyDefinition")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(path => AssetDatabase.LoadAssetAtPath<EnemyDefinition>(path))
            .Where(x => x != null)
            .ToArray();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Hitbox Animation Sampler", EditorStyles.boldLabel);

        if (GUILayout.Button("Refresh Definitions"))
            RefreshDefinitions();

        hitBox = (Transform)EditorGUILayout.ObjectField(
            "Hitbox Transform", hitBox, typeof(Transform), true);

        fps = Mathf.Max(1f, EditorGUILayout.FloatField("Sample FPS", fps));

        EditorGUILayout.Space();

        if (definitions == null) return;

        foreach (var target in definitions)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(target.name, EditorStyles.boldLabel);

            var data = target.HitBoxSampleData;
            if (data == null) continue;

            foreach (var entry in data.entries)
            {
                EditorGUILayout.BeginHorizontal();

                entry.animation = (AttackAnimation)EditorGUILayout.EnumPopup(entry.animation);

                if (GUILayout.Button("Bake", GUILayout.Width(55)))
                    Bake(target, entry);

                EditorGUILayout.LabelField(
                    $"{entry.samples.Count} samples",
                    GUILayout.Width(90));

                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button($"Add Entry to {target.name}"))
            {
                Undo.RecordObject(target, "Add Hitbox Entry");
                data.entries.Add(new HitBoxSampleEntry());
                EditorUtility.SetDirty(target);
            }
        }
    }

    private void Bake(EnemyDefinition target, HitBoxSampleEntry entry)
    {
        var enemyVisuals = target.WorldModel;
        if (enemyVisuals == null || hitBox == null) return;

        var animator = enemyVisuals.GetComponentInChildren<Animator>(true);
        if (animator == null || animator.runtimeAnimatorController is not AnimatorController controller)
        {
            Debug.LogError("No AnimatorController found.", enemyVisuals);
            return;
        }

        var state = controller.layers
            .SelectMany(l => l.stateMachine.states).Select(s => s.state)
            .FirstOrDefault(s => s.name == entry.animation.StateName());
        if (state?.motion is not AnimationClip clip)
        {
            Debug.LogError($"No AnimationClip found in state {entry.animation.StateName()}", enemyVisuals);
            return;
        }

        var transforms = enemyVisuals.GetComponentsInChildren<Transform>(true);
        var states = transforms.Select(t => (t, t.gameObject.activeSelf, t.localPosition, t.localRotation, t.localScale)).ToArray();
        var wasAnimatorEnabled = animator.enabled;
        var box = hitBox.GetComponent<BoxCollider>();

        Undo.RecordObjects(new UnityEngine.Object[] { target, animator }, "Bake Hitbox Samples");
        entry.samples.Clear();
        try
        {
            int count = Mathf.CeilToInt(clip.length * fps);
            for (int i = 0; i <= count; i++)
            {
                float time = Mathf.Min(i / fps, clip.length);
                clip.SampleAnimation(animator.gameObject, time);
                entry.samples.Add(new HitBoxSample
                {
                    normalizedTime = clip.length > 0f ? time / clip.length : 0f,
                    position = enemyVisuals.transform.InverseTransformPoint(hitBox.position),
                    rotation = Quaternion.Inverse(enemyVisuals.transform.rotation) * hitBox.rotation,
                    size = box != null ? Vector3.Scale(box.size, hitBox.lossyScale) : Vector3.zero
                });
            }
        }
        finally
        {
            foreach (var (t, wasActive, pos, rot, scale) in states)
            {
                t.gameObject.SetActive(wasActive);
                t.SetLocalPositionAndRotation(pos, rot);
                t.localScale = scale;
            }
            animator.enabled = wasAnimatorEnabled;
        }

        EditorUtility.SetDirty(animator);
        EditorUtility.SetDirty(target);
        Debug.Log($"Baked {entry.samples.Count} samples from {clip.name}", target);
    }
}
