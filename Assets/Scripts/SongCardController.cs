using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controla la tarjeta visual de una canción en el menú de selección.
/// Todos los campos se cablean desde el prefab; cada uno está protegido
/// contra null para que un prefab a medio configurar no lance excepciones.
/// </summary>
public class SongCardController : MonoBehaviour
{
    [Header("Portada")]
    public RawImage imagenPortada;
    public GameObject placeholderPortada;

    [Header("Textos")]
    public TMP_Text textoTitulo;
    public TMP_Text textoArtista;
    public TMP_Text textoBpm;
    public TMP_Text textoDuracion;

    [Header("Insignia de dificultad")]
    public TMP_Text textoDificultad;
    public Image fondoDificultad;

    public void Configurar(SongMetadata datos)
    {
        if (datos == null)
            return;

        if (textoTitulo != null)
        {
            textoTitulo.text = datos.titulo;
        }

        if (textoArtista != null)
        {
            textoArtista.text = string.IsNullOrEmpty(datos.artista)
                ? "Artista desconocido"
                : datos.artista;
        }

        if (textoBpm != null)
        {
            textoBpm.text = datos.bpm > 0f ? $"BPM {datos.bpm:0}" : "BPM --";
        }

        if (textoDuracion != null)
        {
            textoDuracion.text = FormatearDuracion(datos.duracionSegundos);
        }

        ConfigurarDificultad(datos.dificultad);
        ConfigurarPortada(datos.portada);
    }

    private static string FormatearDuracion(float segundos)
    {
        if (segundos <= 0f)
            return "--:--";

        int total = Mathf.FloorToInt(segundos);
        int minutos = total / 60;
        int restoSegundos = total % 60;
        return $"{minutos:00}:{restoSegundos:00}";
    }

    private void ConfigurarDificultad(string dificultad)
    {
        if (string.IsNullOrEmpty(dificultad))
        {
            if (fondoDificultad != null)
            {
                fondoDificultad.gameObject.SetActive(false);
            }
            return;
        }

        if (textoDificultad != null)
        {
            textoDificultad.text = dificultad;
        }

        if (fondoDificultad != null)
        {
            fondoDificultad.gameObject.SetActive(true);
            fondoDificultad.color = ColorPorDificultad(dificultad);
        }
    }

    private static Color ColorPorDificultad(string dificultad)
    {
        string clave = dificultad.ToLowerInvariant();

        if (clave.Contains("easy") || clave.Contains("beginner") || clave.Contains("facil") || clave.Contains("fácil"))
            return new Color(0.30f, 0.69f, 0.31f, 1f); // verde

        if (clave.Contains("normal") || clave.Contains("standard"))
            return new Color(1.00f, 0.76f, 0.03f, 1f); // ámbar

        if (clave.Contains("hard") || clave.Contains("dificil") || clave.Contains("difícil"))
            return new Color(0.96f, 0.49f, 0.00f, 1f); // naranja intenso

        if (clave.Contains("insane") || clave.Contains("expert") || clave.Contains("extreme"))
            return new Color(0.90f, 0.22f, 0.21f, 1f); // rojo

        return new Color(0.42f, 0.48f, 0.54f, 1f); // gris pizarra (desconocida)
    }

    private void ConfigurarPortada(Texture2D portada)
    {
        bool hayPortada = portada != null;

        if (imagenPortada != null)
        {
            if (hayPortada)
            {
                imagenPortada.texture = portada;
                imagenPortada.uvRect = RecorteCuadrado(portada);
            }
            imagenPortada.gameObject.SetActive(hayPortada);
        }

        if (placeholderPortada != null)
        {
            placeholderPortada.SetActive(!hayPortada);
        }
    }

    /// <summary>
    /// Las portadas de osu suelen ser 16:9 y la tarjeta las muestra en un cuadrado:
    /// recorta el centro de la textura para no deformarla.
    /// </summary>
    private static Rect RecorteCuadrado(Texture2D textura)
    {
        float ancho = textura.width;
        float alto = textura.height;

        if (ancho > alto)
        {
            float proporcion = alto / ancho;
            return new Rect((1f - proporcion) / 2f, 0f, proporcion, 1f);
        }

        if (alto > ancho)
        {
            float proporcion = ancho / alto;
            return new Rect(0f, (1f - proporcion) / 2f, 1f, proporcion);
        }

        return new Rect(0f, 0f, 1f, 1f);
    }
}
