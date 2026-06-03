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
        // Solo revisamos si el mapa ya se cargó y si hay notas en la cola
        if (mapaListo && notasQueue.Count > 0)
        {
            // Usamos el tiempo del AudioManager global para estar en perfecta sincronía
            float tiempoActual = AudioManager.instance.musicTheme.time;

            float tiempoDeSpawn = notasQueue.Peek().tiempoGolpe - approachTime;

            if (tiempoActual >= tiempoDeSpawn)
            {
                SpawnearCubo(notasQueue.Dequeue());
            }
        }
    }

    // Este método procesa el string limpio que le envía el GameSceneManager
    public void InicializarMapaDesdeTexto(string textoDelMapa)
    {
        if (string.IsNullOrEmpty(textoDelMapa))
        {
            Debug.LogError("El texto del mapa entregado está vacío o nulo.");
            return;
        }

        notasQueue.Clear();
        mapaListo = false;

        // Rompemos las líneas soportando cualquier tipo de salto de línea de Windows/Mac
        string[] lineas = textoDelMapa.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        bool leyendoHitObjects = false;

        foreach (string lineaRaw in lineas)
        {
            string linea = lineaRaw.Trim();

            // Buscamos la sección de notas usando .Contains para saltarnos caracteres invisibles de formato (BOM)
            if (linea.Contains("[HitObjects]"))
            {
                leyendoHitObjects = true;
                Debug.Log("-> [PARSER] Sección [HitObjects] localizada. Empezando a leer notas...");
                continue;
            }

            // Si detectamos que inicia otra sección diferente del archivo .osu, dejamos de leer
            if (leyendoHitObjects && linea.StartsWith("[") && !linea.Contains("[HitObjects]"))
            {
                leyendoHitObjects = false;
                break;
            }

            if (leyendoHitObjects && !string.IsNullOrWhiteSpace(linea))
            {
                if (linea.StartsWith("//")) continue; // Omitir comentarios si los hay

                string[] partes = linea.Split(',');

                if (partes.Length >= 3)
                {
                    try
                    {
                        int xPos = int.Parse(partes[0].Trim());

                        // Parseo usando InvariantCulture para evitar conflictos si la PC usa comas decimales
                        float tiempo = float.Parse(partes[2].Trim(), System.Globalization.CultureInfo.InvariantCulture) / 1000f;

                        int indiceCarril = 0;
                        if (xPos > 128 && xPos <= 256) indiceCarril = 1;
                        else if (xPos > 256 && xPos <= 384) indiceCarril = 2;
                        else if (xPos > 384) indiceCarril = 3;

                        notasQueue.Enqueue(new NotaOsu { carril = indiceCarril, tiempoGolpe = tiempo });
                    }
                    catch (System.Exception ex)
                    {
                        // Si encuentra una línea extraña como un spinner largo, no traba el juego y sigue leyendo
                        Debug.LogWarning($"Línea de nota omitida en el archivo: '{linea}'. Motivo: {ex.Message}");
                    }
                }
            }
        }

        Debug.Log("====> [RESULTADO] Total de notas cargadas con éxito en la cola: " + notasQueue.Count);

        if (notasQueue.Count > 0)
        {
            mapaListo = true; // Activamos el Update para iniciar el spawneo
        }
        else
        {
            Debug.LogError("====> [ERROR] No se pudo extraer ninguna nota. Verifica que el archivo .osu tenga la estructura correcta separada por comas.");
        }
    }

    void SpawnearCubo(NotaOsu nota)
    {
        if (Cubeprefabs.Length == 0 || Spawnpoints.Length <= nota.carril) return;

        int indexCube = Random.Range(0, Cubeprefabs.Length);
        GameObject cube = Instantiate(Cubeprefabs[indexCube], Spawnpoints[nota.carril].transform.position, Quaternion.identity);
        cube.transform.SetParent(transform);
    }
}