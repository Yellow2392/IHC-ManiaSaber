using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Acceso multiplataforma a los .zip de canciones guardados en StreamingAssets.
/// En Android (y por lo tanto en el .apk de Quest) StreamingAssets vive comprimido
/// dentro del paquete y NO es accesible con File/Directory: hay que leerlo con
/// UnityWebRequest. Tampoco se puede listar ese directorio en el dispositivo, así
/// que la lista de canciones sale de un manifest.txt generado por
/// SongZipManifestBuilder en vez de escanear la carpeta.
/// </summary>
public static class SongZipLibrary
{
    private const string CarpetaRelativa = "MusicFiles/ZipFiles";
    private const string NombreManifest = "manifest.txt";

    private static string RutaCompleta(string nombreArchivo)
    {
        string ruta = Path.Combine(Application.streamingAssetsPath, CarpetaRelativa, nombreArchivo).Replace('\\', '/');

        // En Android, streamingAssetsPath ya trae el prefijo "jar:file://.../!/assets"
        // que UnityWebRequest entiende tal cual. En el resto de plataformas hace
        // falta el esquema "file://" para que se trate como URI local.
        if (Application.platform == RuntimePlatform.Android)
            return ruta;

        return "file://" + ruta;
    }

    public static IEnumerator ObtenerNombresCanciones(Action<string[]> alTerminar)
    {
        using (UnityWebRequest peticion = UnityWebRequest.Get(RutaCompleta(NombreManifest)))
        {
            yield return peticion.SendWebRequest();

            if (peticion.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"SongZipLibrary: no se pudo leer el manifest de canciones: {peticion.error}");
                alTerminar(Array.Empty<string>());
                yield break;
            }

            string[] nombres = peticion.downloadHandler.text
                .Split('\n')
                .Select(linea => linea.Trim('\r', '\n', ' '))
                .Where(linea => linea.Length > 0)
                .ToArray();

            alTerminar(nombres);
        }
    }

    public static IEnumerator ObtenerBytesDeCancion(string nombreZipSinExtension, Action<byte[]> alTerminar)
    {
        using (UnityWebRequest peticion = UnityWebRequest.Get(RutaCompleta(nombreZipSinExtension + ".zip")))
        {
            yield return peticion.SendWebRequest();

            if (peticion.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"SongZipLibrary: no se pudo leer el zip de '{nombreZipSinExtension}': {peticion.error}");
                alTerminar(null);
                yield break;
            }

            alTerminar(peticion.downloadHandler.data);
        }
    }
}
