using System.Collections.Generic;
using UnityEngine;

public class InputDrawPaths : MonoBehaviour
{
    public Camera cam;
    public GridSpawner grid;
    public LevelData levelData;
    public int levelIndex = 0;


    public PathManager pathManager;

    private readonly Dictionary<Vector2Int, int> endpointToPair = new();

    private void Awake()
    {
        if (!cam) cam = Camera.main;
    }

    private void Start()
    {
        BuildEndpointLookup();
    }

    public void BuildEndpointLookup()
    {
        endpointToPair.Clear();
        if (levelData == null || levelData.levels == null || levelData.levels.Count == 0) return;

        var level = levelData.levels[Mathf.Clamp(levelIndex, 0, levelData.levels.Count - 1)];
        if (level == null || level.pairs == null) return;

        for (int i = 0; i < level.pairs.Count; i++)
        {
            endpointToPair[level.pairs[i].a] = i;
            endpointToPair[level.pairs[i].b] = i;
        }

        pathManager.TotalPairs = level.pairs.Count;
    }

    private void Update()
    {
        if (pathManager == null || grid == null || cam == null) return;

        if (PointerDown(out Vector2 downPos))
        {
            if (TryGetCell(downPos, out var cell) && endpointToPair.TryGetValue(cell, out int pairId))
            {
                pathManager.StartDrawing(pairId, cell);
            }
        }

        if (pathManager.IsDrawing && PointerHeld(out Vector2 heldPos))
        {
            if (TryGetCell(heldPos, out var cell))
            {
                bool moved = pathManager.TryStepTo(cell);

                if (moved && endpointToPair.TryGetValue(cell, out int touchedPairId))
                {
                    if (touchedPairId == pathManager.ActivePairId)
                    {
                        if (pathManager.ActivePath.Count >= 2 &&
                            cell != pathManager.ActivePath[0])
                        {
                            pathManager.CommitActive();
                        }
                    }
                    else
                    {
                        pathManager.CancelActive();
                    }
                }
            }
        }

        if (pathManager.IsDrawing && PointerUp(out _))
        {
            pathManager.CancelActive();
        }
    }

    private bool EndedOnMatchingDot()
    {
        int pairId = pathManager.ActivePairId;
        if (pairId < 0) return false;
        if (pathManager.ActivePath.Count < 2) return false;

        Vector2Int start = pathManager.ActivePath[0];
        Vector2Int end = pathManager.ActivePath[^1];

        if (!endpointToPair.TryGetValue(start, out int startPair)) return false;
        if (!endpointToPair.TryGetValue(end, out int endPair)) return false;

        return startPair == pairId && endPair == pairId && end != start;
    }

    private bool TryGetCell(Vector2 screenPos, out Vector2Int cell)
    {
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -cam.transform.position.z));
        world.z = 0f;

        float startX = -((grid.width - 1) * grid.cellSpacing) / 2f;
        float startY = -((grid.height - 1) * grid.cellSpacing) / 2f;

        int x = Mathf.RoundToInt((world.x - startX) / grid.cellSpacing);
        int y = Mathf.RoundToInt((world.y - startY) / grid.cellSpacing);

        cell = new Vector2Int(x, y);
        return x >= 0 && x < grid.width && y >= 0 && y < grid.height;
    }

    private bool PointerDown(out Vector2 pos)
    {
        if (Input.GetMouseButtonDown(0)) { pos = Input.mousePosition; return true; }
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) { pos = Input.GetTouch(0).position; return true; }
        pos = default; return false;
    }

    private bool PointerHeld(out Vector2 pos)
    {
        if (Input.GetMouseButton(0)) { pos = Input.mousePosition; return true; }
        if (Input.touchCount > 0)
        {
            var t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary) { pos = t.position; return true; }
        }
        pos = default; return false;
    }

    private bool PointerUp(out Vector2 pos)
    {
        if (Input.GetMouseButtonUp(0)) { pos = Input.mousePosition; return true; }
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended) { pos = Input.GetTouch(0).position; return true; }
        pos = default; return false;
    }
}