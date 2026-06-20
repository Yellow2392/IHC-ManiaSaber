using UnityEngine;

public class CubeHit : MonoBehaviour
{
    [HideInInspector] public float tiempoGolpeExacto; // Asignado por CubeSpawnManager
    [HideInInspector] public int tipoCuboAsignado;    // 0 para A, 1 para B
    [HideInInspector] public bool resuelto = false;   // Evita doble conteo (golpe vs expiración en el mismo frame)

    [Header("Configuración de Golpe")]
    public float margenErrorMaximo = 0.2f; // Segundos de tolerancia para hit
    public int puntajeMaximo = 100;

    public void ProcesarGolpe(int tipoSableQueGolpeo)
    {
        // Si este cubo ya fue resuelto (acierto/fallo) no se cuenta de nuevo.
        if (resuelto) return;
        resuelto = true;

        bool puntuo = false;

        // 1. Validación de color/mano. Solo puntúa el sable correcto; el cubo se
        //    destruye en cualquier caso (al final del método).
        if (tipoSableQueGolpeo == tipoCuboAsignado
            && AudioManager.instance != null && AudioManager.instance.musicTheme != null)
        {
            float tiempoActual = AudioManager.instance.musicTheme.time;
            float diferencia = Mathf.Abs(tiempoGolpeExacto - tiempoActual);

            // 2. Validación de ritmo
            if (diferencia <= margenErrorMaximo)
            {
                CalcularPuntaje(diferencia);
                puntuo = true;

                // 3. Feedback de Audio: Reproduce el sonido de corte
                AudioSource slice = AudioManager.instance.sliceSound;
                if (slice != null && slice.clip != null)
                {
                    // PlayOneShot evita que el sonido se corte al destruir el objeto inmediatamente
                    slice.PlayOneShot(slice.clip);
                }

                // TODO: Aquí irá el sistema de partículas en el siguiente paso
            }
        }

        // 4. RF-10: registramos el resultado. Acierto si puntuó; fallo si el sable
        //    fue incorrecto o el golpe quedó fuera del margen de ritmo.
        if (ScoreManager.Instance != null)
        {
            if (puntuo) ScoreManager.Instance.RegistrarAcierto();
            else ScoreManager.Instance.RegistrarFallo();
        }

        Destroy(gameObject);
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
