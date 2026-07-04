using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;
using System.IO;
using System;
using TMPro;

public class SongMenuManager : MonoBehaviour
{
    [Header("Configuración de UI")]
    public Transform contenedorBotones;
    public GameObject prefabBoton;

    [Header("Configuración de Escena")]
    public string nombreEscenaJuego = "GameScene";

    [Header("Audio")]
    public AudioClip sonidoTarjeta;

    public static string CancionSeleccionada { get; private set; }

    void Start()
    {
        // En Android la lectura debe ser asíncrona obligatoriamente
        StartCoroutine(GenerarListaCancionesCO());
    }

    IEnumerator GenerarListaCancionesCO()
    {
        // Limpiar contenedor
        foreach (Transform hijo in contenedorBotones)
        {
            Destroy(hijo.gameObject);
        }

        // Usamos barras inclinadas normales para rutas web/Android (StreamingAssets)
        string rutaLista = $"{Application.streamingAssetsPath}/MusicFiles/lista_canciones.txt";

        using (UnityWebRequest txtRequest = UnityWebRequest.Get(rutaLista))
        {
            yield return txtRequest.SendWebRequest();

            if (txtRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[SongMenuManager] Error cargando lista_canciones.txt: {txtRequest.error}. Asegúrate de que el archivo existe en StreamingAssets/MusicFiles/");
                yield break;
            }

            // Separar líneas del archivo txt
            string[] cancionesFicheros = txtRequest.downloadHandler.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if (cancionesFicheros.Length == 0)
            {
                Debug.LogWarning("La lista de canciones está vacía.");
                yield break;
            }

            // Procesar cada archivo zip listado
            foreach (string nombreZip in cancionesFicheros)
            {
                string nombreLimpio = nombreZip.Trim();
                if (string.IsNullOrEmpty(nombreLimpio)) continue;

                string rutaZip = $"{Application.streamingAssetsPath}/MusicFiles/ZipFiles/{nombreLimpio}.zip";

                using (UnityWebRequest zipRequest = UnityWebRequest.Get(rutaZip))
                {
                    yield return zipRequest.SendWebRequest();

                    if (zipRequest.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogWarning($"No se pudo precargar el zip '{nombreLimpio}': {zipRequest.error}");
                        continue;
                    }

                    // Obtenemos los bytes binarios del zip directamente desde el APK
                    byte[] zipBytes = zipRequest.downloadHandler.data;

                    using (MemoryStream ms = new MemoryStream(zipBytes))
                    {
                        // Mandamos el stream de memoria al lector
                        SongMetadata datos = OsuZipReader.LeerMetadata(ms, nombreLimpio);

                        GameObject nuevoBoton = Instantiate(prefabBoton, contenedorBotones);
                        SongCardController card = nuevoBoton.GetComponent<SongCardController>();

                        if (card != null)
                        {
                            card.Configurar(datos);
                        }
                        else
                        {
                            TMP_Text textoBoton = nuevoBoton.GetComponentInChildren<TMP_Text>();
                            if (textoBoton != null) textoBoton.text = datos.titulo;
                        }

                        Button componenteBoton = nuevoBoton.GetComponent<Button>();
                        if (componenteBoton != null)
                        {
                            componenteBoton.onClick.AddListener(() => SeleccionarCancion(datos.nombreZip));
                        }
                    }
                }
            }
        }
    }

    public void SeleccionarCancion(string nombre)
    {
        CancionSeleccionada = nombre;
        Debug.Log("Canción ZIP seleccionada de StreamingAssets: " + CancionSeleccionada);

        if (AudioGlobal.instance != null)
        {
            if (sonidoTarjeta != null) AudioGlobal.instance.PlaySFX(sonidoTarjeta);
            if (AudioGlobal.instance.musicSource != null) AudioGlobal.instance.musicSource.Stop();
        }

        SceneManager.LoadScene(nombreEscenaJuego);
    }
}