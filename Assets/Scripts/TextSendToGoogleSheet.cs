using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;

public class GoogleSheetUploader : MonoBehaviour
{
    public TMP_InputField[] inputFields; // Array of 9 TMP_InputFields

    private string url = "https://script.google.com/macros/s/AKfycbzTX0_TzPEFBBztYSlArtUcE7kTNqfLkC17-YbUXV8baxvkM0Ew51uNuFCY1lR2ViIf8w/exec"; // 替换为你的 Web 应用 URL

    public void SubmitData()
    {
        for (int i = 0; i < inputFields.Length; i++)
        {
            StartCoroutine(PostToGoogle(inputFields[i].text, i + 1, 1)); // Each block corresponds to a different row
        }
    }

    IEnumerator PostToGoogle(string value, int row, int column)
    {
        WWWForm form = new WWWForm();
        form.AddField("value", value);
        form.AddField("row", row);
        form.AddField("column", column);

        UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error: " + www.error);
        }
        else
        {
            Debug.Log("Data successfully sent!");
        }
    }
}