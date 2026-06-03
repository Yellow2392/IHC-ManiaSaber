using UnityEngine;

public class CubeMovement : MonoBehaviour
{
    // Velocidad constante en metros por segundo
    public float speed = 10f;

    void Update()
    {
        // Mueve el cubo hacia el jugador (en el eje Z negativo) de forma constante
        transform.position += new Vector3(0, 0, -speed) * Time.deltaTime;
    }
}