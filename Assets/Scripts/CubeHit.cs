using UnityEngine;

public class CubeHit : MonoBehaviour
{
    [HideInInspector] public float tiempoGolpeExacto; // Asignado por CubeSpawnManager
    [HideInInspector] public int tipoCuboAsignado;    // 0 para A, 1 para B
    [HideInInspector] public bool resuelto = false;   // Evita doble conteo (golpe vs expiración en el mismo frame)
    [HideInInspector] public float offsetSincronizacionAudio; // Mismo offset que usó el spawn para este cubo (CubeSpawnManager)

    [Header("Configuración de Golpe")]
    public float margenErrorMaximo = 0.2f;
    public int puntajeMaximo = 100;

    [Header("Efectos Visuales (Nivel 1 y 2)")]
    [Tooltip("Arrastra aquí el SwordsCube_Sliced_Prefab")]
    public GameObject prefabCuboPartido;

    [Tooltip("VFX de impacto (chispazo, destello, etc.) que se instancian en el punto de corte, alineados al tajo del sable. Se pueden arrastrar varios a la vez.")]
    public GameObject[] prefabsImpactoVFX;

    // Ahora recibimos la dirección del tajo del sable
    public void ProcesarGolpe(int tipoSableQueGolpeo, Vector3 direccionCorte)
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
            float tiempoActual = AudioManager.instance.musicTheme.time - offsetSincronizacionAudio;
            float diferencia = Mathf.Abs(tiempoGolpeExacto - tiempoActual);

            if (diferencia <= margenErrorMaximo)
            {
                CalcularPuntaje(diferencia);
                puntuo = true;

                // 2b. Hit-stop: frena brevemente el avance de ESTE cubo (no del resto)
                //     para darle peso al golpe, sin desincronizar a los demás con el audio.
                CubeMovement movimiento = GetComponent<CubeMovement>();
                if (movimiento != null) movimiento.Congelar(0.04f);

                // 3. Feedback de Audio: Reproduce el sonido de corte
                AudioSource slice = AudioManager.instance.sliceSound;
                if (slice != null && slice.clip != null)
                {
                    // PlayOneShot evita que el sonido se corte al destruir el objeto inmediatamente
                    slice.PlayOneShot(slice.clip);
                }

                // VISUAL (Nivel 2: Alineación por ángulo): orientamos los efectos
                // según la dirección real del tajo del sable.
                Quaternion rotacionCorte = Quaternion.identity;
                if (direccionCorte != Vector3.zero)
                {
                    rotacionCorte = Quaternion.LookRotation(direccionCorte);
                }

                // Cubo partido en dos mitades, con la rotación del corte exacta
                if (prefabCuboPartido != null)
                {
                    Instantiate(prefabCuboPartido, transform.position, rotacionCorte);
                }

                // VFX de impacto en el punto exacto del corte (chispazo + destello, etc.)
                if (prefabsImpactoVFX != null)
                {
                    foreach (GameObject prefabVFX in prefabsImpactoVFX)
                    {
                        if (prefabVFX == null) continue;
                        GameObject vfx = Instantiate(prefabVFX, transform.position, rotacionCorte);
                        Destroy(vfx, 2f); // Los VFX de IRONHEAD no se autodestruyen solos
                    }
                }
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

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SumarPuntos(puntosObtenidos);
        }
    }
}
