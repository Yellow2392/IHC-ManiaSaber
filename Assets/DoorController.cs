using UnityEngine;

public class DoorController : MonoBehaviour
{
    public enum DoorType { SlideHorizontal, SlideVertical, Rotate }

    [Header("Configuración de la Puerta")]
    public DoorType doorType = DoorType.Rotate;
    public float speed = 3f;

    [Header("Ajustes de Movimiento")]
    // Cuánto se moverá en X, Y o Z (Ej: X=2 para horizontal, Y=3 para vertical)
    public Vector3 slideOffset;
    // Cuánto rotará en grados (Ej: Y=90 para giro normal de puerta)
    public Vector3 rotationOffset;

    private Vector3 startPos;
    private Vector3 targetPos;
    private Quaternion startRot;
    private Quaternion targetRot;
    private bool isOpen = false;

    void Start()
    {
        // Guardamos las posiciones y rotaciones iniciales
        startPos = transform.position;
        targetPos = startPos + slideOffset;

        startRot = transform.rotation;
        targetRot = startRot * Quaternion.Euler(rotationOffset);
    }

    void Update()
    {
        float step = speed * Time.deltaTime;

        if (doorType == DoorType.SlideHorizontal || doorType == DoorType.SlideVertical)
        {
            // Interpola la posición para un deslizamiento suave
            transform.position = Vector3.Lerp(transform.position, isOpen ? targetPos : startPos, step);
        }
        else if (doorType == DoorType.Rotate)
        {
            // Interpola la rotación para un giro suave
            transform.rotation = Quaternion.Lerp(transform.rotation, isOpen ? targetRot : startRot, step);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Se activa cuando el OVRPlayerController entra en el Trigger
        if (other.CompareTag("Player"))
        {
            isOpen = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Se activa cuando el jugador sale
        if (other.CompareTag("Player"))
        {
            isOpen = false;
        }
    }
}