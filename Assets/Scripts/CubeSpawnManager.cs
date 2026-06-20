using System.Collections.Generic;
using UnityEngine;
public class CubeSpawnManager : MonoBehaviour
{
    [Header("Referencias (Cubeprefabs[0] = Sable Izquierdo/A, [1] = Sable Derecho/B)")]

    public GameObject[] Spawnpoints;

    public GameObject[] Cubeprefabs;



    [Header("Configuración de Ritmo")]

    public float approachTime = 2.221f;



    [Header("Configuración de Movimiento")]

    public Vector3 hitPointOffset = new Vector3(0, 0, -22.21f);



    [Header("Silencio Inicial")]

    public float leadInDelay = 3.5f;



    public struct NotaOsu

    {

        public int carril;

        public float tiempoGolpe;

        public int tipoCubo;

    }



    private Queue<NotaOsu> notasQueue = new Queue<NotaOsu>();

    private bool mapaListo = false;



    private float cronometroInterno = 0f;

    private bool musicaIniciada = false;



    // --- NUEVAS VARIABLES PARA CONTROL DE FIN DE JUEGO ---

    private float tiempoUltimaNota = 0f;

    private bool todasLasNotasSpawneadas = false;

    [Header("Ajuste de Cierre")]
    [Tooltip("Segundos extras que se esperan después del golpe de la última nota antes de soltar los resultados.")]
    public float margenFinDeJuego = 3.0f; // <--- EL COLCHÓN DE TIEMPO

    void Start()

    {

        if (AudioManager.instance != null && AudioManager.instance.musicTheme != null)

        {

            AudioManager.instance.musicTheme.Stop();

        }

    }



    void Update()

