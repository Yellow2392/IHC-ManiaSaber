using UnityEngine;
using UnityEngine.SceneManagement;

// RF-15 — Acciones post-partida. Se cablea a los botones del panel de resultados
// (UI_ScoreFinal) en GameScene. Los resultados viven en la misma escena, por lo que
// reintentar = recargar GameScene (el ScoreManager se recrea con los contadores en 0).
//
// La entrada se lee por botones del control (OVRInput), igual que el menú de pausa
// (ver PauseManager): en VR esta escena no tiene EventSystem ni raycaster de UI activo,
// así que los botones del panel nunca reciben clics. Este componente vive en el
// GameObject ScoreManager (activo toda la partida), por lo que solo se atiende el
// control mientras el panel de resultados está visible.
public class ResultadosController : MonoBehaviour
{
    [Tooltip("Nombre EXACTO de la escena de selección de canciones.")]
    public string escenaSeleccion = "MenuSongs";

    [Tooltip("Panel de resultados (UI_ScoreFinal). Solo se leen los botones del control mientras está visible.")]
    public GameObject panelResultados;

    void Update()
    {
        // Solo cuando la pantalla de resultados está en pantalla.
        if (panelResultados == null || !panelResultados.activeInHierarchy) return;

        // Mismo mapeo que el menú de pausa: B = reiniciar la canción, X = volver al menú.
        if (OVRInput.GetDown(OVRInput.RawButton.B)) Reintentar();
        else if (OVRInput.GetDown(OVRInput.RawButton.X)) VolverASeleccion();
    }

    // Botón "Reintentar": recarga la canción actual desde cero.
    public void Reintentar()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Botón "Volver a canciones": regresa al menú de selección.
    public void VolverASeleccion()
    {
        SceneManager.LoadScene(escenaSeleccion);
    }
}
