using UnityEngine;
using UnityEngine.EventSystems;

public class GazeClickHandler : MonoBehaviour, IPointerClickHandler
{
    private AudioSource audioSource;

    private void Awake()
    {
        // Try to get AudioSource from the current object
        audioSource = GetComponent<AudioSource>();

        // If not found, look for AudioSource in the parent
        if (audioSource == null && transform.parent != null)
        {
            audioSource = transform.parent.GetComponent<AudioSource>();
        }

        // Log a warning if AudioSource is still not found
        if (audioSource == null)
        {
            Debug.LogWarning("AudioSource not found on this object or its parent.");
        }
    }

public void OnPointerClick(PointerEventData eventData)
{
    GameObject clickedObject = eventData.pointerPress;
    string buttonName = clickedObject != null ? clickedObject.name : "Unknown";

    Debug.Log($"WOWOWOWOW OnPointerClick - Button Name: {buttonName}");
    if (audioSource != null)
    {
        audioSource.Play();
    }
}
}
