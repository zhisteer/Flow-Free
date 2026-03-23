using System.Collections.Generic;
using UnityEngine;

public class PathManager : MonoBehaviour
{
    private readonly Dictionary<int, List<Vector2Int>> pathsByPair = new();

    private readonly Dictionary<Vector2Int, int> ownerByCell = new();

    public int ActivePairId { get; private set; } = -1;
    public List<Vector2Int> ActivePath { get; } = new();

    public bool IsDrawing => ActivePairId >= 0;

    public bool TryGetOwner(Vector2Int cell, out int pairId) => ownerByCell.TryGetValue(cell, out pairId);

    public bool HasCommittedPath(int pairId) => pathsByPair.ContainsKey(pairId);

    public IReadOnlyDictionary<int, List<Vector2Int>> CommittedPaths => pathsByPair;

    public int TotalPairs { get; set; } 

    

    public void StartDrawing(int pairId, Vector2Int startCell)
    {
        ClearPair(pairId);

        ActivePairId = pairId;
        ActivePath.Clear();
        ActivePath.Add(startCell);
    }

    public void ClearPair(int pairId)
    {
        if (!pathsByPair.TryGetValue(pairId, out var oldPath) || oldPath == null)
            return;

        for (int i = 0; i < oldPath.Count; i++)
            ownerByCell.Remove(oldPath[i]);

        pathsByPair.Remove(pairId);
    }
    public bool TryStepTo(Vector2Int nextCell)
    {
        if (!IsDrawing) return false;
        if (ActivePath.Count == 0) return false;

        Vector2Int last = ActivePath[^1];

        int manhattan = Mathf.Abs(nextCell.x - last.x) + Mathf.Abs(nextCell.y - last.y);
        if (manhattan != 1) return false;

        if (ActivePath.Count >= 2 && nextCell == ActivePath[^2])
        {
            ActivePath.RemoveAt(ActivePath.Count - 1);
            return true;
        }

        if (ActivePath.Contains(nextCell))
            return false;

        if (ownerByCell.TryGetValue(nextCell, out int otherPair) && otherPair != ActivePairId)
        {
            ClearPair(otherPair);
        }

        ActivePath.Add(nextCell);
        return true;
    }

    public void CommitActive()
    {
        if (!IsDrawing) return;

        var committed = new List<Vector2Int>(ActivePath);
        pathsByPair[ActivePairId] = committed;

        for (int i = 0; i < committed.Count; i++)
            ownerByCell[committed[i]] = ActivePairId;

        CancelActive();
    }

    public void CancelActive()
    {
        ActivePairId = -1;
        ActivePath.Clear();
    }

    public void ClearPairAtCell(Vector2Int cell)
    {
        if (TryGetOwner(cell, out int pairId))
            ClearPair(pairId);
    }

    public int GetCommittedLineCount()
    {
        return pathsByPair.Count;
    }

    public bool IsLevelComplete()
    {
        return TotalPairs > 0 && pathsByPair.Count >= TotalPairs;
    }

    public int GetCommittedFilledCellCount()
    {
        return ownerByCell.Count;
    }
    public void ClearAll()
    {
        pathsByPair.Clear();
        ownerByCell.Clear();
        CancelActive();
    }
}
