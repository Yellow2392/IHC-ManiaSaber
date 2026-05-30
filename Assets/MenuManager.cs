using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Paneles de UI")]
    public GameObject panelPrincipal;
    public GameObject panelOpciones;

    [Header("Configuración de Escena")]
    // Escribe aquí el nombre EXACTO de tu escena principal
    public string nombreEscenaJuego = "GameScene";
    public string nombreEscenaCanciones = "MenuSongs";

    void Start()
    {
        Debug.Log("Inicio...");
    }

    // Se llama al presionar "Jugar"
    public void MostrarOpciones()
    {
        Debug.Log("Inicio...");
        SceneManager.LoadScene("MenuSongs");
    }

    // Se llama al presionar cualquiera de las 3 opciones
    public void CargarEscenaJuego()
    {
        SceneManager.LoadScene(nombreEscenaJuego);
    }

    // Se llama al presionar "Salir"
    public void SalirJuego()
    {
        Debug.Log("Saliendo de la aplicación...");
        Application.Quit();
    }
}