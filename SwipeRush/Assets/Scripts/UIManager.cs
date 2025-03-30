using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임의 UI 요소를 관리하는 클래스
/// 메뉴, 팝업, 게임 종료 화면 등 제어
/// </summary>
public class UIManager : MonoBehaviour
{
    /// <summary>일시 정지 화면</summary>
    public GameObject pauseScreen;
    
    /// <summary>라운드 종료 화면</summary>
    public GameObject roundOverScreen;
    
    /// <summary>별점 UI 요소</summary>
    public GameObject winStars1, winStars2, winStars3;
    
    /// <summary>승리 텍스트</summary>
    public TMP_Text winText;
    
    /// <summary>최종 점수 텍스트</summary>
    public TMP_Text finalScoreText;
    
    /// <summary>레벨 선택 씬 이름</summary>
    public string LevelSelect;

    /// <summary>
    /// UI 요소를 초기화
    /// </summary>
    private void Start()
    {
        // 별 아이콘 초기 비활성화
        winStars1.SetActive(false);
        winStars2.SetActive(false);
        winStars3.SetActive(false);
    }

    /// <summary>
    /// ESC 키 입력을 감지하여 일시 정지 화면을 토글
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    /// <summary>
    /// 일시 정지 상태를 토글
    /// </summary>
    public void TogglePause()
    {
        if (!pauseScreen.activeInHierarchy)
        {
            pauseScreen.SetActive(true);
            Time.timeScale = 0; // 게임 일시 정지
        }
        else
        {
            pauseScreen.SetActive(false);
            Time.timeScale = 1; // 게임 재개
        }
    }

    /// <summary>
    /// 메인 메뉴로 이동
    /// </summary>
    public void GoToMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Main Menu"); 
    }

    /// <summary>
    /// 레벨 선택 화면으로 이동
    /// </summary>
    public void GoToLevelSelect()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(LevelSelect);
    }

    /// <summary>
    /// 현재 레벨을 다시 시작
    /// </summary>
    public void TryAgain()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// 다음 레벨로 이동
    /// </summary>
    public void GoToNextLevel()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    /// <summary>
    /// 게임 종료
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }
}
