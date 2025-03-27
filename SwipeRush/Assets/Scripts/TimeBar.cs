using UnityEngine;
using UnityEngine.UI;

public class TimeBar : MonoBehaviour
{
    public Slider timeSlider;
    public float totalTime = 60f;
    private float remainingTime;

    public RoundManager roundManager;

    private void Start()
    {
        remainingTime = totalTime;
        timeSlider.maxValue = totalTime;
        timeSlider.value = totalTime;
    }

    private void Update()
    {
        if (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
            timeSlider.value = remainingTime;
        }
        else
        {
            remainingTime = 0;
            roundManager.EndRound();
        }
    }

    public void ResetTimer()
    {
        remainingTime = totalTime;
        timeSlider.value = totalTime;
    }
}
