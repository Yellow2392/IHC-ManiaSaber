using UnityEngine;

public class SaberCollision : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Revisa si el objeto con el que choca tiene el tag de tu cubo o un script específico
        if (other.gameObject.CompareTag("Cube"))
        {
            // Aquí irá la lógica para instanciar las mitades cortadas
            Destroy(other.gameObject);
        }
    }
}