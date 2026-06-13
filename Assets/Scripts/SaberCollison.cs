using UnityEngine;

public class SaberCollision : MonoBehaviour
{
    [Header("Configuración de Identidad")]
    [Tooltip("0 = Sable Izquierdo (Mano A), 1 = Sable Derecho (Mano B)")]
    public int tipoSable; 

    private void OnTriggerEnter(Collider other)   
    {
        // Esto imprimirá un mensaje CUALQUIER cosa que toque el sable
        Debug.Log($"[Sable] Toqué algo llamado: {other.gameObject.name}"); 
        // Buscamos de forma general si el objeto golpeado tiene la identidad de un CuboHit
        CubeHit cubo = other.GetComponent<CubeHit>();

        if (cubo != null)
        {
            // Le notificamos al cubo que fue golpeado y le pasamos qué sable lo tocó
            cubo.ProcesarGolpe(tipoSable);
        }
    }
}