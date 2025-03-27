using JetBrains.Annotations;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance { get; private set; }
    public int currentScore = 0;
    public TMP_Text scoreText;

    private void Awake()
    {
        // 이미 존재하는 인스턴스가 있으면 새로 생성된 인스턴스를 파괴
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
    }
    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreUI();
    }

    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if(scoreText != null)
        {
            scoreText.text = currentScore.ToString("0");
        }
    }
}
