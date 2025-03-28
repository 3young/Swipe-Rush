using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject pauseScreen;
    public GameObject roundOverScreen;
    public GameObject winStars1, winStars2, winStars3;
    public TMP_Text winText;
    public TMP_Text finalScoreText;
    public string LevelSelect;

    private void Start()
    {
        winStars1.SetActive(false);
        winStars2.SetActive(false);
        winStars3.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (!pauseScreen.activeInHierarchy)
        {
            pauseScreen.SetActive(true);
            Time.timeScale = 0;
        }
        else
        {
            pauseScreen.SetActive(false);
            Time.timeScale = 1;
        }
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Main Menu"); 
    }

    public void GoToLevelSelect()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(LevelSelect);
    }

    public void TryAgain()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToNextLevel()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
