using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
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
        GenerarListaCanciones();
    }

    void GenerarListaCanciones()
    {
        foreach (Transform hijo in contenedorBotones)
        {
            Destroy(hijo.gameObject);
        }

        string rutaCarpeta = Path.Combine(Application.dataPath, "Resources/MusicFiles/ZipFiles");

        if (!Directory.Exists(rutaCarpeta))
        {
            Debug.LogError($"La carpeta no existe en la ruta: {rutaCarpeta}. Asegúrate de crear las carpetas dentro de Assets.");
            return;
        }

        string[] archivosZip = Directory.GetFiles(rutaCarpeta, "*.zip");

        if (archivosZip.Length == 0)
        {
            Debug.LogWarning("No se encontró ningún archivo .zip en la carpeta MusicFiles/ZipFiles");
            return;
        }

        foreach (string rutaCompleta in archivosZip)
        {
            SongMetadata datos = OsuZipReader.LeerMetadata(rutaCompleta);
            GameObject nuevoBoton = Instantiate(prefabBoton, contenedorBotones);

            SongCardController card = nuevoBoton.GetComponent<SongCardController>();
            if (card != null)
            {
                card.Configurar(datos);
            }
            else
            {
                TMP_Text textoBoton = nuevoBoton.GetComponentInChildren<TMP_Text>();
                if (textoBoton != null)
                {
                    textoBoton.text = datos.titulo;
                }
                else
                {
                    Text textoLegacy = nuevoBoton.GetComponentInChildren<Text>();
                    if (textoLegacy != null)
                    {
                        textoLegacy.text = datos.titulo;
                    }
                }
            }

            Button componenteBoton = nuevoBoton.GetComponent<Button>();
            if (componenteBoton != null)
            {
                componenteBoton.onClick.AddListener(() => SeleccionarCancion(datos.nombreZip));
            }
        }
    }

    public void SeleccionarCancion(string nombre)
    {
        CancionSeleccionada = nombre;
        Debug.Log("Canción ZIP seleccionada: " + CancionSeleccionada);

        if (AudioGlobal.instance != null)
        {
            if (sonidoTarjeta != null)
            {
                AudioGlobal.instance.PlaySFX(sonidoTarjeta);
            }

            if (AudioGlobal.instance.musicSource != null)
            {
                AudioGlobal.instance.musicSource.Stop();
            }
        }

        SceneManager.LoadScene(nombreEscenaJuego);
    }
}