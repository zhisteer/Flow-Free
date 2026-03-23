using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PathManager))]
public class PathLineRenderer : MonoBehaviour
{
    public GridSpawner grid;
    public DotSpawner dotSpawner;

    public Material lineMaterial;
    public float lineWidth = 0.15f;
    public int sortingOrder = 10;
    public float z = -0.5f;
    public Color[] pairColors;

    public float stepDuration = 0.10f;

    private PathManager _pm;

    private LineRenderer _active;
    private readonly Dictionary<int, LineRenderer> _committed = new();

    private int _lastActiveCount = 0;
    private Vector2Int _lastEndCell;  
    private bool _hasLastEndCell = false;

    private Vector3 _segFrom;
    private Vector3 _segTo;
    private float _segT = 1f;

    private void Awake()
    {
        _pm = GetComponent<PathManager>();
        _active = CreateLine("ActiveLine");
    }

    private void Start()
    {
        if ((pairColors == null || pairColors.Length == 0) && dotSpawner != null)
            pairColors = dotSpawner.colors;
    }

    private void LateUpdate()
    {
        if (grid == null || lineMaterial == null) return;

        DrawActiveSmooth();
        DrawCommitted();
        CleanupCommitted();
    }

    private void DrawActiveSmooth()
    {
        if (!_pm.IsDrawing || _pm.ActivePath.Count == 0)
        {
            _active.positionCount = 0;
            _lastActiveCount = 0;
            _segT = 1f;
            _hasLastEndCell = false;
            return;
        }

        ApplyStyle(_active, GetColor(_pm.ActivePairId));

        int count = _pm.ActivePath.Count;
        Vector2Int endCellNow = _pm.ActivePath[^1];

        bool stepEvent =
            count != _lastActiveCount ||
            (!_hasLastEndCell || endCellNow != _lastEndCell);

        if (stepEvent)
        {
            if (_hasLastEndCell)
            {
                _segFrom = grid.CellToWorld(_lastEndCell.x, _lastEndCell.y); _segFrom.z = z;
                _segTo = grid.CellToWorld(endCellNow.x, endCellNow.y); _segTo.z = z;
                _segT = 0f;
            }
            else
            {
                _segT = 1f;
            }

            _lastActiveCount = count;
            _lastEndCell = endCellNow;
            _hasLastEndCell = true;
        }

        if (_segT < 1f)
            _segT = Mathf.Min(1f, _segT + Time.deltaTime / Mathf.Max(0.0001f, stepDuration));

        _active.positionCount = count;
        for (int i = 0; i < count; i++)
        {
            var c = _pm.ActivePath[i];
            Vector3 p = grid.CellToWorld(c.x, c.y);
            p.z = z;
            _active.SetPosition(i, p);
        }

        if (count >= 2 && _segT < 1f)
            _active.SetPosition(count - 1, Vector3.Lerp(_segFrom, _segTo, _segT));
    }

    private void DrawCommitted()
    {
        foreach (var kv in _pm.CommittedPaths)
        {
            int pairId = kv.Key;
            List<Vector2Int> cells = kv.Value;
            if (cells == null || cells.Count == 0) continue;

            if (!_committed.TryGetValue(pairId, out var lr) || lr == null)
            {
                lr = CreateLine($"CommittedLine_{pairId}");
                _committed[pairId] = lr;
            }

            ApplyStyle(lr, GetColor(pairId));
            SetPoints(lr, cells);
        }
    }

    private void CleanupCommitted()
    {
        var toRemove = new List<int>();
        foreach (var kv in _committed)
        {
            if (!_pm.CommittedPaths.ContainsKey(kv.Key))
            {
                if (kv.Value) Destroy(kv.Value.gameObject);
                toRemove.Add(kv.Key);
            }
        }
        for (int i = 0; i < toRemove.Count; i++)
            _committed.Remove(toRemove[i]);
    }

    private LineRenderer CreateLine(string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);

        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.material = lineMaterial;

        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.numCapVertices = 8;
        lr.numCornerVertices = 8;
        lr.sortingOrder = sortingOrder;

        return lr;
    }

    private void ApplyStyle(LineRenderer lr, Color c)
    {
        lr.material = lineMaterial;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.startColor = c;
        lr.endColor = c;
    }

    private Color GetColor(int pairId)
    {
        if (pairColors != null && pairColors.Length > 0)
            return pairColors[Mathf.Abs(pairId) % pairColors.Length];

        return Color.white;
    }

    private void SetPoints(LineRenderer lr, List<Vector2Int> cells)
    {
        lr.positionCount = cells.Count;
        for (int i = 0; i < cells.Count; i++)
        {
            var c = cells[i];
            Vector3 p = grid.CellToWorld(c.x, c.y);
            p.z = z;
            lr.SetPosition(i, p);
        }
    }
}