using UnityEngine;

public sealed class GeneradorPiramide : MonoBehaviour
{
    public GameObject[] nivelesPiramide;
    private int indiceActual = 0;

    private int indiceColor = 0; // 0: Amarillo, 1: Rojo, 2: Verde
    private Color[] colores = { Color.yellow, Color.red, Color.green };

    public void AparecerSiguienteCubo()
    {
        if (indiceActual < nivelesPiramide.Length)
        {
            nivelesPiramide[indiceActual].SetActive(true);
            indiceActual++;
        }
    }

    // Esta función la llamará el SEGUNDO botón
    public void CambiarColoresCiclicamente()
    {
        // 1. Avanzar al siguiente color en el ciclo
        indiceColor = (indiceColor + 1) % colores.Length;
        Color nuevoColor = colores[indiceColor];

        // 2. Aplicar el color a todos los cubos que ya han aparecido
        foreach (GameObject cubo in nivelesPiramide)
        {
            if (cubo.activeSelf) // Solo a los que están visibles
            {
                Renderer rend = cubo.GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.material.color = nuevoColor;
                }
            }
        }
    }
}