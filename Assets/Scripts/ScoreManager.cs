using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("UI")]
    public TextMeshProUGUI scoreText;

    private int puntajeActual = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void SumarPuntos(int puntos)
    {
        puntajeActual += puntos;
        if (scoreText != null)
        {
            scoreText.text = puntajeActual.ToString();
        }
    }
}