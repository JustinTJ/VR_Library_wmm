using UnityEngine;
using UnityEngine.UI;

public class BackToPrefabSwitch : MonoBehaviour
{
    public Button buttonA; // Reference to ButtonA
    public GameObject prefabB; // Reference to PrefabB
    public GameObject prefabC; // Reference to PrefabC

    private GameObject instanceB;
    private GameObject instanceC;

    void Start()
    {
        // Add listener to buttonA
        buttonA.onClick.AddListener(OnButtonAClicked);
    }

    void OnButtonAClicked()
    {
        // Instantiate PrefabB and place it in the scene
        instanceB = Instantiate(prefabB, buttonA.transform.parent);
        instanceB.transform.SetSiblingIndex(buttonA.transform.GetSiblingIndex()); // Preserve the UI order

        // Disable buttonA
        buttonA.gameObject.SetActive(false);

        // Add listener to the return button in PrefabB
        Button returnButtonInPrefabB = instanceB.GetComponentInChildren<Button>();
        returnButtonInPrefabB.onClick.AddListener(OnReturnButtonClicked);
    }

    void OnReturnButtonClicked()
    {
        // Instantiate PrefabC and place it in the scene
        instanceC = Instantiate(prefabC, instanceB.transform.parent);
        instanceC.transform.SetSiblingIndex(instanceB.transform.GetSiblingIndex()); // Preserve the UI order

        // Destroy PrefabB instance
        Destroy(instanceB);
    }
}
