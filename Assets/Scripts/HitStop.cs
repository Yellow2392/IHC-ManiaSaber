using UnityEngine;

// Congela brevemente el avance de los cubos que se acercan (no el motor ni el audio),
// para darle "peso" a un golpe acertado sin desincronizar el ritmo con la música.
public static class HitStop
{
    private static float finCongelacion = -1f;

    public static void Activar(float duracionSegundos)
    {
        float fin = Time.unscaledTime + duracionSegundos;
        if (fin > finCongelacion) finCongelacion = fin;
    }

    public static float Multiplicador => Time.unscaledTime < finCongelacion ? 0f : 1f;
}
