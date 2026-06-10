using System.Collections;
using System.IO;
using System.IO.Compression; // Usamos la librería nativa ultra-compatible para Android/Meta Quest
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

    private float audioClipLength;

    void Start()
    {
        string cancionElegida = SongMenuManager.CancionSeleccionada;

        if (string.IsNullOrEmpty(cancionElegida))
        {
            Debug.LogError("[GameSceneManager] ERROR: No se detectó ninguna canción seleccionada en SongMenuManager.CancionSeleccionada.");
            return;
        }

        StartCoroutine(ProcesarZipYJugar(cancionElegida));
    }

    IEnumerator ProcesarZipYJugar(string nombreZip)
    {
        string rutaArchivoZip = Path.Combine(Application.dataPath, $"Resources/MusicFiles/ZipFiles/{nombreZip}.zip");
        Debug.Log($"[GameSceneManager] Intentando buscar el archivo ZIP en: {rutaArchivoZip}");

        if (!File.Exists(rutaArchivoZip))
        {
            Debug.LogError($"[GameSceneManager] ERROR: No existe el archivo ZIP en la ruta especificada: {rutaArchivoZip}");
            yield break;
        }

        string contenidoOsu = "";
        byte[] audioBytes = null;
        string nombreArchivoAudio = "";
        AudioType tipoDeAudio = AudioType.UNKNOWN; // Se asignará dinámicamente

        string mapaRespaldoTexto = "";
        long menorPesoOsu = long.MaxValue;

        Debug.Log($"[GameSceneManager] ¡ZIP Encontrado! Abriendo con System.IO.Compression: {nombreZip}.zip");

        using (FileStream fs = File.OpenRead(rutaArchivoZip))
        {
            using (ZipArchive zipFile = new ZipArchive(fs, ZipArchiveMode.Read))
            {
                Debug.Log($"[GameSceneManager] Archivos internos totales localizados en el ZIP: {zipFile.Entries.Count}");

                foreach (ZipArchiveEntry entrada in zipFile.Entries)
                {
                    if (entrada.FullName.EndsWith("/") || entrada.FullName.EndsWith("\\")) continue;

                    string nombreLimpio = entrada.Name;
                    Debug.Log($"[GameSceneManager] Archivo dentro del ZIP detectado: '{nombreLimpio}' ({entrada.Length} bytes)");

                    // 1. PROCESAR ARCHIVOS .OSU
                    if (nombreLimpio.EndsWith(".osu", System.StringComparison.OrdinalIgnoreCase))
                    {
                        string textoExtraido = "";
                        using (Stream zipStream = entrada.Open())
                        {
                            using (StreamReader reader = new StreamReader(zipStream, System.Text.Encoding.UTF8))
                            {
                                textoExtraido = reader.ReadToEnd();
                            }
                        }

                        if (nombreLimpio.ToUpper().Contains("[BEGINNER]"))
                        {
                            contenidoOsu = textoExtraido;
                            Debug.Log($"[GameSceneManager] -> Dificultad prioritaria [BEGINNER] encontrada: {nombreLimpio}");
                        }
                        else if (entrada.Length < menorPesoOsu && entrada.Length > 0)
                        {
                            menorPesoOsu = entrada.Length;
                            mapaRespaldoTexto = textoExtraido;
                        }
                    }

                    // 2. PROCESAR ARCHIVO DE AUDIO (audio.mp3 o audio.ogg)
                    string nombreSinExtension = Path.GetFileNameWithoutExtension(nombreLimpio);
                    string extension = Path.GetExtension(nombreLimpio).ToLower();

                    if (nombreSinExtension.Equals("audio", System.StringComparison.OrdinalIgnoreCase) &&
                        (extension == ".mp3" || extension == ".ogg"))
                    {
                        nombreArchivoAudio = nombreLimpio;
                        tipoDeAudio = (extension == ".mp3") ? AudioType.MPEG : AudioType.OGGVORBIS;

                        using (Stream zipStream = entrada.Open())
                        {
                            using (MemoryStream audioMs = new MemoryStream())
                            {
                                zipStream.CopyTo(audioMs);
                                audioBytes = audioMs.ToArray();
                            }
                        }
                        Debug.Log($"[GameSceneManager] -> Audio detectado y extraído ({extension.ToUpper()}): {nombreLimpio}");
                    }
                }
            }
        }

        if (string.IsNullOrEmpty(contenidoOsu) && !string.IsNullOrEmpty(mapaRespaldoTexto))
        {
            contenidoOsu = mapaRespaldoTexto;
            Debug.LogWarning("[GameSceneManager] No se encontró mapa '[BEGINNER]'. Se aplicará el mapa .osu de respaldo por peso menor.");
        }

        // ENVIAR NOTAS AL SPAWNER
        if (!string.IsNullOrEmpty(contenidoOsu))
        {
            CubeSpawnManager spawner = cubeSpawnManager.GetComponent<CubeSpawnManager>();
            if (spawner != null)
            {
                Debug.Log($"[GameSceneManager] Enviando texto del .osu al CubeSpawnManager. Longitud del string: {contenidoOsu.Length} caracteres.");
                spawner.InicializarMapaDesdeTexto(contenidoOsu);
            }
            else
            {
                Debug.LogError("[GameSceneManager] ERROR CRÍTICO: El objeto 'cubeSpawnManager' asignado no contiene el componente 'CubeSpawnManager'.");
            }
        }
        else
        {
            Debug.LogError("[GameSceneManager] ERROR CRÍTICO: El string final 'contenidoOsu' está completamente vacío. El ZIP no tiene archivos .osu válidos.");
        }

        // CARGAR EL AUDIO DINÁMICO EN UNITY
        if (audioBytes != null && tipoDeAudio != AudioType.UNKNOWN)
        {
            string rutaTemporal = Path.Combine(Application.temporaryCachePath, nombreArchivoAudio);
            File.WriteAllBytes(rutaTemporal, audioBytes);

            using (UnityWebRequest multimediaRequest = UnityWebRequestMultimedia.GetAudioClip("file://" + rutaTemporal, tipoDeAudio))
            {
                yield return multimediaRequest.SendWebRequest();

                if (multimediaRequest.result == UnityWebRequest.Result.Success)
                {
                    AudioClip clipDeMusica = DownloadHandlerAudioClip.GetContent(multimediaRequest);
                    AudioManager.instance.musicTheme.clip = clipDeMusica;
                    Debug.Log($"[GameSceneManager] Audio ({tipoDeAudio}) cargado con éxito en el AudioManager global.");

                    if (File.Exists(rutaTemporal)) File.Delete(rutaTemporal);
                }
                else
                {
                    Debug.LogError("[GameSceneManager] Error de red/multimedia al cargar el clip de audio: " + multimediaRequest.error);
                }
            }
        }
        else
        {
            Debug.LogError("[GameSceneManager] ERROR: No se encontró un archivo válido 'audio.mp3' o 'audio.ogg' dentro del ZIP.");
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

            if (audioClipLength > 0)
            {
                progressBarImage.fillAmount = (AudioManager.instance.musicTheme.time / audioClipLength);
            }
        }
        GameOver();
    }

    public void GameOver()
    {
        Debug.Log("[GameSceneManager] Fin de la canción alcanzado (GameOver).");
        cubeSpawnManager.SetActive(false);
        timerUI_Gameobject.SetActive(false);
    }

    private string ConvertToMinAndSeconds(float totalTimeInSeconds)
    {
        return Mathf.Floor(totalTimeInSeconds / 60).ToString("00") + ":" + Mathf.FloorToInt(totalTimeInSeconds % 60).ToString("00");
    }
}