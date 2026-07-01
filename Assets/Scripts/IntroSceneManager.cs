using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroSceneManager : MonoBehaviour
{
    [Header("Audio de la intro (asignar el AudioClip de 30s en el Inspector)")]
    public AudioClip introClip;

    [Header("UI")]
    public TextMeshProUGUI timeText;
    public Image progressBarImage;

    private float audioClipLength;

    void Start()
    {
        if (AudioManager.instance == null || AudioManager.instance.musicTheme == null || introClip == null)
        {
            Debug.LogError("[IntroSceneManager] Falta introClip o AudioManager no está listo.");
            return;
        }

        AudioManager.instance.musicTheme.clip = introClip;
        AudioManager.instance.musicTheme.Play();
        audioClipLength = introClip.length;
        progressBarImage.fillAmount = 0f;
        StartCoroutine(StartCountdown(audioClipLength));
    }

    public IEnumerator StartCountdown(float countdownValue)
    {
        while (countdownValue > 0)
        {
            yield return new WaitForSeconds(1.0f);
            countdownValue -= 1;
            timeText.text = ConvertToMinAndSeconds(countdownValue);

            if (audioClipLength > 0 && AudioManager.instance.musicTheme.isPlaying)
            {
                progressBarImage.fillAmount = AudioManager.instance.musicTheme.time / audioClipLength;
            }
        }

        IrAlMenuPrincipal();
    }

    private void IrAlMenuPrincipal()
    {
        if (AudioManager.instance != null && AudioManager.instance.musicTheme != null)
        {
            AudioManager.instance.musicTheme.Stop();
        }

        SceneManager.LoadScene("MenuPrincipal");
    }

    private string ConvertToMinAndSeconds(float totalTimeInSeconds)
    {
        return Mathf.Floor(totalTimeInSeconds / 60).ToString("00") + ":" + Mathf.FloorToInt(totalTimeInSeconds % 60).ToString("00");
    }
}
