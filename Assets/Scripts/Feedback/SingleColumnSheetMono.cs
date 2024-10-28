using System.Collections;
using System.Collections.Generic;
using SheetProcessor;
using TMPro;
using UnityEngine;

public class SingleColumnSheetMono : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField[] inputField;

    public string locationName;
    // Start is called before the first frame update
    void Start()
    {
        SheetManager.Instance.AddSheetMono(this);
        locationName = SheetManager.Instance.CurrentLocation;
    }

    public string GetRowData(int rowIndex)
    {
        var result = inputField.Length <= rowIndex ? string.Empty : inputField[rowIndex].text;
        Debug.Log(result);
        return result;
    }
}
