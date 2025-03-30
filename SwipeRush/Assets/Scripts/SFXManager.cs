using UnityEngine;

/// <summary>
/// 게임 내 효과음을 관리하는 싱글톤 클래스
/// </summary>
public class SFXManager : MonoBehaviour
{
    /// <summary>싱글톤 인스턴스</summary>
    public static SFXManager instance;
    
    /// <summary>다양한 게임 효과음</summary>
    public AudioSource gemSound, explodeSound, roundOverSound;

    /// <summary>
    /// 싱글톤 인스턴스 초기화
    /// </summary>
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 보석 파괴 효과음 재생
    /// </summary>
    public void PlayGemBreak()
    {
        gemSound.Stop();
        gemSound.pitch = Random.Range(0.8f, 1.2f); // 다양한 피치로 재생
        gemSound.Play();
    }

    /// <summary>
    /// 폭발 효과음 재생
    /// </summary>
    public void PlayExplode()
    {
        explodeSound.Stop();
        explodeSound.pitch = Random.Range(0.8f, 1.2f); // 다양한 피치로 재생
        explodeSound.Play();
    }

    /// <summary>
    /// 라운드 종료 효과음 재생
    /// </summary>
    public void PlayRoundOver()
    {
        roundOverSound.Play();
    }
}
