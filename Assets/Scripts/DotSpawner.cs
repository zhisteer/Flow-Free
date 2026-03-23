using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using static LevelData;

public class DotSpawner : MonoBehaviour
{
    public GridSpawner grid;
    public GameObject dotPrefab;

    public LevelData levelData;
    public Level thisLevel;

    public Color[] colors =
    {
        Color.red, Color.blue, Color.green, Color.yellow, Color.magenta
    };

    [Header("Dot Spawn Animation")]
    public float popDuration = 0.18f;
    public float popStagger = 0.03f;
    public Ease popEase = Ease.OutBack;
    public float punchScale = 0.08f; 

    private readonly List<GameObject> spawned = new();

    public void SpawnPairs(int level)
    {
        
        for (int i = 0; i < spawned.Count; i++)
            if (spawned[i]) Destroy(spawned[i]);
        spawned.Clear();

        level = Mathf.Clamp(level, 0, levelData.levels.Count - 1);

        thisLevel = levelData.levels[level];

        for (int i = 0; i < thisLevel.pairs.Count; i++)
        {
            var pair = thisLevel.pairs[i];

            Color c = colors[Mathf.Abs(i) % colors.Length];

            SpawnDot(pair.a, c, $"Dot_{i}_A", i, delayIndex: i * 2);
            SpawnDot(pair.b, c, $"Dot_{i}_B", i, delayIndex: i * 2 + 1);
        }
    }

    private void SpawnDot(Vector2Int cell, Color color, string name, int pairId, int delayIndex)
    {
        Vector3 pos = grid.CellToWorld(cell.x, cell.y);
        pos.z = -1f;

        var go = Instantiate(dotPrefab, pos, Quaternion.identity, transform);
        go.name = name;

        var sr = go.GetComponent<SpriteRenderer>();
        sr.color = color;

        var dot = go.GetComponent<Dot>();
        dot = go.AddComponent<Dot>();
        dot.pairId = pairId;
        dot.cell = cell;

        Transform t = go.transform;
        Vector3 targetScale = t.localScale;
        t.localScale = Vector3.zero;

        float delay = delayIndex * popStagger;

        t.DOScale(targetScale, popDuration)
            .SetDelay(delay)
            .SetEase(popEase)
            .OnComplete(() =>
            {
                if (punchScale > 0f)
                {
                    t.DOPunchScale(Vector3.one * punchScale, 0.12f, vibrato: 6, elasticity: 0.8f);
                }
            });

        spawned.Add(go);
    }

    public void FadeOutAndClear(float duration)
    {
        for (int i = 0; i < spawned.Count; i++)
        {
            if (spawned[i] == null) continue;

            var sr = spawned[i].GetComponent<SpriteRenderer>();
            var go = spawned[i];

            if (sr != null)
            {
                DOTween.Kill(sr);
                sr.DOFade(0f, duration).OnComplete(() =>
                {
                    if (go) Destroy(go);
                });
            }
            else
            {
                Destroy(go);
            }
        }

        spawned.Clear();
    }
}