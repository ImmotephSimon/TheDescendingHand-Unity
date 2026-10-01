using System;
using System.Collections.Generic;
using UnityEngine;

public enum TracedShape
{
    Triangle,
    Circle,
    Square
}

public class ShapeTraceComponent : CardComponent
{
    [Header("Detection tuning")]
    [SerializeField] private float _cornerAngleThreshold = 35f;   // degrees; direction change needed to count as a corner
    [SerializeField] private int _smoothingWindow = 3;            // compare point[i-w] to point[i+w] instead of neighbours, to reduce jitter
    [SerializeField] private float _cornerMergeDistance = 0.75f;  // corners closer than this (world units) get collapsed into one
    [SerializeField] private float _closureTolerancePct = 0.35f;  // end point must be within this % of shape size from start point
    [SerializeField] private float _minTraceDistance = 0.05f;

    private readonly List<Vector3> _tracePoints = new();
    private bool _isTracing;
    private const int ResampleCount = 72;
    private TracedShape _expectedShape;
    private float _minSize;
    private Vector3 _lastTracePoint;
    public event Action OnTraceStarted;
    public event Action<ShapeTraceResult> OnTraceEvaluated;

    public void Configure(TracedShape expectedShape, float minSize)
    {
        _expectedShape = expectedShape;
        _minSize = minSize;
    }

    protected override void OnActivate()
    {
        base.OnActivate();

        Debug.Log($"[{nameof(ShapeTraceComponent)}] Started tracing. Expected: {_expectedShape}");

        _tracePoints.Clear();
        _isTracing = true;
        OnTraceStarted?.Invoke();
    }

    public void EndTrace()
    {
        _isTracing = false;

        if (_tracePoints.Count < 4)
        {
            OnTraceEvaluated?.Invoke(default); // Matched = false, Outline = null -> check before use
            return;
        }

        bool matched = Matches(_tracePoints, _expectedShape, _minSize);
        Vector2[] outline = Resample(ToXZ(_tracePoints), ResampleCount).ToArray(); // using System.Linq
        Vector3 center = BoundingCenter(_tracePoints);

        OnTraceEvaluated?.Invoke(new ShapeTraceResult(matched, _expectedShape, center, outline, center.y));
    }

    private static Vector3 BoundingCenter(IReadOnlyList<Vector3> points)
    {
        Vector3 min = points[0];
        Vector3 max = points[0];
        float ySum = 0f;

        for (int i = 0; i < points.Count; i++)
        {
            Vector3 p = points[i];
            min.x = Mathf.Min(min.x, p.x);
            min.z = Mathf.Min(min.z, p.z);
            max.x = Mathf.Max(max.x, p.x);
            max.z = Mathf.Max(max.z, p.z);
            ySum += p.y;
        }

        return new Vector3(
            (min.x + max.x) * 0.5f,
            ySum / points.Count,          // average height
            (min.z + max.z) * 0.5f);
    }

    private void Update()
    {
        if (!_isTracing) return;

        Vector3 pos = Owner.Transform.position;

        if (Vector3.Distance(pos, _lastTracePoint) < _minTraceDistance)
            return;

        if (_tracePoints.Count > 0)
            Debug.DrawLine(
                _lastTracePoint + Vector3.up * 0.1f,
                pos + Vector3.up * 0.1f,
                Color.red,
                10f,
                depthTest: false);

        _tracePoints.Add(pos);
        _lastTracePoint = pos;
    }


    private bool Matches(List<Vector3> points, TracedShape expected, float minSize)
    {
        if (points.Count < 4) return false; // not enough data to be any real shape

        List<Vector2> path = ToXZ(points);

        float size = BoundingSize(path);
        if (size < minSize)
            return false;

        if (!IsClosed(path, size))
            return false;

        int cornerCount = CountCorners(path);

        return expected switch
        {
            TracedShape.Circle => cornerCount <= 1,
            TracedShape.Triangle => cornerCount == 3,
            TracedShape.Square => cornerCount == 4,
            _ => false
        };
    }

    private static List<Vector2> ToXZ(List<Vector3> points)
    {
        List<Vector2> path = new(points.Count);
        foreach (var p in points)
            path.Add(new Vector2(p.x, p.z));
        return path;
    }

