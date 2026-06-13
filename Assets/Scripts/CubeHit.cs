using UnityEngine;

public class CubeHit : MonoBehaviour
{
    [HideInInspector] public float tiempoGolpeExacto; // Asignado por CubeSpawnManager
    [HideInInspector] public int tipoCuboAsignado;    // 0 para A, 1 para B

    [Header("Configuración de Golpe")]
    public float margenErrorMaximo = 0.2f; // Segundos de tolerancia para hit
    public int puntajeMaximo = 100;

    public void ProcesarGolpe(int tipoSableQueGolpeo)
    {
        // 1. Validación de color/mano
        if (tipoSableQueGolpeo == tipoCuboAsignado)
        {
            float tiempoActual = AudioManager.instance.musicTheme.time;
            float diferencia = Mathf.Abs(tiempoGolpeExacto - tiempoActual);

            // 2. Validación de ritmo
            if (diferencia <= margenErrorMaximo)
            {
                CalcularPuntaje(diferencia);
                
                // 3. Feedback de Audio: Reproduce el sonido de corte
                if (AudioManager.instance != null && AudioManager.instance.sliceSound != null)
                {
                    // PlayOneShot evita que el sonido se corte si destruimos el objeto inmediatamente
                    AudioManager.instance.sliceSound.PlayOneShot(AudioManager.instance.sliceSound.clip);
                }

                // TODO: Aquí irá el sistema de partículas en el siguiente paso
                
                Destroy(gameObject); 
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void CalcularPuntaje(float diferencia)
    {
        float precision = 1.0f - (diferencia / margenErrorMaximo);
        int puntosObtenidos = Mathf.Max(10, Mathf.RoundToInt(precision * puntajeMaximo)); 

        // 4. Feedback de Datos: Suma los puntos al ScoreManager global
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SumarPuntos(puntosObtenidos);
        }
    }
}