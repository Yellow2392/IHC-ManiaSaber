using System;
using UnityEngine;

/// <summary>
/// Datos de una canción extraídos de su archivo .zip (formato osu!mania).
/// Clase de datos simple, NO es un MonoBehaviour.
/// </summary>
[Serializable]
public class SongMetadata
{
    /// <summary>Nombre del zip sin extensión. Se usa como clave de selección.</summary>
    public string nombreZip;

    /// <summary>Título de la canción (campo Title del .osu).</summary>
    public string titulo;

    /// <summary>Artista (campo Artist del .osu).</summary>
    public string artista;

    /// <summary>Dificultad (campo Version del .osu).</summary>
    public string dificultad;

    /// <summary>BPM calculado a partir del primer timing point no heredado.</summary>
    public float bpm;

    /// <summary>Duración aproximada en segundos (último hit object).</summary>
    public float duracionSegundos;

    /// <summary>Imagen de fondo del beatmap. Puede ser null si no se encontró.</summary>
    public Texture2D portada;
}
