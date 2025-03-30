using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 레벨 선택 메뉴를 관리하는 클래스
/// </summary>
public class LevelSelectMenu : MonoBehaviour
{
    /// <summary>메인 메뉴 씬 이름</summary>
    public string mainMenu = "Main Menu";

    /// <summary>
    /// 메인 메뉴로 돌아감
    /// </summary>
    public void GoToMainMenu()
    {
        SceneManager.LoadScene(mainMenu);
    }
}
