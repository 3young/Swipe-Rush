using TMPro;
using UnityEngine;

/// <summary>
/// 게임 점수를 관리하는 싱글톤 클래스
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance { get; private set; } // 싱글톤 인스턴스
    public int currentScore = 0;     // 현재 점수
    public TMP_Text scoreText;       // 점수 표시 텍스트

    /// <summary>
    /// 싱글톤 인스턴스 초기화
    /// </summary>
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
    }
    
    /// <summary>
    /// 점수 추가
    /// </summary>
    /// <param name="amount">추가할 점수</param>
    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreUI();
    }

    /// <summary>
    /// 점수 초기화
    /// </summary>
    public void ResetScore()
    {
        currentScore = 0;
        UpdateScoreUI();
    }

    /// <summary>
    /// 점수 UI 업데이트
    /// </summary>
    private void UpdateScoreUI()
    {
        if(scoreText != null)
        {
            scoreText.text = currentScore.ToString("0");
        }
    }
}
