using UnityEngine;

public class SaberCollision : MonoBehaviour
{
    [Header("Configuración de Identidad")]
    [Tooltip("0 = Sable Izquierdo (Mano A), 1 = Sable Derecho (Mano B)")]
    public int tipoSable;

    private Vector3 posicionFrameAnterior;
    private Vector3 direccionCorteActual;

    void Start()
    {
        posicionFrameAnterior = transform.position;
    }

    void Update()
    {
        // Calculamos la dirección del movimiento en este frame
        Vector3 movimientoEsteFrame = transform.position - posicionFrameAnterior;

        // Si la mano se movió, actualizamos el vector de dirección del corte
        if (movimientoEsteFrame.sqrMagnitude > 0.0001f)
        {
            direccionCorteActual = movimientoEsteFrame.normalized;
        }

        posicionFrameAnterior = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si el objeto golpeado es un cubo, le notificamos qué sable lo tocó.
        CubeHit cubo = other.GetComponent<CubeHit>();

        if (cubo != null)
        {
            // Le pasamos el ID del sable Y la dirección en la que viajaba la mano
            cubo.ProcesarGolpe(tipoSable, direccionCorteActual);
        }
    }
}
