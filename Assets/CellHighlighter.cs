using System.Collections.Generic;
using UnityEngine;

public class CellHighlighter : MonoBehaviour
{
    public PathManager pathManager;
    public GridSpawner grid;
    public Color[] pairColors;
    public DotSpawner dotSpawner;

    [Range(0.15f, 0.5f)]
    public float softness = 0.3f;

    private readonly HashSet<Vector2Int> _highlighted = new();

    private void Start()
    {
        if ((pairColors == null || pairColors.Length == 0) && dotSpawner != null)
            pairColors = dotSpawner.colors;
    }

    private void LateUpdate()
    {
        if (pathManager == null || grid == null) return;
        if (grid.cellVisuals == null || grid.cellVisuals.Count == 0) return;


        HashSet<Vector2Int> shouldHighlight = new();

        foreach (var kv in pathManager.CommittedPaths)
        {
            int pairId = kv.Key;
            Color c = GetColor(pairId);

            foreach (var cell in kv.Value)
            {
                shouldHighlight.Add(cell);
                if (grid.cellVisuals.TryGetValue(cell, out var cv))
                    cv.SetHighlight(c, softness);
            }
        }

        if (pathManager.IsDrawing && pathManager.ActivePath.Count > 0)
        {
            Color c = GetColor(pathManager.ActivePairId);

            foreach (var cell in pathManager.ActivePath)
            {
                shouldHighlight.Add(cell);
                if (grid.cellVisuals.TryGetValue(cell, out var cv))
                    cv.SetHighlight(c, softness);
            }
        }

        foreach (var cell in _highlighted)
        {
            if (!shouldHighlight.Contains(cell))
            {
                if (grid.cellVisuals.TryGetValue(cell, out var cv))
                    cv.ClearHighlight();
            }
        }

        _highlighted.Clear();
        foreach (var cell in shouldHighlight)
            _highlighted.Add(cell);
    }

    private Color GetColor(int pairId)
    {
        if (pairColors != null && pairColors.Length > 0)
            return pairColors[Mathf.Abs(pairId) % pairColors.Length];
        return Color.white;
    }
}