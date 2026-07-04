using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class SliceSoundCycler : MonoBehaviour
{
    [Header("Lista de Efectos de Sonido (en orden de ciclo)")]
    public AudioClip[] efectosDeSonido;

    [Header("Configuración de Recorte")]
    [Tooltip("Tiempo en segundos que se recortará del INICIO de cada audio para evitar desfases (ej: 0.1 para 100 milisegundos).")]
    public float tiempoRecorteInicio = 0.1f; // <--- AJUSTA ESTE VALOR

    private AudioSource miAudioSource;
    private int indiceActual = 0;

    private void Awake()
    {
        miAudioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Llama a esta función cada vez que el sable corte un cubo.
    /// </summary>
    public void ReproducirSiguienteSonido()
    {
        if (efectosDeSonido == null || efectosDeSonido.Length == 0)
        {
            Debug.LogWarning($"[{name}] No hay audio clips asignados en la lista.");
            return;
        }

        if (miAudioSource == null) return;

        // 1. Asignar el clip actual
        miAudioSource.clip = efectosDeSonido[indiceActual];

        // 2. EL TRUCO: Decirle al AudioSource que empiece más adelante del segundo 0
        // Nos aseguramos de no recortar más de lo que dura el propio audio por seguridad
        if (tiempoRecorteInicio < miAudioSource.clip.length)
        {
            miAudioSource.time = tiempoRecorteInicio;
        }
        else
        {
            miAudioSource.time = 0f;
        }

        // 3. Reproducir inmediatamente desde la nueva posición
        miAudioSource.Play();

        // 4. Avanzar de forma cíclica
        indiceActual = (indiceActual + 1) % efectosDeSonido.Length;
    }
}