
using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackHitbox
{
    [Serializable]
    public struct HitWindow
    {
        public float start, end; // 0-1 normalized
    }

    public event Action<IEntity> OnHit;
    public HitWindow window;

    private readonly HashSet<IEntity> _alreadyHit = new();

    private List<HitBoxSample> _hitboxSamples;
    private Transform _visualRoot;
    private bool _active;

    public void Setup(List<HitBoxSample> hitboxSamples, Transform visualRoot, HitWindow window)
    {
        _hitboxSamples = hitboxSamples;
        _visualRoot = visualRoot;
        this.window = window;
    }

    public void Enable()
    {
        _alreadyHit.Clear();
        _active = true;
    }

    public void Disable()
    {
        _active = false;
    }

    public void UpdateHitbox(float normalizedTime)
    {

        if (!_active || _hitboxSamples == null || _hitboxSamples.Count == 0) return;
        if (normalizedTime < window.start || normalizedTime > window.end) return;

        HitBoxSample s = GetPoseAtTime(normalizedTime);
        Debug.Log($"[Hitbox] norm={normalizedTime:F3} pos={s.position} rot={s.rotation.eulerAngles} size={s.size}");

        Vector3 pos = _visualRoot.TransformPoint(s.position);
        Quaternion rot = _visualRoot.rotation * s.rotation;

        DrawBox(pos, s.size * 0.5f, rot, Color.red);
        foreach (var hit in Physics.OverlapBox(
            center: pos,
            halfExtents: s.size * 0.5f,
            orientation: rot,
            Physics.AllLayers,
            QueryTriggerInteraction.Collide))
        {
            if (hit.TryGetComponent<IEntity>(out var target) && _alreadyHit.Add(target))
                OnHit?.Invoke(target);
        }
    }
    private static void DrawBox(Vector3 center, Vector3 halfExtents, Quaternion rot, Color color)
    {
        Vector3[] c = new Vector3[8];
        for (int i = 0; i < 8; i++)
            c[i] = center + rot * new Vector3(
                (i & 1) == 0 ? -halfExtents.x : halfExtents.x,
                (i & 2) == 0 ? -halfExtents.y : halfExtents.y,
                (i & 4) == 0 ? -halfExtents.z : halfExtents.z);
        int[] edges = { 0, 1, 1, 3, 3, 2, 2, 0, 4, 5, 5, 7, 7, 6, 6, 4, 0, 4, 1, 5, 2, 6, 3, 7 };
        for (int i = 0; i < edges.Length; i += 2)
            Debug.DrawLine(c[edges[i]], c[edges[i + 1]], color, 0.1f);
    }

    private HitBoxSample GetPoseAtTime(float time)
    {
        int last = _hitboxSamples.Count - 1;

        if (time <= _hitboxSamples[0].normalizedTime) return _hitboxSamples[0];
        if (time >= _hitboxSamples[last].normalizedTime) return _hitboxSamples[last];

        for (int i = 0; i < last; i++)
        {
            var a = _hitboxSamples[i];
            var b = _hitboxSamples[i + 1];

            if (time > b.normalizedTime) continue;

            float t = Mathf.InverseLerp(a.normalizedTime, b.normalizedTime, time);

            return new HitBoxSample
            {
                normalizedTime = time,
                position = Vector3.Lerp(a.position, b.position, t),
                rotation = Quaternion.Slerp(a.rotation, b.rotation, t),
                size = Vector3.Lerp(a.size, b.size, t)
            };
        }

        return _hitboxSamples[last];
    }


}
