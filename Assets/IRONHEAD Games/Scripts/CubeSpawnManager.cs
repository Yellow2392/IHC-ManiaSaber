using System.Collections.Generic;
using UnityEngine;

public class CubeSpawnManager : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject[] Spawnpoints; // Tus 4 carriles
    public GameObject[] Cubeprefabs; // Tus bloques (rojo/azul o unificados)
    public AudioSource audioSource;  // La canción que va a sonar
    public TextAsset mapaOsu;        // Tu archivo .osu (renombrado a .txt)

    [Header("Configuración de Ritmo")]
    // Cuánto tiempo (en segundos) tarda el cubo en viajar desde el Spawnpoint hasta el jugador
    public float approachTime = 2.221f;

    // Estructura para guardar cada nota
    public struct NotaOsu
    {
        public int carril;
        public float tiempoGolpe; // En segundos
    }

    // Usamos una Queue (cola) porque las notas salen en orden (FIFO: el primero en entrar es el primero en salir)
    private Queue<NotaOsu> notasQueue = new Queue<NotaOsu>();

    void Start()
    {
        CargarMapa();

        // Iniciamos la música
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }

    void Update()
    {
        // Revisamos si aún quedan notas por generar
        if (notasQueue.Count > 0)
        {
            // El tiempo actual de la canción
            float tiempoActual = audioSource.time;

            // El momento exacto en el que DEBEMOS crear el cubo para que llegue a tiempo
            float tiempoDeSpawn = notasQueue.Peek().tiempoGolpe - approachTime;

            // Si el tiempo de la canción ya alcanzó o superó el tiempo de spawn, instanciamos
            if (tiempoActual >= tiempoDeSpawn)
            {
                SpawnearCubo(notasQueue.Dequeue());
            }
        }
    }

    void CargarMapa()
    {
        if (mapaOsu == null)
        {
            Debug.LogError("No has asignado el archivo del mapa.");
            return;
        }

        // Leemos el texto línea por línea
        string[] lineas = mapaOsu.text.Split('\n');
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
                    // El archivo .osu da el tiempo en milisegundos. Lo dividimos entre 1000 para pasarlo a segundos.
                    float tiempo = float.Parse(partes[2]) / 1000f;

                    // Lógica para asignar el carril (0 a 3) basado en la posición X de Osu
                    int indiceCarril = 0;
                    if (xPos > 128 && xPos <= 256) indiceCarril = 1;
                    else if (xPos > 256 && xPos <= 384) indiceCarril = 2;
                    else if (xPos > 384) indiceCarril = 3;

                    notasQueue.Enqueue(new NotaOsu { carril = indiceCarril, tiempoGolpe = tiempo });
                }
            }
        }
        Debug.Log("Total de notas cargadas: " + notasQueue.Count);
    }

    void SpawnearCubo(NotaOsu nota)
    {
        // Elegimos un color/prefab al azar (o puedes hacerlo fijo si tus sables no distinguen color)
        int indexCube = Random.Range(0, Cubeprefabs.Length);

        // Instanciamos el cubo en el carril correspondiente
        GameObject cube = Instantiate(Cubeprefabs[indexCube], Spawnpoints[nota.carril].transform.position, Quaternion.identity);
        cube.transform.SetParent(transform);
    }
}