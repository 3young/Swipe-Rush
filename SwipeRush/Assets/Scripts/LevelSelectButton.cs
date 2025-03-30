using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 레벨 선택 버튼을 관리하는 클래스
/// </summary>
public class LevelSelectButton : MonoBehaviour
{
    public string levelToLoad;          // 로드할 레벨 이름
    public GameObject star1, star2, star3; // 획득한 별 아이콘
    public Button levelButton;          // 레벨 버튼 참조
    public string levelToUnlock;        // 이 레벨을 잠금 해제하기 위한 이전 레벨

    /// <summary>
    /// 버튼 초기화 및 별 상태 설정
    /// </summary>
    private void Start()
    {
        // 별 아이콘 초기 비활성화
        star1.SetActive(false);
        star2.SetActive(false);
        star3.SetActive(false);

        // 레벨 잠금 상태 확인
        bool isUnlocked = true;

        if(!string.IsNullOrEmpty(levelToUnlock))
        {
            // 이전 레벨에서 최소 1개 이상의 별을 획득해야 잠금 해제
            isUnlocked = PlayerPrefs.GetInt(levelToUnlock + "_Star", 0) > 0;
        }

        // 버튼 상호작용 설정
        if(levelButton != null)
        {
            levelButton.interactable = isUnlocked;
        }

        // 레벨이 잠금 해제되었다면 별 상태 업데이트
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

    /// <summary>
    /// 선택한 레벨 로드
    /// </summary>
    public void LoadLevel()
    {
        SceneManager.LoadScene(levelToLoad);
    }
}
