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
    public TextMeshProUGUI precisionText;        // % de aciertos sobre el total
    public TextMeshProUGUI aciertosFallosText;   // "Aciertos: X   Fallos: Y"

    private int puntajeActual = 0;
    private int aciertos = 0;   // golpes válidos (sable correcto + dentro de margen)
    private int fallos = 0;     // cubos no golpeados o golpes que no puntuaron
    private const string KEY_HIGH_SCORE = "Highscore_ManiaSaber";

    void Awake()
    {
        // Mismo patrón singleton que AudioManager: evita un manager fantasma si
        // hubiera dos ScoreManager en la escena.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
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

    // RF-10: registro de aciertos/fallos. Cada cubo se resuelve exactamente una vez
    // (lo llaman CubeHit al golpear y CubeMovement al expirar sin golpear).
    public void RegistrarAcierto()
    {
        aciertos++;
    }

    public void RegistrarFallo()
    {
        fallos++;
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

        // 4. RF-14: precisión y conteo de aciertos/fallos.
        int total = aciertos + fallos;
        float precision = total > 0 ? (100f * aciertos / total) : 0f;

        if (precisionText != null)
        {
            precisionText.text = precision.ToString("0.0") + "%";
        }

        if (aciertosFallosText != null)
        {
            aciertosFallosText.text = "Aciertos: " + aciertos + "   Fallos: " + fallos;
        }
    }
}
