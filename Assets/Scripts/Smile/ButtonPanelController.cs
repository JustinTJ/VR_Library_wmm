using UnityEngine;
using UnityEngine.UI;

public class ButtonPanelController : MonoBehaviour
{
    public GameObject panel; // Panel GameObject

    void Start()
    {
        // Hide the Panel at start
        panel.SetActive(false);
    }

    public void TogglePanel()
    {
        // Toggle the visibility of the Panel
        panel.SetActive(!panel.activeSelf);
    }

    public void ClosePanel()
    {
        // Close the Panel by setting its active state to false
        panel.SetActive(false);
    }
}
