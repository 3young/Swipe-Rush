using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임의 제한 시간을 관리하는 클래스
/// </summary>
public class TimeBar : MonoBehaviour
{
    public Slider timeSlider;          // 시간 표시 슬라이더
    public float totalTime = 60f;      // 총 제한 시간 (초)
    private float remainingTime;       // 남은 시간

    public RoundManager roundManager;  // 라운드 관리자 참조
    
    /// <summary>
    /// 타임바 초기화
    /// </summary>
    private void Start()
    {
        remainingTime = totalTime;
        timeSlider.maxValue = totalTime;
        timeSlider.value = totalTime;
    }

    /// <summary>
    /// 매 프레임마다 남은 시간을 갱신하고 시간이 다 되면 라운드 종료
    /// </summary>
    private void Update()
    {
        if (remainingTime > 0)
        {
            // 시간 감소 및 슬라이더 업데이트
            remainingTime -= Time.deltaTime;
            timeSlider.value = remainingTime;
        }
        else if(remainingTime <= 0 && !roundManager.waitingForAutoMatches)
        {
            // 시간이 다 되면 라운드 종료
            remainingTime = 0;
            roundManager.TryEndRoundAfterAutoMatches();
        }
    }

    /// <summary>
    /// 타이머 초기화
    /// </summary>
    public void ResetTimer()
    {
        remainingTime = totalTime;
        timeSlider.value = totalTime;
    }
}
