using UnityEngine;

public class SlicedPart : MonoBehaviour
{
    [Header("Configuración de Fuerzas")]
    [Tooltip("Fuerza con la que los fragmentos saldrán despedidos hacia afuera. ¡Súbelo para más explosión!")]
    public float fuerzaImpulso = 12f; // Subido de 4 a 12 para un estallido inicial fuerte

    [Tooltip("Fuerza de rotación caótica que tendrán los pedazos en el aire.")]
    public float fuerzaTorque = 15f;

    [Tooltip("Multiplicador de gravedad. Valores altos hacen que los pedazos caigan como piedras.")]
    public float pesoGravedadExtra = 35f; // Fuerza hacia abajo añadida frame a frame

    [Header("Optimización")]
    [Tooltip("Tiempo en segundos antes de eliminar el fragmento. Reducido para limpieza rápida.")]
    public float tiempoDeVida = 1.2f; // Reducido de 3s a 1.2s para que desaparezcan rápido

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Generamos una dirección puramente explosiva en 360 grados
            Vector3 direccionAleatoria = new Vector3(
                Random.Range(-1f, 1f), 
                Random.Range(-0.1f, 0.8f), // Ligero ángulo hacia arriba para la expansión inicial
                Random.Range(-1f, 1f)
            ).normalized;

            rb.AddForce(direccionAleatoria * fuerzaImpulso, ForceMode.Impulse);

            Vector3 torqueAleatorio = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * fuerzaTorque;
            rb.AddTorque(torqueAleatorio, ForceMode.Impulse);
        }

        // Desaparecen mucho más rápido de la pantalla
        Destroy(gameObject, tiempoDeVida);
    }

    void FixedUpdate()
    {
        // Aplicamos una fuerza constante hacia abajo para simular una gravedad pesada y veloz
        if (rb != null)
        {
            rb.AddForce(Vector3.down * pesoGravedadExtra, ForceMode.Acceleration);
        }
    }
}