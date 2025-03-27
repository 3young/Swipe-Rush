using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoundManager : MonoBehaviour
{
    public UIManager uiManager;
    public ScoreManager scoreManager;
    public Board board;
    public bool waitingForAutoMatches = false;

    private void Awake()
    {
        Board board = FindFirstObjectByType<Board>();
    }

    public void tryEndRoundAfterAutoMatches()
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

        if (uiManager != null && uiManager.roundOverScreen != null)
        {
            uiManager.roundOverScreen.SetActive(true); // 게임 오버 패널 활성화
        }
        else
        {
            Debug.LogWarning("UIManager 또는 RoundOverScreen이 할당되지 않았습니다.");
        }

        if (scoreManager != null)
        {
            uiManager.finalScoreTEXT.text = scoreManager.currentScore.ToString("0");
        }
        else
        {
            Debug.LogWarning("ScoreManager가 할당되지 않았습니다.");
        }
    }
}
