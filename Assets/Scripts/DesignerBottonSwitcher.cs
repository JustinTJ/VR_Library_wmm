using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DesignerBottonSwitcher : MonoBehaviour
{
    public Button buttonA; // Reference to ButtonA
    private int clickCount = 0; // Counter to track the number of clicks


    private void OnEnable()
    {
        buttonA.onClick.AddListener(OnButtonAClicked);
    }

    private void OnDisable()
    {
        buttonA.onClick.RemoveListener(OnButtonAClicked);
    }

    void OnButtonAClicked()
    {
        // Increment click counter
        clickCount++;

        // Check if button has been clicked twice
        if (clickCount == 2)
        {
            /*// Instantiate PrefabB and place it in the scene
            instanceB = Instantiate(prefabB, buttonA.transform.parent);
            instanceB.transform.SetSiblingIndex(buttonA.transform.GetSiblingIndex()); // Preserve the UI order
            */
            var root = buttonA.transform.parent.parent.gameObject;
            var canvas = root.transform.parent.GetComponentInChildren<ButtonReplaceManagement>().deactivatedCanvas;
            canvas.gameObject.SetActive(true);
            Destroy(root);
            // Reset click counter
            clickCount = 0;
        }
    }
}
