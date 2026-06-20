using System.Collections;
using System.IO;
using System.IO.Compression;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameSceneManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI timeText;
    public Image progressBarImage;
    public GameObject timerUI_Gameobject;

    [Header("Managers")]
    public GameObject cubeSpawnManager;

    [Header("Pantallas de Puntaje")]
    [Tooltip("HUD de puntaje en vivo durante la partida (UI_ScoreCurrent). Se oculta al terminar.")]
    public GameObject hudEnVivo;
    [Tooltip("Pantalla de resultados (UI_ScoreFinal). Arranca oculta y se muestra al terminar.")]
    public GameObject panelResultados;

    private float audioClipLength;

    void Start()
    {
        string cancionElegida = SongMenuManager.CancionSeleccionada;

        // Fallback para poder probar directamente en GameScene sin usar el menú
        if (string.IsNullOrEmpty(cancionElegida))
        {
            cancionElegida = "NOMBRE_DE_TU_ZIP_DE_PRUEBA_AQUI"; // Reemplaza con un nombre de archivo real
            Debug.LogWarning($"[GameSceneManager] Prueba directa. Usando: {cancionElegida}");
        }
        //AudioManager.instance.Stop();
        StartCoroutine(ProcesarZipYJugar(cancionElegida));
    }

    IEnumerator ProcesarZipYJugar(string nombreZip)
    {
        string rutaArchivoZip = Path.Combine(Application.dataPath, $"Resources/MusicFiles/ZipFiles/{nombreZip}.zip");

        if (!File.Exists(rutaArchivoZip))
        {
            Debug.LogError($"[GameSceneManager] No se encontró el ZIP en: {rutaArchivoZip}");
            yield break;
        }

        string contenidoOsu = "";
        byte[] audioBytes = null;
        string nombreArchivoAudio = "";
        AudioType tipoDeAudio = AudioType.UNKNOWN;
        string mapaRespaldoTexto = "";
        long menorPesoOsu = long.MaxValue;

        using (FileStream fs = File.OpenRead(rutaArchivoZip))
        {
            using (ZipArchive zipFile = new ZipArchive(fs, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry entrada in zipFile.Entries)
                {
                    if (entrada.FullName.EndsWith("/") || entrada.FullName.EndsWith("\\")) continue;

                    string nombreLimpio = entrada.Name;

                    if (nombreLimpio.EndsWith(".osu", System.StringComparison.OrdinalIgnoreCase))
                    {
                        string textoExtraido = "";
                        using (Stream zipStream = entrada.Open())
                        using (StreamReader reader = new StreamReader(zipStream, System.Text.Encoding.UTF8))
                        {
                            textoExtraido = reader.ReadToEnd();
                        }

                        if (nombreLimpio.ToUpper().Contains("[BEGINNER]")) contenidoOsu = textoExtraido;
                        else if (entrada.Length < menorPesoOsu && entrada.Length > 0)
                        {
                            menorPesoOsu = entrada.Length;
                            mapaRespaldoTexto = textoExtraido;
                        }
                    }

                    string nombreSinExtension = Path.GetFileNameWithoutExtension(nombreLimpio);
                    string extension = Path.GetExtension(nombreLimpio).ToLower();

                    if (nombreSinExtension.Equals("audio", System.StringComparison.OrdinalIgnoreCase) &&
                        (extension == ".mp3" || extension == ".ogg"))
                    {
                        nombreArchivoAudio = nombreLimpio;
                        tipoDeAudio = (extension == ".mp3") ? AudioType.MPEG : AudioType.OGGVORBIS;

                        using (Stream zipStream = entrada.Open())
                        using (MemoryStream audioMs = new MemoryStream())
                        {
                            zipStream.CopyTo(audioMs);
                            audioBytes = audioMs.ToArray();
                        }
                    }
                }
            }
        }

        if (string.IsNullOrEmpty(contenidoOsu) && !string.IsNullOrEmpty(mapaRespaldoTexto))
            contenidoOsu = mapaRespaldoTexto;

        if (audioBytes != null && tipoDeAudio != AudioType.UNKNOWN)
        {
            string rutaTemporal = Path.Combine(Application.temporaryCachePath, nombreArchivoAudio);
            File.WriteAllBytes(rutaTemporal, audioBytes);

            // CORRECCIÓN: Formato de URI obligatorio para UnityWebRequest local
            string rutaUri = "file:///" + rutaTemporal.Replace("\\", "/");

            using (UnityWebRequest multimediaRequest = UnityWebRequestMultimedia.GetAudioClip(rutaUri, tipoDeAudio))
            {
                yield return multimediaRequest.SendWebRequest();

                if (multimediaRequest.result == UnityWebRequest.Result.Success)
                {
                    AudioClip clipDeMusica = DownloadHandlerAudioClip.GetContent(multimediaRequest);
                    AudioManager.instance.musicTheme.clip = clipDeMusica;
                    if (File.Exists(rutaTemporal)) File.Delete(rutaTemporal);
                }
                else
                {
                    Debug.LogError("[GameSceneManager] Falló carga de audio: " + multimediaRequest.error);
                }
            }
        }

        if (!string.IsNullOrEmpty(contenidoOsu))
        {
            CubeSpawnManager spawner = cubeSpawnManager.GetComponent<CubeSpawnManager>();
            if (spawner != null) spawner.InicializarMapaDesdeTexto(contenidoOsu);
        }

        if (AudioManager.instance.musicTheme.clip != null)
        {
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

            if (audioClipLength > 0 && AudioManager.instance.musicTheme.isPlaying)
            {
                progressBarImage.fillAmount = (AudioManager.instance.musicTheme.time / audioClipLength);
            }
        }
        GameOver();
    }

    public void GameOver()
    {
        cubeSpawnManager.SetActive(false);
        timerUI_Gameobject.SetActive(false);

        // Cierra el ciclo de puntaje: persiste el récord y pinta finalScoreText /
        // highScoreText (ya cableados en el ScoreManager de la escena).
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.FinalizarPartida();
        }

        // Cambia del HUD de juego a la pantalla de resultados.
        if (hudEnVivo != null) hudEnVivo.SetActive(false);
        if (panelResultados != null) panelResultados.SetActive(true);
    }

    private string ConvertToMinAndSeconds(float totalTimeInSeconds)
    {
        return Mathf.Floor(totalTimeInSeconds / 60).ToString("00") + ":" + Mathf.FloorToInt(totalTimeInSeconds % 60).ToString("00");
    }
}