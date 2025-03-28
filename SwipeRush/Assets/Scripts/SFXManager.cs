using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager instance;
    public AudioSource gemSound, explodeSound, roundOverSound;

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

    public void PlayGemBreak()
    {
        gemSound.Stop();
        gemSound.pitch = Random.Range(0.8f, 1.2f);
        gemSound.Play();
    }

    public void PlayExplode()
    {
        explodeSound.Stop();
        explodeSound.pitch = Random.Range(0.8f, 1.2f);
        explodeSound.Play();
    }

    public void PlayRoundOver()
    {
        roundOverSound.Play();
    }
}