    private static float BoundingSize(List<Vector2> path)
    {
        Vector2 min = path[0], max = path[0];
        foreach (var p in path)
        {
            min = Vector2.Min(min, p);
            max = Vector2.Max(max, p);
        }
        return (max - min).magnitude; // bounding-box diagonal
    }

    private bool IsClosed(List<Vector2> path, float size)
    {
        float closeDist = Vector2.Distance(path[0], path[^1]);
        return closeDist <= size * _closureTolerancePct;
    }

    

    private int CountCorners(List<Vector2> path)
    {
        List<Vector2> p = Resample(path, ResampleCount);
        int n = p.Count;
        int k = Mathf.Max(2, n / 12); // arc window on each side of a point

        float[] turn = new float[n];
        for (int i = 0; i < n; i++)
        {
            Vector2 prev = p[(i - k + n) % n];
            Vector2 next = p[(i + k) % n];
            turn[i] = Vector2.Angle(p[i] - prev, next - p[i]);
        }

        int corners = 0;
        for (int i = 0; i < n; i++)
        {
            if (turn[i] < _cornerAngleThreshold) continue;

            bool isPeak = true;
            for (int j = 1; j <= k && isPeak; j++)
            {
                if (turn[(i + j) % n] > turn[i] || turn[(i - j + n) % n] >= turn[i])
                    isPeak = false;
            }
            if (isPeak) corners++;
        }

        Debug.Log($"[{nameof(ShapeTraceComponent)}] Corners detected: {corners}");
        return corners;
    }

    // Evenly spaced points along the closed path (last point connects back to first)
    private static List<Vector2> Resample(List<Vector2> path, int count)
    {
        var closed = new List<Vector2>(path) { path[0] };

        float total = 0f;
        for (int i = 1; i < closed.Count; i++)
            total += Vector2.Distance(closed[i - 1], closed[i]);

        float step = total / count;
        var result = new List<Vector2>(count) { closed[0] };
        float carried = 0f;

        for (int i = 1; i < closed.Count && result.Count < count; i++)
        {
            Vector2 a = closed[i - 1], b = closed[i];
            float seg = Vector2.Distance(a, b);
            if (seg < 1e-6f) continue;

            float d = step - carried;
            while (d <= seg && result.Count < count)
            {
                result.Add(Vector2.Lerp(a, b, d / seg));
                d += step;
            }
            carried = seg - (d - step);
        }
        return result;
    }
}

public readonly struct ShapeTraceResult
{
    public readonly bool Matched;
    public readonly TracedShape Shape;
    public readonly Vector3 Center;
    public readonly Vector2[] Outline;   // closed polygon in XZ (last connects to first)
    public readonly Vector2 BoundsMin;
    public readonly Vector2 BoundsMax;
    public readonly float Y;             // height of the plane the shape was drawn on
    public Vector3 HalfExtents => new((BoundsMax.x - BoundsMin.x) * 0.5f, 5f, (BoundsMax.y - BoundsMin.y) * 0.5f);
    public ShapeTraceResult(bool matched, TracedShape shape, Vector3 center,
                            Vector2[] outline, float y)
    {
        Matched = matched;
        Shape = shape;
        Center = center;
        Outline = outline;
        Y = y;

        Vector2 min = outline[0], max = outline[0];
        foreach (var p in outline)
        {
            min = Vector2.Min(min, p);
            max = Vector2.Max(max, p);
        }
        BoundsMin = min;
        BoundsMax = max;
    }

    public bool Contains(Vector3 worldPos) => Contains(new Vector2(worldPos.x, worldPos.z));

    public bool Contains(Vector2 p)
    {
        // cheap reject
        if (p.x < BoundsMin.x || p.x > BoundsMax.x || p.y < BoundsMin.y || p.y > BoundsMax.y)
            return false;

        // ray casting (even-odd rule)
        bool inside = false;
        for (int i = 0, j = Outline.Length - 1; i < Outline.Length; j = i++)
        {
            Vector2 a = Outline[i], b = Outline[j];
            if ((a.y > p.y) != (b.y > p.y) &&
                p.x < (b.x - a.x) * (p.y - a.y) / (b.y - a.y) + a.x)
                inside = !inside;
        }
        return inside;
    }
}