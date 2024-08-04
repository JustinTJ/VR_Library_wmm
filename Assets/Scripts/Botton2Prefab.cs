using UnityEngine;
using UnityEngine.UI;

public class ButtonToPrefabSwitch : MonoBehaviour
{
    public Button buttonA; // Reference to ButtonA
    public GameObject prefabB; // Reference to PrefabB

    private GameObject instanceB;
    private int clickCount = 0; // Counter to track the number of clicks

    void Start()
    {
        // Add listener to buttonA
        buttonA.onClick.AddListener(OnButtonAClicked);
    }

    void OnButtonAClicked()
    {
        // Increment click counter
        clickCount++;

        // Check if button has been clicked twice
        if (clickCount == 2)
        {
            // Instantiate PrefabB and place it in the scene
            instanceB = Instantiate(prefabB, buttonA.transform.parent);
            instanceB.transform.SetSiblingIndex(buttonA.transform.GetSiblingIndex()); // Preserve the UI order

            // Disable buttonA
            buttonA.gameObject.SetActive(false);

            // Reset click counter
            clickCount = 0;
        }
    }
}