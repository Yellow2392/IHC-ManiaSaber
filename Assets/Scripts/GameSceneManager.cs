using ICSharpCode.SharpZipBase.Zip; // Usamos la nueva librería ultra-compatible
using System.Collections;
using System.IO;
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
            Debug.LogWarning("No se detectó ninguna canción seleccionada en el menú.");
            return;
        }

        StartCoroutine(ProcesarZipYJugar(cancionElegida));
    }

    IEnumerator ProcesarZipYJugar(string nombreZip)
    {
        string rutaArchivoZip = Path.Combine(Application.dataPath, $"Resources/MusicFiles/ZipFiles/{nombreZip}.zip");

        if (!File.Exists(rutaArchivoZip))
        {
            Debug.LogError($"No se encontró el archivo ZIP en la ruta: {rutaArchivoZip}");
            yield break;
        }

        string contenidoOsu = "";
        byte[] audioBytes = null;
        string nombreArchivoAudio = "music.mp3"; // Nombre por defecto por si acaso

        string mapaRespaldoTexto = "";
        long menorPesoOsu = long.MaxValue;

        Debug.Log($"=== [SHARPZIPLIB] Abriendo con éxito: {nombreZip}.zip ===");

        // Abrimos el archivo usando la nueva librería tolerante a fallos
        using (FileStream fs = File.OpenRead(rutaArchivoZip))
        {
            using (ZipFile zipFile = new ZipFile(fs))
            {
                Debug.Log($"Archivos internos totales en el ZIP: {zipFile.Count}");

                foreach (ZipEntry entrada in zipFile)
                {
                    if (!entrada.IsFile) continue; // Ignorar carpetas vacías

                    string nombreLimpio = entrada.Name;
                    Debug.Log($"-> Archivo detectado dentro del ZIP: '{nombreLimpio}' ({entrada.Size} bytes)");

                    // 1. PROCESAR ARCHIVOS .OSU
                    if (nombreLimpio.EndsWith(".osu", System.StringComparison.OrdinalIgnoreCase))
                    {
                        string textoExtraido = "";
                        using (Stream zipStream = zipFile.GetInputStream(entrada))
                        {
                            using (StreamReader reader = new StreamReader(zipStream, System.Text.Encoding.UTF8))
                            {
                                textoExtraido = reader.ReadToEnd();
                            }
                        }

                        // Si es el mapa Beginner, lo elegimos de inmediato y rompemos la búsqueda de mapas
                        if (nombreLimpio.ToUpper().Contains("[BEGINNER]"))
                        {
                            contenidoOsu = textoExtraido;
                            Debug.Log($"¡Dificultad prioritaria [BEGINNER] encontrada e indexada!: {nombreLimpio}");
                        }
                        // Si no es beginner, calculamos su peso para el respaldo
                        else if (entrada.Size < menorPesoOsu && entrada.Size > 0)
                        {
                            menorPesoOsu = entrada.Size;
                            mapaRespaldoTexto = textoExtraido;
                        }
                    }

                    // 2. PROCESAR ARCHIVO .MP3
                    if (nombreLimpio.EndsWith(".mp3", System.StringComparison.OrdinalIgnoreCase))
                    {
                        // Extraemos el nombre real del archivo sin importar si viene con ruta de carpeta de Windows
                        nombreArchivoAudio = Path.GetFileName(nombreLimpio);

                        using (Stream zipStream = zipFile.GetInputStream(entrada))
                        {
                            using (MemoryStream audioMs = new MemoryStream())
                            {
                                zipStream.CopyTo(audioMs);
                                audioBytes = audioMs.ToArray();
                            }
                        }
                        Debug.Log($"¡Audio .mp3 extraído correctamente en memoria!: {nombreLimpio}");
                    }
                }
            }
        }

        // Si terminó el escaneo y no encontramos un "[BEGINNER]", aplicamos el mapa más liviano que guardamos
        if (string.IsNullOrEmpty(contenidoOsu) && !string.IsNullOrEmpty(mapaRespaldoTexto))
        {
            contenidoOsu = mapaRespaldoTexto;
            Debug.LogWarning("No se encontró mapa '[BEGINNER]'. Se aplicó el mapa .osu de respaldo (el de menor peso).");
        }

        // ENVIAR NOTAS AL SPAWNER
        if (!string.IsNullOrEmpty(contenidoOsu))
        {
            CubeSpawnManager spawner = cubeSpawnManager.GetComponent<CubeSpawnManager>();
            if (spawner != null)
            {
                Debug.Log($"Enviando texto del .osu al CubeSpawnManager. Longitud del string: {contenidoOsu.Length}");
                spawner.InicializarMapaDesdeTexto(contenidoOsu);
            }
            else
            {
                Debug.LogError("No se encontró el script CubeSpawnManager en la escena.");
            }
        }
        else
        {
            Debug.LogError("ERROR CRÍTICO: No se encontró ningún archivo .osu válido dentro del ZIP.");
        }

        // CARGAR EL AUDIO MP3 EN UNITY
        if (audioBytes != null)
        {
            string rutaTemporal = Path.Combine(Application.temporaryCachePath, nombreArchivoAudio);
            File.WriteAllBytes(rutaTemporal, audioBytes);

            using (UnityWebRequest multimediaRequest = UnityWebRequestMultimedia.GetAudioClip("file://" + rutaTemporal, AudioType.MPEG))
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
                    Debug.LogError("Error al cargar el clip de audio en Unity: " + multimediaRequest.error);
                }
            }
        }
        else
        {
            Debug.LogError("No se encontraron bytes de audio .mp3 válidos.");
        }

        // ARRANCAR EL JUEGO Y LA MÚSICA
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
        cubeSpawnManager.SetActive(false);
        timerUI_Gameobject.SetActive(false);
    }

    private string ConvertToMinAndSeconds(float totalTimeInSeconds)
    {
        return Mathf.Floor(totalTimeInSeconds / 60).ToString("00") + ":" + Mathf.FloorToInt(totalTimeInSeconds % 60).ToString("00");
    }
}