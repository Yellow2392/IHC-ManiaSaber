using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;

public static class OsuZipReader
{
    // Ahora recibe un Stream (flujo de memoria) en lugar de una ruta de archivo string
    public static SongMetadata LeerMetadata(Stream zipStream, string nombreZip)
    {
        SongMetadata datos = new SongMetadata
        {
            nombreZip = nombreZip,
            titulo = nombreZip,
            artista = string.Empty,
            dificultad = string.Empty,
            bpm = 0f,
            duracionSegundos = 0f,
            portada = null
        };

        try
        {
            // Abrimos el zip directamente desde el flujo de datos en memoria
            using (ZipArchive zip = new ZipArchive(zipStream, ZipArchiveMode.Read))
            {
                ZipArchiveEntry entradaOsu = null;
                ZipArchiveEntry respaldoMasPequeno = null;
                long menorPeso = long.MaxValue;

                foreach (ZipArchiveEntry entrada in zip.Entries)
                {
                    if (EsEntradaDeDirectorio(entrada))
                        continue;

                    if (!entrada.Name.EndsWith(".osu", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (entrada.Name.ToUpperInvariant().Contains("[BEGINNER]"))
                    {
                        entradaOsu = entrada;
                        break;
                    }

                    if (entrada.Length > 0 && entrada.Length < menorPeso)
                    {
                        menorPeso = entrada.Length;
                        respaldoMasPequeno = entrada;
                    }
                }

                if (entradaOsu == null)
                    entradaOsu = respaldoMasPequeno;

                if (entradaOsu == null)
                {
                    Debug.LogWarning($"OsuZipReader: no se encontró ningún archivo .osu dentro de '{nombreZip}'.");
                    return datos;
                }

                string titulo = null;
                string tituloUnicode = null;
                string artista = null;
                string artistaUnicode = null;
                string version = null;
                string archivoFondo = null;
                float bpm = 0f;
                int maxMs = 0;
                string seccion = string.Empty;

                using (StreamReader lector = new StreamReader(entradaOsu.Open(), Encoding.UTF8))
                {
                    string linea;
                    while ((linea = lector.ReadLine()) != null)
                    {
                        linea = linea.Trim();
                        if (linea.Length == 0) continue;

                        if (linea[0] == '[' && linea[linea.Length - 1] == ']')
                        {
                            seccion = linea;
                            continue;
                        }

                        switch (seccion)
                        {
                            case "[Metadata]":
                                {
                                    int indiceDosPuntos = linea.IndexOf(':');
                                    if (indiceDosPuntos < 0) break;

                                    string clave = linea.Substring(0, indiceDosPuntos).Trim();
                                    string valor = linea.Substring(indiceDosPuntos + 1).Trim();

                                    switch (clave)
                                    {
                                        case "Title": titulo = valor; break;
                                        case "TitleUnicode": tituloUnicode = valor; break;
                                        case "Artist": artista = valor; break;
                                        case "ArtistUnicode": artistaUnicode = valor; break;
                                        case "Version": version = valor; break;
                                    }
                                    break;
                                }

                            case "[TimingPoints]":
                                {
                                    if (bpm > 0f) break;

                                    string[] campos = linea.Split(',');
                                    if (campos.Length < 2) break;

                                    if (float.TryParse(campos[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float beatLength)
                                        && beatLength > 0f)
                                    {
                                        bpm = 60000f / beatLength;
                                    }
                                    break;
                                }

                            case "[Events]":
                                {
                                    if (archivoFondo != null || linea.StartsWith("//")) break;

                                    if (linea.StartsWith("0,0,"))
                                    {
                                        int primeraComilla = linea.IndexOf('"');
                                        if (primeraComilla >= 0)
                                        {
                                            int segundaComilla = linea.IndexOf('"', primeraComilla + 1);
                                            if (segundaComilla > primeraComilla)
                                            {
                                                archivoFondo = linea
                                                    .Substring(primeraComilla + 1, segundaComilla - primeraComilla - 1)
                                                    .Replace('\\', '/')
                                                    .Trim();
                                            }
                                        }
                                    }
                                    break;
                                }

                            case "[HitObjects]":
                                {
                                    string[] campos = linea.Split(',');
                                    if (campos.Length < 4) break;

                                    if (int.TryParse(campos[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int tiempo)
                                        && tiempo > maxMs)
                                    {
                                        maxMs = tiempo;
                                    }

                                    if (int.TryParse(campos[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out int tipo)
                                        && (tipo & 128) != 0
                                        && campos.Length >= 6)
                                    {
                                        string parametros = campos[5];
                                        int indiceDosPuntos = parametros.IndexOf(':');
                                        string textoFin = indiceDosPuntos >= 0 ? parametros.Substring(0, indiceDosPuntos) : parametros;

                                        if (int.TryParse(textoFin, NumberStyles.Integer, CultureInfo.InvariantCulture, out int tiempoFin)
                                            && tiempoFin > maxMs)
                                        {
                                            maxMs = tiempoFin;
                                        }
                                    }
                                    break;
                                }
                        }
                    }
                }

                datos.titulo = PrimeroNoVacio(titulo, tituloUnicode, nombreZip);
                datos.artista = PrimeroNoVacio(artista, artistaUnicode, string.Empty);
                datos.dificultad = version ?? string.Empty;
                datos.bpm = bpm;
                datos.duracionSegundos = maxMs / 1000f;

                if (!string.IsNullOrEmpty(archivoFondo))
                {
                    ZipArchiveEntry entradaImagen = BuscarEntradaPorNombre(zip, archivoFondo);
                    if (entradaImagen != null)
                    {
                        byte[] bytes = LeerBytes(entradaImagen);
                        Texture2D textura = new Texture2D(2, 2);
                        if (ImageConversion.LoadImage(textura, bytes))
                        {
                            datos.portada = textura;
                        }
                        else
                        {
                            UnityEngine.Object.Destroy(textura);
                        }
                    }
                }
            }
        }
        catch (Exception excepcion)
        {
            Debug.LogWarning($"OsuZipReader: error leyendo virtualmente '{nombreZip}': {excepcion.Message}");
        }

        return datos;
    }

    private static bool EsEntradaDeDirectorio(ZipArchiveEntry entrada)
    {
        return string.IsNullOrEmpty(entrada.Name)
            || entrada.FullName.EndsWith("/")
            || entrada.FullName.EndsWith("\\");
    }

    private static ZipArchiveEntry BuscarEntradaPorNombre(ZipArchive zip, string nombre)
    {
        string objetivo = nombre.Replace('\\', '/');
        foreach (ZipArchiveEntry entrada in zip.Entries)
        {
            if (EsEntradaDeDirectorio(entrada)) continue;
            string rutaCompleta = entrada.FullName.Replace('\\', '/');

            if (rutaCompleta.Equals(objetivo, StringComparison.OrdinalIgnoreCase) ||
                rutaCompleta.EndsWith("/" + objetivo, StringComparison.OrdinalIgnoreCase))
                return entrada;
        }
        return null;
    }

    private static byte[] LeerBytes(ZipArchiveEntry entrada)
    {
        using (Stream flujo = entrada.Open())
        using (MemoryStream memoria = new MemoryStream())
        {
            flujo.CopyTo(memoria);
            return memoria.ToArray();
        }
    }

    private static string PrimeroNoVacio(string primero, string segundo, string respaldo)
    {
        if (!string.IsNullOrEmpty(primero)) return primero;
        if (!string.IsNullOrEmpty(segundo)) return segundo;
        return respaldo;
    }
}