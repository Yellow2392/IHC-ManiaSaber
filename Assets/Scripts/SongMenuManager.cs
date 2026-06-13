using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using TMPro;

public class SongMenuManager : MonoBehaviour
{
    [Header("Configuración de UI")]
    public Transform contenedorBotones;
    public GameObject prefabBotón;

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

        // 2. Definimos la ruta real en el disco duro dentro de tu proyecto
        string rutaCarpeta = Path.Combine(Application.dataPath, "Resources/MusicFiles/ZipFiles");

        if (!Directory.Exists(rutaCarpeta))
        {
            Debug.LogError($"La carpeta no existe en la ruta: {rutaCarpeta}. Asegúrate de crear las carpetas dentro de Assets.");
            return;
        }

        // 3. Buscamos TODOS los archivos que terminen estrictamente en .zip
        string[] archivosZip = Directory.GetFiles(rutaCarpeta, "*.zip");

        if (archivosZip.Length == 0)
        {
            Debug.LogWarning("No se encontró ningún archivo .zip en la carpeta MusicFiles/ZipFiles");
            return;
        }

        foreach (string rutaCompleta in archivosZip)
        {
            // Extrae solo el nombre (ej: de "C:/.../Thriller.zip" a "Thriller")
            string nombreCancion = Path.GetFileNameWithoutExtension(rutaCompleta);

            // 4. Crear el botón
            GameObject nuevoBoton = Instantiate(prefabBotón, contenedorBotones);

            TMP_Text textoBoton = nuevoBoton.GetComponentInChildren<TMP_Text>();
            if (textoBoton != null)
            {
                textoBoton.text = nombreCancion;
            }
            else
            {
                nuevoBoton.GetComponentInChildren<Text>().text = nombreCancion;
            }

            Button componenteBoton = nuevoBoton.GetComponent<Button>();
            componenteBoton.onClick.AddListener(() => SeleccionarCancion(nombreCancion));
        }
    }

    void SeleccionarCancion(string nombre)
    {
        CancionSeleccionada = nombre;
        Debug.Log("Canción ZIP seleccionada: " + CancionSeleccionada);
        SceneManager.LoadScene(nombreEscenaJuego);
    }
}