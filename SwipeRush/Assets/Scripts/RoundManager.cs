using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoundManager : MonoBehaviour
{
    private UIManager uiManager;
    public ScoreManager scoreManager;
    public Board board;
    public bool waitingForAutoMatches = false;
    public int scoreTarget1, scoreTarget2, scoreTarget3;

    private void Awake()
    {
        uiManager = FindFirstObjectByType<UIManager>();
        board = FindFirstObjectByType<Board>();
    }

    public void TryEndRoundAfterAutoMatches()
    {
        StartCoroutine(WaitForAutoMatchesCo());
    }

    private IEnumerator WaitForAutoMatchesCo()
    {
        waitingForAutoMatches = true;
        while (board.currentState != Board.BoardState.moving)
        {
            yield return null;
        }

        waitingForAutoMatches = false;
        EndRound();
    }

    public void EndRound()
    {
        Time.timeScale = 0; // 게임 일시 정지
        SFXManager.instance.PlayRoundOver();

        if (uiManager == null || uiManager.roundOverScreen == null)
        {
            Debug.LogWarning("UIManager 또는 RoundOverScreen이 할당되지 않았습니다.");
            return;
        }
        
        uiManager.roundOverScreen.SetActive(true); // 게임 오버 패널 활성화

        if (scoreManager == null)
        {
            Debug.LogWarning("ScoreManager가 할당되지 않았습니다.");
            return;
        }
        
        uiManager.finalScoreText.text = scoreManager.currentScore.ToString("0");
        EvaluateScore(scoreManager.currentScore);
    }

    private void EvaluateScore(int score)
    {
        string sceneKey = SceneManager.GetActiveScene().name;

        if (score >= scoreTarget3)
        {
            ShowResult("Unbelievable!           You're a 3-Star Legend!", uiManager.winStars3);
            PlayerPrefs.SetInt(sceneKey + "_Star", 3);
        }

        else if(score >= scoreTarget2)
        {
            ShowResult("Awesome! You scored 2 Shiny Stars!", uiManager.winStars2);
            PlayerPrefs.SetInt(sceneKey + "_Star", 2);
        }

        else if (score >= scoreTarget1)
        {
            ShowResult("Nice Try! You got 1 Sparkly Star!", uiManager.winStars1);
            PlayerPrefs.SetInt(sceneKey + "_Star", 1);
        }

        else
        {
            uiManager.winText.text = "Oof! No stars this time...           Try again!";
        }

    }

    private void ShowResult(string message, GameObject starObject)
    {
        uiManager.winText.text = message;
        if(starObject != null)
        {
            starObject.SetActive(true);
        }
    }
}
