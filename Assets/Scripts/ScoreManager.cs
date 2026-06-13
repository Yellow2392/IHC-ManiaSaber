using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("UI Durante la Partida")]
    public TextMeshProUGUI currentScoreText;

    [Header("UI Pantalla Final (Resultados)")]
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI highScoreText;

    private int puntajeActual = 0;
    private string KEY_HIGH_SCORE = "Highscore_ManiaSaber";

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        // Al iniciar la partida, mostramos 0 en la interfaz actual
        ActualizarTextoActual();
    }

    public void SumarPuntos(int puntos)
    {
        puntajeActual += puntos;
        ActualizarTextoActual();
    }

    private void ActualizarTextoActual()
    {
        if (currentScoreText != null)
        {
            currentScoreText.text = puntajeActual.ToString();
        }
    }

    // El GameSceneManager o tu controlador de fin de partida llamará a este método cuando termine la música
    public void FinalizarPartida()
    {
        // 1. Conseguimos el récord histórico guardado anteriormente (si no existe, es 0)
        int recordActual = PlayerPrefs.GetInt(KEY_HIGH_SCORE, 0);

        // 2. Si el puntaje de esta partida supera al récord, guardamos el nuevo récord
        if (puntajeActual > recordActual)
        {
            recordActual = puntajeActual;
            PlayerPrefs.SetInt(KEY_HIGH_SCORE, recordActual);
            PlayerPrefs.Save(); // Guarda físicamente el dato en el visor Quest / PC
        }

        // 3. Pintamos los datos finales en los textos de la pantalla de resultados
        if (finalScoreText != null)
        {
            finalScoreText.text = puntajeActual.ToString();
        }

        if (highScoreText != null)
        {
            highScoreText.text = recordActual.ToString();
        }
    }
}