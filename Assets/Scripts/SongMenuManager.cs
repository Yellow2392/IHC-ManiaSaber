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

    public static string CancionSeleccionada { get; private set; }

    void Start()
    {
        GenerarListaCanciones();
    }

    void GenerarListaCanciones()
    {
        // 1. Limpiar el contenedor
        foreach (Transform hijo in contenedorBotones)
        {
            Destroy(hijo.gameObject);
        }

        // 2. Ruta real en disco dentro del proyecto
        string rutaCarpeta = Path.Combine(Application.dataPath, "Resources/MusicFiles/ZipFiles");

        if (!Directory.Exists(rutaCarpeta))
        {
            Debug.LogError($"La carpeta no existe en la ruta: {rutaCarpeta}. Asegúrate de crear las carpetas dentro de Assets.");
            return;
        }

        // 3. Buscar todos los archivos que terminen estrictamente en .zip
        string[] archivosZip = Directory.GetFiles(rutaCarpeta, "*.zip");

        if (archivosZip.Length == 0)
        {
            Debug.LogWarning("No se encontró ningún archivo .zip en la carpeta MusicFiles/ZipFiles");
            return;
        }

        foreach (string rutaCompleta in archivosZip)
        {
            // 4. Leer la metadata del .osu dentro del zip (título, artista, BPM, portada...).
            //    nombreZip (nombre del archivo sin extensión) sigue siendo la clave de selección.
            SongMetadata datos = OsuZipReader.LeerMetadata(rutaCompleta);

            // 5. Crear la tarjeta/botón
            GameObject nuevoBoton = Instantiate(prefabBoton, contenedorBotones);

            SongCardController card = nuevoBoton.GetComponent<SongCardController>();
            if (card != null)
            {
                card.Configurar(datos);
            }
            else
            {
                // Respaldo heredado: prefab simple con un solo texto.
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
        SceneManager.LoadScene(nombreEscenaJuego);
    }
}
