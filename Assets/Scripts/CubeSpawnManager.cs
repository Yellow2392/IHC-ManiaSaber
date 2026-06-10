using System.Collections.Generic;
using UnityEngine;

public class CubeSpawnManager : MonoBehaviour
{
    [Header("Referencias (Cubeprefabs[0] = Sable Izquierdo/A, [1] = Sable Derecho/B)")]
    public GameObject[] Spawnpoints;      // 0: carril más izquierdo ... 3: carril más derecho
    public GameObject[] Cubeprefabs;      // Debe contener exactamente 2 prefabs (A y B)

    [Header("Configuración de Ritmo")]
    public float approachTime = 2.221f;   // Segundos que tarda un cubo en llegar al punto de golpeo

    [Header("Configuración de Movimiento")]
    [Tooltip("Offset desde el Spawnpoint hasta el punto de golpeo (ej. (0,0,-10) si los cubos avanzan hacia atrás)")]
    public Vector3 hitPointOffset = new Vector3(0, 0, -22.21f);  // Ajusta según tu escena

    [Tooltip("Silencio antes de que inicie la canción. Debe ser mayor que approachTime para que los primeros cubos se vean venir.")]
    public float leadInDelay = 3.5f;      // Tiempo de preparación antes de que empiece la música

    public struct NotaOsu
    {
        public int carril;
        public float tiempoGolpe;
        public int tipoCubo; // 0 = A (Izquierda), 1 = B (Derecha)
    }

    private Queue<NotaOsu> notasQueue = new Queue<NotaOsu>();
    private bool mapaListo = false;

    // Control del tiempo negativo antes de que arranque la canción
    private float cronometroInterno = 0f;
    private bool musicaIniciada = false;

    void Start()
    {
        // Aseguramos que la música esté detenida al iniciar
        if (AudioManager.instance != null && AudioManager.instance.musicTheme != null)
        {
            AudioManager.instance.musicTheme.Stop();
        }
    }

    void Update()
    {
        if (mapaListo && notasQueue.Count > 0)
        {
            float tiempoAudioVirtual;

            // ---------- FASE 1: Antes de que suene la música ----------
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
            // ---------- FASE 2: Música en reproducción (tiempo real del audio) ----------
            else
            {
                tiempoAudioVirtual = AudioManager.instance.musicTheme.time;
            }

            // Spawneamos todos los cubos cuyo tiempo de spawn ya haya llegado
            while (notasQueue.Count > 0)
            {
                float tiempoDeSpawn = notasQueue.Peek().tiempoGolpe - approachTime;

                if (tiempoAudioVirtual >= tiempoDeSpawn)
                {
                    SpawnearCubo(notasQueue.Dequeue());
                }
                else
                {
                    break; // La siguiente nota aún no debe salir
                }
            }
        }
        else if (mapaListo && notasQueue.Count == 0)
        {
            Debug.Log("[CubeSpawnManager] Todas las notas spawneadas.");
            mapaListo = false;
        }
    }

