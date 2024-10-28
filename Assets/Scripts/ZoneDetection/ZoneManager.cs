using UnityEngine;

public class ZoneManager : MonoBehaviour
{
    public UIManager uiManager;
    public string zoneName;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            uiManager.CurrentLocation = zoneName;
        }
    }
}