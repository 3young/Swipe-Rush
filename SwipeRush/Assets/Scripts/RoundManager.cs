using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 라운드를 관리하는 클래스
/// </summary>
public class RoundManager : MonoBehaviour
{
    private UIManager uiManager;           // UI 관리자 참조
    public ScoreManager scoreManager;      // 점수 관리자 참조
    public Board board;                    // 보드 참조
    public bool waitingForAutoMatches = false; // 자동 매치 대기 상태
    public int scoreTarget1, scoreTarget2, scoreTarget3; // 별 획득 점수 목표

    /// <summary>
    /// 필요한 컴포넌트 참조 설정
    /// </summary>
    private void Awake()
    {
        uiManager = FindFirstObjectByType<UIManager>();
        board = FindFirstObjectByType<Board>();
    }

    /// <summary>
    /// 자동 매치 처리 후 라운드 종료 시도
    /// </summary>
    public void TryEndRoundAfterAutoMatches()
    {
        StartCoroutine(WaitForAutoMatchesCo());
    }

    /// <summary>
    /// 자동 매치가 완료될 때까지 기다린 후 라운드 종료
    /// </summary>
    private IEnumerator WaitForAutoMatchesCo()
    {
        waitingForAutoMatches = true;  // 대기 상태 플래그 설정
        
        // 현재 보드가 움직임 상태(모든 처리 완료)가 될 때까지 대기
        while (board.currentState != Board.BoardState.moving)
        {
            yield return null;  // 다음 프레임까지 대기
        }

        waitingForAutoMatches = false;  // 대기 상태 해제
        EndRound();  // 모든 처리가 끝난 후 게임 종료
    }

    /// <summary>
    /// 라운드를 종료하고 결과 표시
    /// </summary>
    public void EndRound()
    {
        Time.timeScale = 0; // 게임 일시 정지
        SFXManager.instance.PlayRoundOver();

        // UI 관리자와 종료 화면이 유효한지 확인
        if (uiManager != null && uiManager.roundOverScreen != null)
        {
            uiManager.roundOverScreen.SetActive(true); // 게임 오버 패널 활성화
            
            // 점수 관리자가 유효한지 확인
            if (scoreManager != null)
            {
                uiManager.finalScoreText.text = scoreManager.currentScore.ToString("0");
                EvaluateScore(scoreManager.currentScore);
            }
        }
    }

    /// <summary>
    /// 최종 점수를 평가하고 별 등급 부여
    /// </summary>
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

    /// <summary>
    /// 결과 메시지와 별 아이콘 표시
    /// </summary>
    private void ShowResult(string message, GameObject starObject)
    {
        uiManager.winText.text = message;
        if(starObject != null)
        {
            starObject.SetActive(true);
        }
    }
}
