using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridSpawner : MonoBehaviour
{
    public GameObject cellPrefab;

    public int width = 5;
    public int height = 5;

    public DotSpawner dotSpawner;

    public float cellSpacing = 1.05f;

    public float popDuration = 0.3f;
    public float popStagger = 0.1f;
    public Ease popEase = Ease.OutBack;
    public Image startButton;

    public readonly Dictionary<Vector2Int, CellVisual> cellVisuals = new();

    public void SpawnGridAnimated()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        int index = 0;

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                Vector3 pos = CellToWorld(x, y);

                var cell = Instantiate(cellPrefab, pos, Quaternion.identity, transform);
                cell.name = $"Cell_{x}_{y}";

                var cv = cell.GetComponent<CellVisual>();
                if (cv != null)
                    cellVisuals[new Vector2Int(x, y)] = cv;

                var t = cell.transform;
                Vector3 basePos = t.position;

                t.position = basePos + Vector3.up * -1f;
                SpriteRenderer tSprite = t.GetComponent<SpriteRenderer>();
                tSprite.DOFade(0f, 0f);

                float delay = index * popStagger;

                t.DOMove(basePos, popDuration).SetDelay(delay).SetEase(Ease.OutQuad);
                tSprite.DOFade(1f, popDuration).SetDelay(delay).SetEase(Ease.OutQuad);

                index++;
            }

        float totalDelay = (width * height - 1) * popStagger + popDuration;

        DOVirtual.DelayedCall(totalDelay, () =>
        {
            dotSpawner.SpawnPairs(0);
        });
    }

    public Vector3 CellToWorld(int x, int y)
    {
        float startX;
        float startY;

        startX = -((width - 1) * cellSpacing) / 2f;
        startY = -((height - 1) * cellSpacing) / 2f;

        return new Vector3(
            startX + x * cellSpacing,
            startY + y * cellSpacing,
            0f
        );
    }
}
