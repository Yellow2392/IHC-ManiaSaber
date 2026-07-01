using UnityEngine;

public class CubeMovement : MonoBehaviour
{
    [HideInInspector] public float approachTime;   // lo asigna el CubeSpawnManager
    [HideInInspector] public Vector3 targetPosition; // punto de golpeo

    private Vector3 startPosition;
    private float elapsed;

    void Start()
    {
        startPosition = transform.position;
        elapsed = 0f;
    }

    void Update()
    {
        // Propio acumulador (en vez de Time.time - spawnTime) para poder frenarlo
        // brevemente con HitStop sin tocar el reloj global ni desincronizar el audio.
        elapsed += Time.deltaTime * HitStop.Multiplicador;
        float fraction = Mathf.Clamp01(elapsed / approachTime);
        transform.position = Vector3.Lerp(startPosition, targetPosition, fraction);

        // El cubo pasó el punto de golpeo sin ser golpeado: cuenta como fallo (RF-10).
        if (fraction >= 1f)
        {
            CubeHit hit = GetComponent<CubeHit>();
            if (hit == null || !hit.resuelto)   // guard: que el sable no lo haya resuelto este frame
            {
                if (hit != null) hit.resuelto = true;
                if (ScoreManager.Instance != null) ScoreManager.Instance.RegistrarFallo();
            }
            Destroy(gameObject);
        }
    }
}