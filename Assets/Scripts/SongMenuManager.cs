using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
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
        StartCoroutine(GenerarListaCanciones());
    }

    IEnumerator GenerarListaCanciones()
    {
        foreach (Transform hijo in contenedorBotones)
        {
            Destroy(hijo.gameObject);
        }

        string[] nombresCanciones = null;
        yield return SongZipLibrary.ObtenerNombresCanciones(nombres => nombresCanciones = nombres);

        if (nombresCanciones == null || nombresCanciones.Length == 0)
        {
            Debug.LogWarning("No se encontró ninguna canción en el manifest de StreamingAssets/MusicFiles/ZipFiles");
            yield break;
        }

        foreach (string nombreCancion in nombresCanciones)
        {
            byte[] datosZip = null;
            yield return SongZipLibrary.ObtenerBytesDeCancion(nombreCancion, bytes => datosZip = bytes);

            SongMetadata datos = OsuZipReader.LeerMetadata(datosZip, nombreCancion);
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