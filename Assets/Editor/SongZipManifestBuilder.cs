using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// En el dispositivo (Android/Quest) los archivos de StreamingAssets viven comprimidos
/// dentro del .apk y no se pueden listar con Directory.GetFiles. Por eso el juego
/// necesita un manifest.txt con los nombres de las canciones; este script lo mantiene
/// sincronizado con el contenido real de la carpeta ZipFiles.
/// </summary>
public class SongZipManifestBuilder : IPreprocessBuildWithReport
{
    private const string CarpetaZip = "Assets/StreamingAssets/MusicFiles/ZipFiles";
    private const string NombreManifest = "manifest.txt";

    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        Regenerar();
    }

    [MenuItem("IHC/Regenerar manifest de canciones")]
    public static void Regenerar()
    {
        if (!Directory.Exists(CarpetaZip))
        {
            Debug.LogError($"SongZipManifestBuilder: no existe la carpeta '{CarpetaZip}'.");
            return;
        }

        string[] nombres = Directory.GetFiles(CarpetaZip, "*.zip")
            .Select(ruta => Path.GetFileNameWithoutExtension(ruta))
            .OrderBy(nombre => nombre)
            .ToArray();

        string rutaManifest = Path.Combine(CarpetaZip, NombreManifest);
        File.WriteAllText(rutaManifest, string.Join("\n", nombres));

        AssetDatabase.ImportAsset(rutaManifest);
        Debug.Log($"SongZipManifestBuilder: manifest regenerado con {nombres.Length} canciones.");
    }
}
