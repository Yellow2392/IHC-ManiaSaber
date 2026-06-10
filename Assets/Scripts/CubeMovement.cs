using UnityEngine;

public class CubeMovement : MonoBehaviour
{
    [HideInInspector] public float approachTime;   // lo asigna el CubeSpawnManager
    [HideInInspector] public Vector3 targetPosition; // punto de golpeo

    private Vector3 startPosition;
    private float spawnTime;

    void Start()
    {
        startPosition = transform.position;
        spawnTime = Time.time;
    }

    void Update()
    {
        float elapsed = Time.time - spawnTime;
        float fraction = Mathf.Clamp01(elapsed / approachTime);
        transform.position = Vector3.Lerp(startPosition, targetPosition, fraction);

        // Opcional: destruir si ya pasó el punto de golpeo y no fue golpeado
        if (fraction >= 1f)
        {
            // Aquí podrías llamar a un fallo o simplemente destruir
            Destroy(gameObject);
        }
    }
}