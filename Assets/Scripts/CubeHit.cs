using UnityEngine;

public class CubeHit : MonoBehaviour
{
    [HideInInspector] public float tiempoGolpeExacto; 
    [HideInInspector] public int tipoCuboAsignado;    

    [Header("Configuración de Golpe")]
    public float margenErrorMaximo = 0.2f; 
    public int puntajeMaximo = 100;

    [Header("Efectos Visuales (Nivel 1 y 2)")]
    [Tooltip("Arrastra aquí el SwordsCube_Sliced_Prefab")]
    public GameObject prefabCuboPartido;

    // Ahora recibimos la dirección del tajo del sable
    public void ProcesarGolpe(int tipoSableQueGolpeo, Vector3 direccionCorte)
    {
        if (tipoSableQueGolpeo == tipoCuboAsignado)
        {
            float tiempoActual = AudioManager.instance.musicTheme.time;
            float diferencia = Mathf.Abs(tiempoGolpeExacto - tiempoActual);

            if (diferencia <= margenErrorMaximo)
            {
                CalcularPuntaje(diferencia);
                
                // AUDIO
                if (AudioManager.instance != null && AudioManager.instance.sliceSound != null)
                {
                    AudioManager.instance.sliceSound.PlayOneShot(AudioManager.instance.sliceSound.clip);
                }

                // VISUAL (Nivel 2: Alineación por ángulo)
                if (prefabCuboPartido != null)
                {
                    // Calculamos una rotación orientada hacia la dirección del corte del sable
                    Quaternion rotacionCorte = Quaternion.identity;
                    if (direccionCorte != Vector3.zero)
                    {
                        rotacionCorte = Quaternion.LookRotation(direccionCorte);
                    }

                    // Instanciamos el objeto partido con la rotación del corte exacta
                    Instantiate(prefabCuboPartido, transform.position, rotacionCorte);
                }
                
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

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SumarPuntos(puntosObtenidos);
        }
    }
}