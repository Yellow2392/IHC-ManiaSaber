using UnityEngine;

public class SaberCollision : MonoBehaviour
{
    [Header("Configuración de Identidad")]
    [Tooltip("0 = Sable Izquierdo (Mano A), 1 = Sable Derecho (Mano B)")]
    public int tipoSable;

    private void OnTriggerEnter(Collider other)
    {
        // Si el objeto golpeado es un cubo, le notificamos qué sable lo tocó.
        CubeHit cubo = other.GetComponent<CubeHit>();

        if (cubo != null)
        {
            cubo.ProcesarGolpe(tipoSable);
        }
    }
}
