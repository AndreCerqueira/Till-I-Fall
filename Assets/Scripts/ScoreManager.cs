using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [SerializeField] private Transform scoreContainer;
    [SerializeField] private Transform bestScoreContainer;
    private int _score = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public void AddScore(int amount)
    {
        _score += amount;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (scoreText == null) return;
        scoreText.text = $"Score: {_score}";

        if (scoreContainer == null) return;
        scoreContainer.DOKill();
        scoreContainer.localScale = Vector3.one;

        scoreContainer.DOScale(1.1f, 0.25f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                scoreContainer.DOScale(1f, 0.15f).SetEase(Ease.InOutSine);
            });
        
        if (bestScoreText == null) return;
        var texts = bestScoreText.text.Split(' ');
        var text = texts.LastOrDefault();
        if (text == null) return;
        int bestScore = text != "" ? int.Parse(text) : 0;
        if (_score > bestScore)
        {
            bestScoreText.text = "Best: " + _score;
            AnimateBestScore();
        }
    }
    
    private void AnimateBestScore()
    {
        if (bestScoreContainer == null) return;
        bestScoreContainer.DOKill();
        bestScoreContainer.localScale = Vector3.one;

        bestScoreContainer.DOScale(1.1f, 0.25f)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                bestScoreContainer.DOScale(1f, 0.2f).SetEase(Ease.InOutSine);
            });
    }

    public void ResetScore()
    {
        _score = 0;
        _score--;
        UpdateUI();
    }
}