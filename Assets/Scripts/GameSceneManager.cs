using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameSceneManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI timeText;
    public Image progressBarImage;
    public GameObject timerUI_Gameobject;

    [Header("Managers")]
    public GameObject cubeSpawnManager; // Asegúrate de que este GameObject tenga el script CubeSpawnManager

    float audioClipLength;

    void Start()
    {
        string cancionElegida = SongMenuManager.CancionSeleccionada;

        if (string.IsNullOrEmpty(cancionElegida))
        {
            Debug.LogWarning("No se detectó ninguna canción del menú.");
        }
        else
        {
            Debug.Log("Cargando archivos para la canción: " + cancionElegida);

            // 1. Cargar Audio
            AudioClip nuevoClip = Resources.Load<AudioClip>($"MusicFiles/AudioFiles/{cancionElegida}");
            if (nuevoClip != null)
            {
                AudioManager.instance.musicTheme.clip = nuevoClip;
            }
            else
            {
                Debug.LogError($"No se encontró el archivo de audio en: Resources/MusicFiles/AudioFiles/{cancionElegida}");
            }

            // 2. Cargar Texto y ENTREGARLO al generador de cubos
            TextAsset archivoTexto = Resources.Load<TextAsset>($"MusicFiles/TextFiles/{cancionElegida}");
            if (archivoTexto != null)
            {
                // Buscamos el componente en el GameObject y le pasamos el mapa
                CubeSpawnManager spawner = cubeSpawnManager.GetComponent<CubeSpawnManager>();
                if (spawner != null)
                {
                    spawner.InicializarMapa(archivoTexto);
                    Debug.Log("Archivo de texto entregado con éxito al CubeSpawnManager.");
                }
                else
                {
                    Debug.LogError("El objeto cubeSpawnManager no tiene el script CubeSpawnManager asignado.");
                }
            }
            else
            {
                Debug.LogWarning($"No se encontró el archivo de texto en: Resources/MusicFiles/TextFiles/{cancionElegida}");
            }
        }

        // Iniciar la música
        if (AudioManager.instance.musicTheme.clip != null)
        {
            AudioManager.instance.musicTheme.Play();
            audioClipLength = AudioManager.instance.musicTheme.clip.length;
            StartCoroutine(StartCountdown(audioClipLength));
        }

        progressBarImage.fillAmount = 0f;
    }

    public IEnumerator StartCountdown(float countdownValue)
    {
        while (countdownValue > 0)
        {
            yield return new WaitForSeconds(1.0f);
            countdownValue -= 1;
            timeText.text = ConvertToMinAndSeconds(countdownValue);

            if (audioClipLength > 0)
            {
                progressBarImage.fillAmount = (AudioManager.instance.musicTheme.time / audioClipLength);
            }
        }
        GameOver();
    }

    public void GameOver()
    {
        Debug.Log("Game Over");
        timeText.text = ConvertToMinAndSeconds(0);
        cubeSpawnManager.SetActive(false);
        timerUI_Gameobject.SetActive(false);
    }

    private string ConvertToMinAndSeconds(float totalTimeInSeconds)
    {
        return Mathf.Floor(totalTimeInSeconds / 60).ToString("00") + ":" + Mathf.FloorToInt(totalTimeInSeconds % 60).ToString("00");
    }
}