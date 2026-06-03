using System.Collections.Generic;
using UnityEngine;

public class CubeSpawnManager : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject[] Spawnpoints;
    public GameObject[] Cubeprefabs;

    [Header("Configuración de Ritmo")]
    public float approachTime = 2.221f;

    public struct NotaOsu
    {
        public int carril;
        public float tiempoGolpe;
    }

    private Queue<NotaOsu> notasQueue = new Queue<NotaOsu>();
    private bool mapaListo = false;

    void Update()
    {
        if (mapaListo && notasQueue.Count > 0)
        {
            float tiempoActual = AudioManager.instance.musicTheme.time;

            // CORRECCIÓN: El cubo debe spawnear ANTES de que llegue su tiempo de golpe real.
            // Para que aparezca antes, el tiempo actual de la canción debe superar el tiempo de golpe menos el approachTime.
            float tiempoDeSpawn = notasQueue.Peek().tiempoGolpe - approachTime;

            // Evitamos errores de desfase si el tiempo de spawn da menor o igual a cero en las primeras notas
            if (tiempoDeSpawn < 0 && tiempoActual >= 0)
            {
                Debug.LogWarning($"[CubeSpawnManager] Nota forzada a spawnear inmediatamente. Tiempo de golpe original: {notasQueue.Peek().tiempoGolpe}s. Saliendo al inicio.");
                SpawnearCubo(notasQueue.Dequeue());
                return;
            }

            if (tiempoActual >= tiempoDeSpawn)
            {
                Debug.Log($"[CubeSpawnManager] Spawneando nota de la cola. Tiempo canción: {tiempoActual:F2}s | Tiempo planeado de spawn: {tiempoDeSpawn:F2}s (Golpe a los: {notasQueue.Peek().tiempoGolpe:F2}s)");
                SpawnearCubo(notasQueue.Dequeue());
            }
        }
        else if (mapaListo && notasQueue.Count == 0)
        {
            Debug.Log("[CubeSpawnManager] Se han spawneado todas las notas del mapa. Cola vacía.");
            mapaListo = false; // Desactivamos para no saturar el Update
        }
    }

    public void InicializarMapaDesdeTexto(string textoDelMapa)
    {
        if (string.IsNullOrEmpty(textoDelMapa))
        {
            Debug.LogError("[CubeSpawnManager] ERROR: El texto entregado por GameSceneManager es nulo o está vacío.");
            return;
        }

        notasQueue.Clear();
        mapaListo = false;

        string[] lineas = textoDelMapa.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        bool leyendoHitObjects = false;
        int lineasProcesadasExitosamente = 0;

        Debug.Log($"[CubeSpawnManager] Iniciando análisis del archivo .osu. Total de líneas de texto a leer: {lineas.Length}");

        foreach (string lineaRaw in lineas)
        {
            string linea = lineaRaw.Trim();

            if (linea.Contains("[HitObjects]"))
            {
                leyendoHitObjects = true;
                Debug.Log("[CubeSpawnManager] -> Sección [HitObjects] localizada con éxito en el texto.");
                continue;
            }

            if (leyendoHitObjects && linea.StartsWith("[") && !linea.Contains("[HitObjects]"))
            {
                leyendoHitObjects = false;
                Debug.Log("[CubeSpawnManager] -> Se detectó el inicio de otra sección posterior a HitObjects. Finalizando lectura.");
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
                        lineasProcesadasExitosamente++;
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"[CubeSpawnManager] Línea inválida u omitida: '{linea}'. Error: {ex.Message}");
                    }
                }
            }
        }

        Debug.Log($"[CubeSpawnManager] ====== FIN DEL PROCESAMIENTO ======");
        Debug.Log($"[CubeSpawnManager] Notas añadidas exitosamente a la cola: {notasQueue.Count}");

        // Verificaciones críticas de seguridad antes de activar el juego
        if (Cubeprefabs == null || Cubeprefabs.Length == 0)
        {
            Debug.LogError("[CubeSpawnManager] ERROR CRÍTICO: No has asignado ningún Prefab de Cubo en el array 'Cubeprefabs' desde el Inspector de Unity.");
            return;
        }

        if (Spawnpoints == null || Spawnpoints.Length == 0)
        {
            Debug.LogError("[CubeSpawnManager] ERROR CRÍTICO: No has asignado los objetos de los carriles en el array 'Spawnpoints' desde el Inspector de Unity.");
            return;
        }

        if (notasQueue.Count > 0)
        {
            mapaListo = true;
            Debug.Log("[CubeSpawnManager] Estado 'mapaListo' cambiado a TRUE. El bucle Update comenzará a generar cubos coordinados.");
        }
        else
        {
            Debug.LogError("[CubeSpawnManager] ERROR: La cola final de notas terminó en 0. Revisa la codificación del archivo .osu.");
        }
    }

    void SpawnearCubo(NotaOsu nota)
    {
        if (Cubeprefabs.Length == 0 || Spawnpoints.Length <= nota.carril)
        {
            Debug.LogError($"[CubeSpawnManager] Abortando instanciación. Prefabs vacíos o el carril asignado ({nota.carril}) supera los Spawnpoints disponibles ({Spawnpoints.Length}).");
            return;
        }

        if (Spawnpoints[nota.carril] == null)
        {
            Debug.LogError($"[CubeSpawnManager] ERROR: El Spawnpoint en el carril {nota.carril} está asignado como NULL en el inspector.");
            return;
        }

        int indexCube = Random.Range(0, Cubeprefabs.Length);
        GameObject cube = Instantiate(Cubeprefabs[indexCube], Spawnpoints[nota.carril].transform.position, Quaternion.identity);
        cube.transform.SetParent(transform);
    }
}