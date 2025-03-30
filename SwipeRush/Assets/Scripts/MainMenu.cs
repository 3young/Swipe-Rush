using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임의 메인 메뉴를 관리하는 클래스
/// </summary>
public class MainMenu : MonoBehaviour
{
    /// <summary>게임 시작 시 로드할 레벨</summary>
    public string levelToLoad;

    /// <summary>
    /// 게임을 시작
    /// </summary>
    public void StartGame()
    {
        SceneManager.LoadScene(levelToLoad);
    }

    /// <summary>
    /// 게임을 종료
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }
}
