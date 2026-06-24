using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Paneles de UI")]
    public GameObject panelPrincipal;
    public GameObject panelOpciones;

    [Header("Configuración de Escena")]
    public string nombreEscenaJuego = "GameScene";
    public string nombreEscenaCanciones = "MenuSongs";

    [Header("Audio")]
    public AudioClip sonidoBoton; // Arrastra tu sonido de clic aquí en el Inspector

    public void MostrarOpciones()
    {
        ReproducirClic();
        SceneManager.LoadScene("MenuSongs");
    }

    public void CargarEscenaJuego()
    {
        ReproducirClic();
        SceneManager.LoadScene(nombreEscenaJuego);
    }

    public void SalirJuego()
    {
        ReproducirClic();
        Application.Quit();
    }

    private void ReproducirClic()
    {
        if (AudioGlobal.instance != null && sonidoBoton != null)
        {
            AudioGlobal.instance.PlaySFX(sonidoBoton);
        }
    }
}