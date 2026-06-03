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

    // Ya no usamos Start() para cargar el mapa, dejamos que GameSceneManager lo active

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

    // Este nuevo método público será llamado desde el GameSceneManager
    public void InicializarMapa(TextAsset archivoMapa)
    {
        if (archivoMapa == null)
        {
            Debug.LogError("El archivo de mapa entregado es nulo.");
            return;
        }

        string[] lineas = archivoMapa.text.Split('\n');
        bool leyendoHitObjects = false;

        foreach (string linea in lineas)
        {
            if (linea.StartsWith("[HitObjects]"))
            {
                leyendoHitObjects = true;
                continue;
            }

            if (leyendoHitObjects && !string.IsNullOrWhiteSpace(linea))
            {
                string[] partes = linea.Split(',');

                if (partes.Length >= 3)
                {
                    int xPos = int.Parse(partes[0]);
                    float tiempo = float.Parse(partes[2]) / 1000f;

                    int indiceCarril = 0;
                    if (xPos > 128 && xPos <= 256) indiceCarril = 1;
                    else if (xPos > 256 && xPos <= 384) indiceCarril = 2;
                    else if (xPos > 384) indiceCarril = 3;

                    notasQueue.Enqueue(new NotaOsu { carril = indiceCarril, tiempoGolpe = tiempo });
                }
            }
        }

        Debug.Log("Total de notas cargadas en la cola: " + notasQueue.Count);
        mapaListo = true; // Activamos la bandera para que el Update empiece a trabajar
    }

    void SpawnearCubo(NotaOsu nota)
    {
        if (Cubeprefabs.Length == 0 || Spawnpoints.Length <= nota.carril) return;

        int indexCube = Random.Range(0, Cubeprefabs.Length);
        GameObject cube = Instantiate(Cubeprefabs[indexCube], Spawnpoints[nota.carril].transform.position, Quaternion.identity);
        cube.transform.SetParent(transform);
    }
}