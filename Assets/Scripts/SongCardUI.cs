using UnityEngine;
using UnityEngine.EventSystems; // Importante

public class SongCardUI : MonoBehaviour, IPointerClickHandler
{
    private AudioSource audioSource;
    public AudioClip previewClip;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Este método se dispara automáticamente al hacer clic/tocar el objeto
    public void OnPointerClick(PointerEventData eventData)
    {
        if (previewClip != null && audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = previewClip;
            audioSource.Play();
        }
    }
}