    public void InicializarMapaDesdeTexto(string textoDelMapa)
    {
        if (string.IsNullOrEmpty(textoDelMapa))
        {
            Debug.LogError("[CubeSpawnManager] Texto del mapa vacío.");
            return;
        }

        notasQueue.Clear();
        mapaListo = false;

        // Reiniciamos cronómetros y detenemos música (por si se reinicia el nivel)
        cronometroInterno = 0f;
        musicaIniciada = false;
        if (AudioManager.instance != null && AudioManager.instance.musicTheme != null)
        {
            AudioManager.instance.musicTheme.Stop();
        }

        string[] lineas = textoDelMapa.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        bool leyendoHitObjects = false;
        List<NotaOsu> listaTemporal = new List<NotaOsu>();

        foreach (string lineaRaw in lineas)
        {
            string linea = lineaRaw.Trim();

            if (linea.Contains("[HitObjects]"))
            {
                leyendoHitObjects = true;
                continue;
            }

            if (leyendoHitObjects && linea.StartsWith("[") && !linea.Contains("[HitObjects]"))
                break;

            if (leyendoHitObjects && !string.IsNullOrWhiteSpace(linea) && !linea.StartsWith("//"))
            {
                string[] partes = linea.Split(',');

                if (partes.Length >= 3)
                {
                    try
                    {
                        int xPos = int.Parse(partes[0].Trim());
                        float tiempo = float.Parse(partes[2].Trim(), System.Globalization.CultureInfo.InvariantCulture) / 1000f;

                        // Mapeo clásico de columnas de osu! (4 carriles)
                        int indiceCarril = 0;                         // x <= 128
                        if (xPos > 128 && xPos <= 256) indiceCarril = 1;
                        else if (xPos > 256 && xPos <= 384) indiceCarril = 2;
                        else if (xPos > 384) indiceCarril = 3;

                        listaTemporal.Add(new NotaOsu { carril = indiceCarril, tiempoGolpe = tiempo });
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[CubeSpawnManager] Línea omitida: {ex.Message}");
                    }
                }
            }
        }

        // Ordenamos por tiempo y, en caso de empate, por carril (izquierda a derecha)
        listaTemporal.Sort((a, b) =>
        {
            int comp = a.tiempoGolpe.CompareTo(b.tiempoGolpe);
            if (comp == 0) return a.carril.CompareTo(b.carril);
            return comp;
        });

        // Asignamos tipo de cubo (A/B) alternando en acordes
        for (int i = 0; i < listaTemporal.Count; i++)
        {
            NotaOsu nota = listaTemporal[i];

            // Si la nota actual está en el mismo instante que la anterior (acorde), alternamos
            if (i > 0 && Mathf.Abs(listaTemporal[i].tiempoGolpe - listaTemporal[i - 1].tiempoGolpe) <= 0.005f)
            {
                nota.tipoCubo = (listaTemporal[i - 1].tipoCubo == 0) ? 1 : 0;
            }
            else
            {
                nota.tipoCubo = 0; // Por defecto empieza con A (Izquierda)
            }

            listaTemporal[i] = nota;
            notasQueue.Enqueue(nota);
        }

        // Verificaciones finales
        if (Cubeprefabs == null || Cubeprefabs.Length < 2 || Spawnpoints == null || Spawnpoints.Length == 0)
        {
            Debug.LogError("[CubeSpawnManager] Faltan prefabs o spawnpoints.");
            return;
        }

        if (notasQueue.Count > 0)
        {
            mapaListo = true;
            Debug.Log($"[CubeSpawnManager] Mapa cargado. {notasQueue.Count} notas. Lead-in de {leadInDelay}s.");
        }
    }

    void SpawnearCubo(NotaOsu nota)
    {
        // Validaciones
        if (nota.tipoCubo < 0 || nota.tipoCubo >= Cubeprefabs.Length || Spawnpoints.Length <= nota.carril)
            return;

        if (Spawnpoints[nota.carril] == null) return;

        // Instanciar el prefab correcto (A o B)
        GameObject cube = Instantiate(Cubeprefabs[nota.tipoCubo], Spawnpoints[nota.carril].transform.position, Quaternion.identity);
        cube.transform.SetParent(transform);

        // 1. Asignar datos al script CubeHit (para el golpeo y puntuación)
        CubeHit cubeHit = cube.GetComponent<CubeHit>();
        if (cubeHit != null)
        {
            cubeHit.tiempoGolpeExacto = nota.tiempoGolpe;
            cubeHit.tipoCuboAsignado = nota.tipoCubo;
        }
        else
        {
            Debug.LogWarning("[CubeSpawnManager] El prefab no tiene script CubeHit.");
        }

        // 2. Asignar datos al script CubeMovement (para movimiento sincronizado)
        CubeMovement cubeMovement = cube.GetComponent<CubeMovement>();
        if (cubeMovement != null)
        {
            cubeMovement.approachTime = this.approachTime;
            // Calcula la posición exacta donde debe estar en el momento del golpe
            cubeMovement.targetPosition = Spawnpoints[nota.carril].transform.position + hitPointOffset;
        }
        else
        {
            Debug.LogWarning("[CubeSpawnManager] El prefab no tiene script CubeMovement.");
        }
    }
}