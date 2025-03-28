using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSelectButton : MonoBehaviour
{
    public string levelToLoad;
    public GameObject star1, star2, star3;
    public Button levelButton;

    public string levelToUnlock;

    private void Start()
    {
        star1.SetActive(false);
        star2.SetActive(false);
        star3.SetActive(false);

        bool isUnlocked = true;

        if(!string.IsNullOrEmpty(levelToUnlock))
        {
            isUnlocked = PlayerPrefs.GetInt(levelToUnlock + "_Star", 0) > 0;
        }

        if(levelButton != null)
        {
            levelButton.interactable = isUnlocked;
        }

        if (isUnlocked)
        {
            int stars = PlayerPrefs.GetInt(levelToLoad + "_Star", 0);

            if (stars >= 1)
            {
                star1.SetActive(true);
            }

            if (stars >= 2)
            {
                star2.SetActive(true);
            }

            if (stars >= 3)
            {
                star3.SetActive(true);
            }
        }
    }

    public void LoadLevel()
    {
        SceneManager.LoadScene(levelToLoad);
    }

}
