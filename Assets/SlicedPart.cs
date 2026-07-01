using UnityEngine;

public class SlicedPart : MonoBehaviour
{
    [Header("Dirección del Corte")]
    [Tooltip("Dirección local en la que esta mitad sale despedida. Al instanciar el prefab, CubeHit ya rota todo el objeto según el tajo real del sable, así que esta dirección queda alineada al plano de corte automáticamente.")]
    public Vector3 direccionFuerzaLocal = Vector3.right;

    [Tooltip("Dispersión aleatoria (en grados) sobre la dirección del corte, para que cada mitad no salga siempre exactamente igual.")]
    public float dispersionAngulo = 20f;

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
            // Partimos de la dirección del corte (heredada vía rotación del prefab padre) y le
            // sumamos una dispersión leve para que no se vea siempre idéntico.
            Quaternion dispersion = Quaternion.Euler(
                Random.Range(-dispersionAngulo, dispersionAngulo),
                Random.Range(-dispersionAngulo, dispersionAngulo),
                Random.Range(-dispersionAngulo, dispersionAngulo)
            );
            Vector3 direccionCorte = transform.TransformDirection((dispersion * direccionFuerzaLocal).normalized);

            rb.AddForce(direccionCorte * fuerzaImpulso, ForceMode.Impulse);

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