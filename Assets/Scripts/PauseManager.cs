using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestiona la pausa de la partida (RF-12) y las opciones del menú de pausa
/// (RF-13: continuar / reiniciar / volver al menú).
///
/// La pausa congela DOS relojes independientes:
///   1. Time.timeScale = 0  → detiene CubeMovement (usa Time.time) y el contador
///      de GameSceneManager (WaitForSeconds).
///   2. musicTheme.Pause()  → detiene el reloj de audio (musicTheme.time), del que
///      dependen el spawn de cubos, la barra de progreso y la ventana de golpe.
/// Sin pausar el audio, esos sistemas seguirían avanzando aunque timeScale = 0.
///
/// La entrada se lee por botones del control (OVRInput) en Update(), que sigue
/// corriendo con timeScale = 0. No se usan botones físicos ni raycaster de UI
/// porque la física (FixedUpdate) se detiene durante la pausa.
/// </summary>
public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    [Header("UI")]
    [Tooltip("Canvas world-space del menú de pausa (UI_Pause). Arranca inactivo.")]
    public GameObject panelPausa;

    [Header("Configuración de Escena")]
    public string escenaMenu = "MenuSongs";
    public string escenaJuego = "GameScene";

    private bool estaPausado = false;
    // Se activa al terminar la partida para no pausar sobre la pantalla de resultados.
    private bool pausaBloqueada = false;

    void Awake()
    {
        // Mismo patrón singleton por-escena que ScoreManager.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Update()
    {
        if (pausaBloqueada) return;

        // Botón Menú (☰) del control izquierdo: alterna pausa/reanuda.
        if (OVRInput.GetDown(OVRInput.Button.Start))
        {
            AlternarPausa();
        }

        // Opciones del menú de pausa (solo cuando está pausado).
        if (estaPausado)
        {
            if (OVRInput.GetDown(OVRInput.RawButton.A)) Continuar();        // A (control der.)
            else if (OVRInput.GetDown(OVRInput.RawButton.B)) Reiniciar();   // B (control der.)
            else if (OVRInput.GetDown(OVRInput.RawButton.X)) VolverAlMenu(); // X (control izq.)
        }
    }

    public void AlternarPausa()
    {
        if (estaPausado) Continuar();
        else Pausar();
    }

    public void Pausar()
    {
        Time.timeScale = 0f;

        if (AudioManager.instance != null && AudioManager.instance.musicTheme != null)
        {
            AudioManager.instance.musicTheme.Pause();
        }

        if (panelPausa != null) panelPausa.SetActive(true);
        estaPausado = true;
    }

    // Públicos para poder cablearse también a Button.onClick en el futuro.

    public void Continuar()
    {
        Time.timeScale = 1f;

        if (AudioManager.instance != null && AudioManager.instance.musicTheme != null)
        {
            AudioManager.instance.musicTheme.UnPause();
        }

        if (panelPausa != null) panelPausa.SetActive(false);
        estaPausado = false;
    }

    public void Reiniciar()
    {
        // Imprescindible restaurar timeScale ANTES de cargar: si no, la escena
        // nueva arrancaría congelada.
        Time.timeScale = 1f;

        if (AudioManager.instance != null && AudioManager.instance.musicTheme != null)
        {
            AudioManager.instance.musicTheme.Stop();
        }

        // SongMenuManager.CancionSeleccionada (estático) se conserva → recarga la misma canción.
        SceneManager.LoadScene(escenaJuego);
    }

    public void VolverAlMenu()
    {
        Time.timeScale = 1f;

        if (AudioManager.instance != null && AudioManager.instance.musicTheme != null)
        {
            AudioManager.instance.musicTheme.Stop();
        }

        SceneManager.LoadScene(escenaMenu);
    }

    /// <summary>
    /// Lo llama GameSceneManager.GameOver() para impedir pausar una vez mostrada
    /// la pantalla de resultados. Si quedó pausado, restaura el estado por seguridad.
    /// </summary>
    public void BloquearPausa()
    {
        pausaBloqueada = true;

        if (estaPausado)
        {
            Time.timeScale = 1f;
            if (panelPausa != null) panelPausa.SetActive(false);
            estaPausado = false;
        }
    }

    void OnDestroy()
    {
        // Red de seguridad: un timeScale congelado nunca debe filtrarse a la siguiente escena.
        Time.timeScale = 1f;
    }
}
