using UnityEngine;
using DG.Tweening;

public class CellVisual : MonoBehaviour
{
    public SpriteRenderer sr;

    private Color _baseColor;
    private Color _currentTint = Color.clear;
    private Tween _tween;

    private void Awake()
    {
        if (!sr) sr = GetComponent<SpriteRenderer>();
        _baseColor = sr.color;
    }

    public void SetHighlight(Color color, float softness = 0.35f)
    {
        Color target = Color.Lerp(_baseColor, color, softness);

        if (target == _currentTint) return;
        _currentTint = target;

        _tween?.Kill();
        _tween = sr.DOColor(target, 0.15f).SetEase(Ease.OutQuad);
    }

    public void ClearHighlight()
    {

        if (_currentTint == _baseColor) return;
        _currentTint = _baseColor;

        _tween?.Kill();
        _tween = sr.DOColor(_baseColor, 0.2f).SetEase(Ease.OutQuad);
    }

    public void FillColor(Color color, float duration, float delay, float punchAmount = 0.15f)
    {
        _tween?.Kill();

        _tween = sr.DOColor(color, duration)
            .SetDelay(delay)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                transform.DOPunchScale(Vector3.one * punchAmount, 0.2f, 6, 0.6f);
            });
    }

    public void FadeOut(float duration, float delay)
    {
        _tween?.Kill();

        Color transparent = new Color(sr.color.r, sr.color.g, sr.color.b, 0f);
        _tween = sr.DOColor(transparent, duration)
            .SetDelay(delay)
            .SetEase(Ease.InQuad);
    }

    public void KillTweens()
    {
        _tween?.Kill();
        DOTween.Kill(sr);   
        DOTween.Kill(transform); 
    }

    public void SpinReset(float duration, float delay)
    {
        _tween?.Kill();
        DOTween.Kill(sr);
        DOTween.Kill(transform);

        float halfDuration = duration * 0.5f;

        Sequence spin = DOTween.Sequence();
        spin.SetDelay(delay);

        spin.Append(
            transform.DORotate(new Vector3(0f, 90f, 0f), halfDuration, RotateMode.Fast)
                .SetEase(Ease.InQuad)
        );

        spin.AppendCallback(() =>
        {
            sr.color = _baseColor;
            _currentTint = _baseColor;
        });

        spin.Append(
            transform.DORotate(new Vector3(0f, 180f, 0f), halfDuration, RotateMode.Fast)
                .SetEase(Ease.OutQuad)
        );

        spin.AppendCallback(() =>
        {
            transform.rotation = Quaternion.identity;
        });
    }

    private Sequence _danceSeq;

    private Tween _danceTween;

    public void BreathingGlow(Color[] palette, float delay, float softness = 0.3f, float stepDuration = 1.5f)
    {
        KillTweens();

        Sequence seq = DOTween.Sequence();
        seq.SetDelay(delay);
        seq.SetLoops(-1);

        for (int i = 0; i < palette.Length; i++)
        {
            Color target = Color.Lerp(_baseColor, palette[i], softness);
            seq.Append(sr.DOColor(target, stepDuration).SetEase(Ease.InOutSine));
        }

        _danceTween = seq;
    }

    public void StopDance(float fadeDuration = 0.3f)
    {
        _danceTween?.Kill();
        _danceTween = null;

        _tween?.Kill();
        DOTween.Kill(sr);
        DOTween.Kill(transform);

        sr.DOColor(_baseColor, fadeDuration);
        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.identity;
    }
}