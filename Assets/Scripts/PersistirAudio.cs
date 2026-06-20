using UnityEngine;

public class PersistirAudio : MonoBehaviour
{
    private void Awake()
    {
        // Esto le dice a Unity que no destruya este objeto al cambiar de escena
        DontDestroyOnLoad(gameObject);
    }
}