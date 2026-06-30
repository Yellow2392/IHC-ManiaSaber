using UnityEngine;
using UnityEngine.SceneManagement;

// RF-15 — Acciones post-partida. Se cablea a los botones del panel de resultados
// (UI_ScoreFinal) en GameScene. Los resultados viven en la misma escena, por lo que
// reintentar = recargar GameScene (el ScoreManager se recrea con los contadores en 0).
public class ResultadosController : MonoBehaviour
{
    [Tooltip("Nombre EXACTO de la escena de selección de canciones.")]
    public string escenaSeleccion = "MenuSongs";

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
