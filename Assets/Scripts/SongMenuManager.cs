using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using TMPro; // Quita esto si usas el Text clásico de Unity

public class SongMenuManager : MonoBehaviour
{
    [Header("Configuración de UI")]
    public Transform contenedorBotones; // El objeto con Vertical Layout Group
    public GameObject prefabBotón;      // Tu prefab de botón de canción

    [Header("Configuración de Escena")]
    public string nombreEscenaJuego = "GameScene";

    // Variable estática para que la escena de juego sepa qué canción se eligió
    public static string CancionSeleccionada { get; private set; }

    void Start()
    {
        GenerarListaCanciones();
    }

    void GenerarListaCanciones()
    {
        // Limpiamos el contenedor por si acaso hay botones viejos de prueba
        foreach (Transform hijo in contenedorBotones)
        {
            Destroy(hijo.gameObject);
        }

        // Buscamos todos los archivos de texto en Assets/Resources/MusicFiles/TextFiles
        // Nota: Unity no necesita la extensión (.txt) al usar Resources.LoadAll
        TextAsset[] archivosTexto = Resources.LoadAll<TextAsset>("MusicFiles/TextFiles");

        if (archivosTexto.Length == 0)
        {
            Debug.LogWarning("No se encontraron canciones en Resources/MusicFiles/TextFiles");
            return;
        }

        foreach (TextAsset archivo in archivosTexto)
        {
            string nombreCancion = archivo.name; // Ej: "Song1"

            // Instanciar el botón en el contenedor
            GameObject nuevoBoton = Instantiate(prefabBotón, contenedorBotones);

            // Cambiar el texto del botón (Soporta TextMeshPro)
            TMP_Text textoBoton = nuevoBoton.GetComponentInChildren<TMP_Text>();
            if (textoBoton != null)
            {
                textoBoton.text = nombreCancion;
            }
            else
            {
                // Por si usas el Text antiguo de Unity
                nuevoBoton.GetComponentInChildren<Text>().text = nombreCancion;
            }

            // Asignar la función del Click dinámicamente
            Button componenteBoton = nuevoBoton.GetComponent<Button>();
            componenteBoton.onClick.AddListener(() => SeleccionarCancion(nombreCancion));
        }
    }

    void SeleccionarCancion(string nombre)
    {
        // Guardamos el nombre en la variable estática
        CancionSeleccionada = nombre;
        Debug.Log("Canción seleccionada: " + CancionSeleccionada);

        // Cargamos la escena de juego
        SceneManager.LoadScene(nombreEscenaJuego);
    }
}