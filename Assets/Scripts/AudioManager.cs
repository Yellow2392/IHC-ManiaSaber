using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource sliceSound;
    public AudioSource gunSound;
    public AudioSource musicTheme;
    public AudioSource buttonClickSound;

    public static AudioManager instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this.gameObject); // Asegura que persista entre menús y el GameScene
    }
}