    {

        if (mapaListo)

        {

            float tiempoAudioVirtual;



            // ---------- FASE 1: Antes de que suene la música ----------

            if (!musicaIniciada)

            {

                cronometroInterno += Time.deltaTime;

                tiempoAudioVirtual = cronometroInterno - leadInDelay;



                if (cronometroInterno >= leadInDelay)

                {

                    if (AudioManager.instance != null && AudioManager.instance.musicTheme != null)

                    {

                        AudioManager.instance.musicTheme.Play();

                    }

                    musicaIniciada = true;

                }

            }

            // ---------- FASE 2: Música en reproducción ----------

            else

            {

                tiempoAudioVirtual = AudioManager.instance.musicTheme.time;

            }



            // Spawneamos bloques mientras queden en la cola

            while (notasQueue.Count > 0)

            {

                float tiempoDeSpawn = notasQueue.Peek().tiempoGolpe - approachTime;



                if (tiempoAudioVirtual >= tiempoDeSpawn)

                {

                    SpawnearCubo(notasQueue.Dequeue());

                }

                else

                {

                    break;

                }

            }



            // Verificar si acabamos de vaciar la cola por primera vez

            if (notasQueue.Count == 0 && !todasLasNotasSpawneadas)

            {

                todasLasNotasSpawneadas = true;

                Debug.Log("[CubeSpawnManager] Todos los cubos han salido de sus spawnpoints. Esperando que terminen el trayecto...");

            }



            // ---------- FASE 3: Esperar que los últimos bloques terminen antes de finalizar ----------

            // ---------- FASE 3: Esperar que los últimos bloques terminen antes de finalizar ----------
            if (todasLasNotasSpawneadas)
            {
                // Buscamos si queda algún cubo con el script CubeMovement vivo en la escena
                // (Los cubos hijos de este manager)
                CubeMovement[] cubosRestantes = GetComponentsInChildren<CubeMovement>();

                // Si ya se spawneó todo Y ya no queda ningún cubo viajando por la pista...
                if (cubosRestantes.Length == 0)
                {
                    // Añadimos un pequeño retraso de frames o tiempo usando el colchón para que no sea abrupto
                    cronometroInterno += Time.deltaTime;

                    // Usamos el leadInDelay como un temporizador temporal para el final
                    if (cronometroInterno >= (leadInDelay + margenFinDeJuego))
                    {
                        Debug.Log("[CubeSpawnManager] ¡Nivel completado! Todos los bloques fueron jugados.");
                        mapaListo = false;

                        // AQUÍ LLAMA A TU PANEL DE RESULTADOS
                        // Ejemplo: CanvasResultados.SetActive(true);
                    }
                }
                else
                {
                    // Mientras queden cubos, reiniciamos este mini cronómetro para que empiece a contar 
                    // justo cuando desaparezca el último clon
                    cronometroInterno = leadInDelay;
                }
            }

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

        todasLasNotasSpawneadas = false; // Reiniciar flag

        tiempoUltimaNota = 0f; // Reiniciar tiempo



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



                        int indiceCarril = 0;

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



        listaTemporal.Sort((a, b) =>

        {

            int comp = a.tiempoGolpe.CompareTo(b.tiempoGolpe);

            if (comp == 0) return a.carril.CompareTo(b.carril);

            return comp;

        });



        // --- REGISTRAMOS EL TIEMPO DEL ÚLTIMO CUBO DEL MAPA ---

        if (listaTemporal.Count > 0)

        {

            tiempoUltimaNota = listaTemporal[listaTemporal.Count - 1].tiempoGolpe;

        }



        // NUEVA ASIGNACIÓN (Basada en carril: 0 y 1 = Prefab A | 2 y 3 = Prefab B)

        for (int i = 0; i < listaTemporal.Count; i++)

        {

            NotaOsu nota = listaTemporal[i];



            // 1. Asignación inicial 100% basada en el carril (Natural y cómodo para VR)

            nota.tipoCubo = (nota.carril <= 1) ? 0 : 1;



            // 2. Comprobación de Acordes (Notas simultáneas)

            bool esNotaDobleConAnterior = (i > 0 && Mathf.Abs(listaTemporal[i].tiempoGolpe - listaTemporal[i - 1].tiempoGolpe) <= 0.005f);



            if (esNotaDobleConAnterior)

            {

                NotaOsu notaAnterior = listaTemporal[i - 1];



                // Si por culpa del mapa de osu! AMBAS notas del acorde terminaron con el mismo color...

                if (nota.tipoCubo == notaAnterior.tipoCubo)

                {

                    // ... Evaluamos cuál está más a la izquierda en el espacio para decidir las manos de forma intuitiva

                    if (nota.carril > notaAnterior.carril)

                    {

                        // Si esta nota está más a la derecha que su compañera, la obligamos a ser del color Derecho (1)

                        nota.tipoCubo = 1;



                        // Y modificamos la anterior en la cola/lista para que sea el color Izquierdo (0)

                        notaAnterior.tipoCubo = 0;

                        listaTemporal[i - 1] = notaAnterior;

                    }

                    else

                    {

                        // Si esta nota es la que está más a la izquierda, ella se queda con el color Izquierdo (0)

                        nota.tipoCubo = 0;



                        // Y la anterior pasa a ser el color Derecho (1)

                        notaAnterior.tipoCubo = 1;

                        listaTemporal[i - 1] = notaAnterior;

                    }

                }

            }



            listaTemporal[i] = nota;

        }



        // Al terminar de procesar y corregir de forma segura toda la lista, la pasamos a la cola final

        foreach (NotaOsu notaLista in listaTemporal)

        {

            notasQueue.Enqueue(notaLista);

        }



        if (Cubeprefabs == null || Cubeprefabs.Length < 2 || Spawnpoints == null || Spawnpoints.Length == 0)

        {

            Debug.LogError("[CubeSpawnManager] Faltan prefabs o spawnpoints.");

            return;

        }



        if (notasQueue.Count > 0)

        {

            mapaListo = true;

            Debug.Log($"[CubeSpawnManager] Mapa cargado. {notasQueue.Count} notas. Última nota en: {tiempoUltimaNota}s.");

        }

    }



    void SpawnearCubo(NotaOsu nota)

    {

        if (nota.tipoCubo < 0 || nota.tipoCubo >= Cubeprefabs.Length || Spawnpoints.Length <= nota.carril)

            return;



        if (Spawnpoints[nota.carril] == null) return;



        GameObject cube = Instantiate(Cubeprefabs[nota.tipoCubo], Spawnpoints[nota.carril].transform.position, Quaternion.identity);

        cube.transform.SetParent(transform);



        CubeHit cubeHit = cube.GetComponent<CubeHit>();

        if (cubeHit != null)

        {

            cubeHit.tiempoGolpeExacto = nota.tiempoGolpe;

            cubeHit.tipoCuboAsignado = nota.tipoCubo;

        }



        CubeMovement cubeMovement = cube.GetComponent<CubeMovement>();

        if (cubeMovement != null)

        {

            cubeMovement.approachTime = this.approachTime;

            cubeMovement.targetPosition = Spawnpoints[nota.carril].transform.position + hitPointOffset;

        }

    }

}