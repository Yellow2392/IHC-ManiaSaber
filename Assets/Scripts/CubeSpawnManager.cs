using System.Collections.Generic;
using UnityEngine;

public class CubeSpawnManager : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject[] Spawnpoints;
    public GameObject[] Cubeprefabs;

    [Header("Configuración de Ritmo")]
    public float approachTime = 3.118f;

    [Tooltip("Debe ser mayor al approachTime. Es el silencio antes de que inicie la canción.")]
    public float leadInDelay = 3.5f; // NUEVO: Tiempo de preparación

    public struct NotaOsu
    {
        public int carril;
        public float tiempoGolpe;
    }

    private Queue<NotaOsu> notasQueue = new Queue<NotaOsu>();
    private bool mapaListo = false;

    // NUEVO: Variables para manejar el tiempo negativo
    private float cronometroInterno = 0f;
    private bool musicaIniciada = false;

    void Start()
    {
        if (AudioManager.instance != null && AudioManager.instance.musicTheme != null) { 
            AudioManager.instance.musicTheme.Stop();
        }
    }

    void Update()
    {
        if (mapaListo && notasQueue.Count > 0)
        {
            float tiempoAudioVirtual;

            // FASE 1: Antes de que suene la música (El tiempo es "negativo" para los primeros cubos)
            if (!musicaIniciada)
            {
                cronometroInterno += Time.deltaTime;
                tiempoAudioVirtual = cronometroInterno - leadInDelay;

                // Cuando el cronómetro cubre el silencio inicial, damos Play a la música
                if (cronometroInterno >= leadInDelay)
                {
                    AudioManager.instance.musicTheme.Play();
                    musicaIniciada = true;
                }
            }
            // FASE 2: La música está sonando. Usamos el tiempo real del audio para no desincronizarnos nunca.
            else
            {
                tiempoAudioVirtual = AudioManager.instance.musicTheme.time;
            }

            // CORRECCIÓN CRÍTICA: Usamos WHILE en lugar de IF. 
            // Si hay un "Acorde" (2 notas o más en el mismo exacto milisegundo), 
            // el while las spawnea todas de golpe en el mismo frame.
            while (notasQueue.Count > 0)
            {
                float tiempoDeSpawn = notasQueue.Peek().tiempoGolpe - approachTime;

                if (tiempoAudioVirtual >= tiempoDeSpawn)
                {
                    SpawnearCubo(notasQueue.Dequeue());
                }
                else
                {
                    // Si la nota en la cima aún no debe salir, rompemos el ciclo hasta el próximo frame
                    break;
                }
            }
        }
        else if (mapaListo && notasQueue.Count == 0)
        {
            Debug.Log("[CubeSpawnManager] Se han spawneado todas las notas del mapa. Cola vacía.");
            mapaListo = false;
        }
    }

    public void InicializarMapaDesdeTexto(string textoDelMapa)
    {
        if (string.IsNullOrEmpty(textoDelMapa))
        {
            Debug.LogError("[CubeSpawnManager] ERROR: El texto entregado es nulo.");
            return;
        }

        notasQueue.Clear();
        mapaListo = false;

        // Reiniciamos los cronómetros por si se está reiniciando el nivel
        cronometroInterno = 0f;
        musicaIniciada = false;
        if (AudioManager.instance.musicTheme != null && AudioManager.instance.musicTheme.isPlaying)
        {
            AudioManager.instance.musicTheme.Stop();
        }

        string[] lineas = textoDelMapa.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        bool leyendoHitObjects = false;

        foreach (string lineaRaw in lineas)
        {
            string linea = lineaRaw.Trim();

            if (linea.Contains("[HitObjects]"))
            {
                leyendoHitObjects = true;
                continue;
            }

            if (leyendoHitObjects && linea.StartsWith("[") && !linea.Contains("[HitObjects]"))
            {
                break;
            }

            if (leyendoHitObjects && !string.IsNullOrWhiteSpace(linea))
            {
                if (linea.StartsWith("//")) continue;

                string[] partes = linea.Split(',');

                if (partes.Length >= 3)
                {
                    try
                    {
                        int xPos = int.Parse(partes[0].Trim());
                        float tiempo = float.Parse(partes[2].Trim(), System.Globalization.CultureInfo.InvariantCulture) / 1000f;

                        int indiceCarril = 0;
                        if (xPos > 128 && xPos <= 256) indiceCarril = 1;
                        else if (xPos > 256 && xPos <= 384) indiceCarril = 2;
                        else if (xPos > 384) indiceCarril = 3;

                        notasQueue.Enqueue(new NotaOsu { carril = indiceCarril, tiempoGolpe = tiempo });
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[CubeSpawnManager] Línea omitida. Error: {ex.Message}");
                    }
                }
            }
        }

        // Verificaciones críticas
        if (Cubeprefabs == null || Cubeprefabs.Length == 0 || Spawnpoints == null || Spawnpoints.Length == 0)
        {
            Debug.LogError("[CubeSpawnManager] ERROR CRÍTICO: Faltan Prefabs o Spawnpoints.");
            return;
        }

        if (notasQueue.Count > 0)
        {
            mapaListo = true;
            Debug.Log("[CubeSpawnManager] Mapa inicializado correctamente. Empieza el Lead-in.");
        }
    }

    void SpawnearCubo(NotaOsu nota)
    {
        if (Cubeprefabs.Length == 0 || Spawnpoints.Length <= nota.carril || Spawnpoints[nota.carril] == null) return;

        int indexCube = Random.Range(0, Cubeprefabs.Length);
        GameObject cube = Instantiate(Cubeprefabs[indexCube], Spawnpoints[nota.carril].transform.position, Quaternion.identity);
        cube.transform.SetParent(transform);
    }
}
