using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Image startButton;
    public GridSpawner gridSpawner;
    public PathManager pathManager;
    public DotSpawner dotSpawner;
    public PathLineRenderer pathLineRenderer;
    public CellHighlighter cellHighlighter;
    public InputDrawPaths input;
    public LevelData level;

    public CanvasGroup gameplayUI;
    public Image star;
    public TextMeshProUGUI flowText;
    public TextMeshProUGUI pipeText;
    public TextMeshProUGUI levelText;
    public Image nextButton;
    public Button resetButton;
    private bool _celebrating = false;
    public int maxLevel;


    public int currentLevel;

    void Start()
    {
        startButton.DOFade(1f, 0.5f);
    }

    void Update()
    {

        int flows = pathManager.GetCommittedLineCount();
        flowText.text = $"Flow: {flows}/{dotSpawner.thisLevel.pairs.Count}";

        int filledCells = pathManager.GetCommittedFilledCellCount();
        if (pathManager.IsDrawing)
        {
            foreach (var c in pathManager.ActivePath)
                if (!pathManager.TryGetOwner(c, out _)) filledCells++;
        }
        int totalCells = gridSpawner.width * gridSpawner.height;

        float pct = totalCells > 0 ? (filledCells / (float)totalCells) * 100f : 0f;
        pipeText.text = $"Pipe: {pct:0}%";

        levelText.text = $"Level {currentLevel + 1}";

        if (pathManager.IsLevelComplete() && pct == 100 && !_celebrating)
        {
            _celebrating = true;
            PlayLevelComplete();
        }

        
    }

    private void PlayLevelComplete()
    {
        bool endGame = false;
        if (currentLevel + 1 == maxLevel) endGame = true;
        cellHighlighter.enabled = false;
        input.enabled = false;
        resetButton.interactable = false;

        float cellDelay = 0f;
        float stagger = 0.03f;
        Color[] colors = dotSpawner.colors;

        for (int y = 0; y < gridSpawner.height; y++)
            for (int x = 0; x < gridSpawner.width; x++)
            {
                var coord = new Vector2Int(x, y);
                if (!gridSpawner.cellVisuals.TryGetValue(coord, out var cv)) continue;

                Color fillColor;
                if (pathManager.TryGetOwner(coord, out int pairId))
                    fillColor = colors[Mathf.Abs(pairId) % colors.Length];
                else
                    fillColor = Color.white;

                cv.FillColor(fillColor, 0.2f, cellDelay, 0.12f);
                cellDelay += stagger;
            }

        pathLineRenderer.gameObject.SetActive(false);

        dotSpawner.FadeOutAndClear(0.3f);

        float afterFill = cellDelay + 0.4f;

        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(afterFill);
        if (!endGame)
        {
            seq.Append(nextButton.DOFade(1f, 1f));
            seq.AppendCallback(() => nextButton.raycastTarget = true);
        }
        else
        {
            seq.AppendCallback(PlayEndGameCelebration);
            seq.AppendInterval(0.5f);
            seq.Append(star.DOFade(1f, 1f));
            //seq.AppendCallback(() => star.raycastTarget = true);
        }
    }

    public void OnStartButtonClicked()
    {
        startButton.raycastTarget = false;
        DOTween.Kill(this);

        Sequence seq = DOTween.Sequence();

        seq.Append(startButton.transform.DOPunchScale(new Vector3(0.2f, 0.2f), 0.5f, 0, 0));
        seq.Append(startButton.DOFade(0f, 0.5f));

        seq.AppendInterval(0.5f);

        seq.AppendCallback(gridSpawner.SpawnGridAnimated);
        seq.AppendInterval(2f);
        seq.Append(gameplayUI.DOFade(1f, 0.5f));
        seq.AppendCallback(() => gameplayUI.interactable = true);
    }

    public void OnNextButtonClicked()
    {
        nextButton.raycastTarget = false;
        resetButton.interactable = true;

        Sequence seq = DOTween.Sequence();
        seq.Append(nextButton.DOFade(0f, 0.3f));
        seq.AppendInterval(0.1f);

        seq.AppendCallback(() =>
        {
            float cellDelay = 0f;
            float stagger = 0.03f;

            for (int y = 0; y < gridSpawner.height; y++)
                for (int x = 0; x < gridSpawner.width; x++)
                {
                    var coord = new Vector2Int(x, y);
                    if (gridSpawner.cellVisuals.TryGetValue(coord, out var cv))
                    {
                        cv.SpinReset(0.35f, cellDelay);
                        cellDelay += stagger;
                    }
                }
        });

        float totalSpinTime = (gridSpawner.width * gridSpawner.height) * 0.03f + 0.35f + 0.2f;
        seq.AppendInterval(totalSpinTime);

        seq.AppendCallback(LoadNextLevel);
    }

    public void OnResetButtonClicked()
    {
        pathManager.ClearAll();
    }

    private void LoadNextLevel()
    {
        _celebrating = false;

        currentLevel++;

        cellHighlighter.enabled = true;
        pathLineRenderer.gameObject.SetActive(true);

        pathManager.ClearAll();

        int randomLevel = GetRandomLevelIndex();


        input.enabled = true;
        input.levelIndex = randomLevel;
        input.BuildEndpointLookup();

        dotSpawner.SpawnPairs(randomLevel);
    }

    private bool _endDancing = false;

    private void PlayEndGameCelebration()
    {
        _endDancing = true;

        Color[] colors = dotSpawner.colors;

        float cx = (gridSpawner.width - 1) / 2f;
        float cy = (gridSpawner.height - 1) / 2f;

        for (int y = 0; y < gridSpawner.height; y++)
        {
            for (int x = 0; x < gridSpawner.width; x++)
            {
                var coord = new Vector2Int(x, y);
                if (!gridSpawner.cellVisuals.TryGetValue(coord, out var cv)) continue;

                float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                float delay = dist * 0.12f;

                
                int offset = (x + y) % colors.Length;
                Color[] shifted = new Color[colors.Length];
                for (int i = 0; i < colors.Length; i++)
                    shifted[i] = colors[(i + offset) % colors.Length];

                cv.BreathingGlow(shifted, delay, softness: 0.25f, stepDuration: 1.8f);
            }
        }
    }

    public void StopEndGameCelebration()
    {
        if (!_endDancing) return;
        _endDancing = false;

        for (int y = 0; y < gridSpawner.height; y++)
            for (int x = 0; x < gridSpawner.width; x++)
            {
                var coord = new Vector2Int(x, y);
                if (gridSpawner.cellVisuals.TryGetValue(coord, out var cv))
                    cv.StopDance();
            }
    }

    private int GetRandomLevelIndex()
    {
        int count = level.levels.Count;

        int newIndex;
        do
        {
            newIndex = Random.Range(1, count);
        }
        while (newIndex == currentLevel);

        return newIndex;
    }
}