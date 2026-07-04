using UnityEngine;

public class CubeMovement : MonoBehaviour
{
    [HideInInspector] public float approachTime;   // lo asigna el CubeSpawnManager
    [HideInInspector] public Vector3 targetPosition; // punto de golpeo

    private Vector3 startPosition;
    private float elapsed;
    private float congeladoHasta = -1f; // Time.unscaledTime hasta el cual ESTE cubo no avanza

    void Start()
    {
        startPosition = transform.position;
        elapsed = 0f;
    }

    // Congela brevemente el avance de este cubo (p.ej. al ser golpeado) sin afectar
    // al resto de cubos en vuelo ni al reloj de audio.
    public void Congelar(float duracionSegundos)
    {
        float fin = Time.unscaledTime + duracionSegundos;
        if (fin > congeladoHasta) congeladoHasta = fin;
    }

    void Update()
    {
        if (Time.unscaledTime >= congeladoHasta)
            elapsed += Time.deltaTime;

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