using UnityEngine;

public class CubeHit : MonoBehaviour
{
    [HideInInspector] public float tiempoGolpeExacto;
    [HideInInspector] public int tipoCuboAsignado; // 0 para A, 1 para B

    [Header("Configuración de Golpe")]
    public float margenErrorMaximo = 0.2f; // Segundos de tolerancia para hit
    public int puntajeMaximo = 100;

    void OnTriggerEnter(Collider other)
    {
        Saber sable = other.GetComponent<Saber>();

        if (sable != null && sable.tipoSable == tipoCuboAsignado)
        {
            float tiempoActual = AudioManager.instance.musicTheme.time;
            float diferencia = Mathf.Abs(tiempoGolpeExacto - tiempoActual);

            if (diferencia <= margenErrorMaximo)
            {
                CalcularPuntaje(diferencia);
                Destroy(gameObject); // O lanzar sistema de partículas
            }
        }
    }

    void CalcularPuntaje(float diferencia)
    {
        // Interpola la precisión: 0 diferencia = 100% puntaje. Max diferencia = 0% puntaje.
        float precision = 1.0f - (diferencia / margenErrorMaximo);
        int puntosObtenidos = Mathf.Max(10, Mathf.RoundToInt(precision * puntajeMaximo)); 

        ScoreManager.Instance.SumarPuntos(puntosObtenidos);
    }
}