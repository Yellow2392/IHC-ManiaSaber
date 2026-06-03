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
    public GameObject cubeSpawnManager;

    // Audio related
    float audioClipLength;
    private float timeToStartGame = 5.0f;

    void Start()
    {
        // ==========================================
        // NUEVO: CARGAR CANCIÓN DINÁMICAMENTE
        // ==========================================
        // Leemos el nombre de la canción que guardamos en el menú
        string cancionElegida = SongMenuManager.CancionSeleccionada;

        // Por si acaso entraste directamente a la escena de juego sin pasar por el menú (para pruebas)
        if (string.IsNullOrEmpty(cancionElegida))
        {
            Debug.LogWarning("No se detectó ninguna canción del menú. Usando una por defecto o la que ya esté en el AudioManager.");
        }
        else
        {
            Debug.Log("Cargando archivos para la canción: " + cancionElegida);

            // 1. Cargamos el archivo de Audio (.mp3) desde Resources
            AudioClip nuevoClip = Resources.Load<AudioClip>($"MusicFiles/AudioFiles/{cancionElegida}");

            if (nuevoClip != null)
            {
                // Se lo asignamos al AudioSource de tu AudioManager
                AudioManager.instance.musicTheme.clip = nuevoClip;
            }
            else
            {
                Debug.LogError($"No se encontró el archivo de audio en: Resources/MusicFiles/AudioFiles/{cancionElegida}");
            }

            // 2. Cargamos el archivo de Texto (.txt) por si tu mapeador de cubos lo necesita
            TextAsset archivoTexto = Resources.Load<TextAsset>($"MusicFiles/TextFiles/{cancionElegida}");

            if (archivoTexto != null)
            {
                // Aquí tienes el texto listo para usar. 
                // Puedes pasárselo a tu 'cubeSpawnManager' si lo necesitas, por ejemplo:
                // cubeSpawnManager.GetComponent<TuScriptDeCubos>().CargarNotas(archivoTexto.text);
                Debug.Log("Archivo de texto (.txt) cargado correctamente.");
            }
            else
            {
                Debug.LogWarning($"No se encontró el archivo de texto en: Resources/MusicFiles/TextFiles/{cancionElegida}");
            }
        }
        // ==========================================

        // IMPORTANTE: Asegurarnos de que la música empiece a reproducirse con el nuevo clip
        if (AudioManager.instance.musicTheme.clip != null)
        {
            AudioManager.instance.musicTheme.Play();
        }

        // Tu código original continúa exactamente igual aquí:
        audioClipLength = AudioManager.instance.musicTheme.clip.length;
        Debug.Log(audioClipLength);

        // Starting the countdown with song
        StartCoroutine(StartCountdown(audioClipLength));

        // Resetting progress bar
        progressBarImage.fillAmount = Mathf.Clamp(0, 0, 1);
    }

    public IEnumerator StartCountdown(float countdownValue)
    {
        while (countdownValue > 0)
        {
            yield return new WaitForSeconds(1.0f);
            countdownValue -= 1;

            timeText.text = ConvertToMinAndSeconds(countdownValue);

            // Evitamos un error de división por cero si por alguna razón no hay audio
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

        // Disable cube spawning
        cubeSpawnManager.SetActive(false);

        // Disable timer UI
        timerUI_Gameobject.SetActive(false);
    }

    private string ConvertToMinAndSeconds(float totalTimeInSeconds)
    {
        string timeText = Mathf.Floor(totalTimeInSeconds / 60).ToString("00") + ":" + Mathf.FloorToInt(totalTimeInSeconds % 60).ToString("00");
        return timeText;
    }
}