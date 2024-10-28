using System;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.Net;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Newtonsoft.Json;
using System.Linq;

using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;


public class BoxIndexHolder : MonoBehaviour
{
    public int boxIndex; // Identifier for matching with DataItem's box_index.
}

public class DataItem
{
    public string filename { get; set; }
    public string file_location { get; set; }
    public string transcription { get; set; }
    public string journey_map { get; set; }
    public string user_name { get; set; }
    public string rotation { get; set; }
    public string box_index { get; set; }
    public string timestamp { get; set; }
}

public class UnityLoader : MonoBehaviour
{
    public GameObject prefabToSpawn;
    public string username = "Mingming";
    public string backendUrl = "https://work.manakin-gecko.ts.net:10000";
    public string audioEndpoint = "/get_objects_by_position";
    public string userEndpoint = "/get_objects_by_position_by_username";


    void Start()
    {
        Debug.Log("Starting UnityLoader");
        if (prefabToSpawn == null)
        {
            Debug.LogError("Prefab to spawn is not assigned!");
            return;
        }
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        StartCoroutine(GetObjectsByPosition());
    }

    private Vector3 ParseToVector3(string s)
    {
        string[] split = s.Trim(' ','(',')').Split(',');
        float x = float.Parse(split[0]);
        float y = float.Parse(split[1]);
        float z = float.Parse(split[2]);
        return new Vector3(x, y, z);
    }

    IEnumerator GetObjectsByPosition()
    {
        string new_string = userEndpoint + "?username="+username; 
        UnityWebRequest request = UnityWebRequest.Get(backendUrl + new_string);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
             Debug.Log(new_string);
             Debug.LogError($"Error: {request.error}");
             yield break;
        }

        var dataByPosition = JsonConvert.DeserializeObject<Dictionary<string, List<DataItem>>>(request.downloadHandler.text);

        Debug.Log("Hello there");

        foreach (var kvp in dataByPosition)
        {
            string key = kvp.Key;
            List<DataItem> dataItems = kvp.Value;

            Debug.Log($"Key: {key}");

            var vector_3 = ParseToVector3(key);

            Debug.Log("DebugTest - "+vector_3);

            GameObject new_object = Instantiate(prefabToSpawn, vector_3, Quaternion.identity);

            // add tag to new_object with the value of the key
            new_object.tag = "NewLoadedRecording";
            
            InitializeTranscriptions(new_object, dataItems, "NewLoadedRecording");
        }

        yield return null; // Ensure the coroutine completes
    }

    GameObject FindChildByName(GameObject parent, string name)
    {
        Transform[] children = parent.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child.name == name)
            {
                return child.gameObject;
            }
        }
        return null;
    }

    GameObject[] FindChildrenWithTag(GameObject parent, string tag)
    {
        Transform[] children = parent.GetComponentsInChildren<Transform>(true);
        var taggedChildren = new System.Collections.Generic.List<GameObject>();
        foreach (Transform child in children)
        {
            if (child.CompareTag(tag))
            {
                taggedChildren.Add(child.gameObject);
            }
        }
        return taggedChildren.ToArray();
    }
private List<GameObject> FindObjectsWithTagUnderParent(GameObject parent, string tag)
{
    List<GameObject> taggedObjects = new List<GameObject>();
    foreach (Transform child in parent.transform)
    {
        if (child.CompareTag(tag))
        {
            taggedObjects.Add(child.gameObject);
        }
    }
    return taggedObjects;
}

private void InitializeTranscriptions(GameObject spawnedPrefab, List<DataItem> dataItems, string key)
{
    Debug.Log($"Processing {dataItems.Count} items for prefab at position {spawnedPrefab.transform.position}");

    // Create a dictionary to store items by box_index for easier lookup
    Dictionary<string, DataItem> itemsByBoxIndex = new Dictionary<string, DataItem>();
    foreach (var item in dataItems)
    {
        itemsByBoxIndex[item.box_index] = item;
        Debug.Log($"Stored item with box_index {item.box_index}: {item.transcription}");
    }

    // Process each box index (0-8 assuming 9 boxes in journey map)
    for (int i = 0; i < 9; i++)
    {
        string boxIndex = i.ToString();
        string tagToFind = $"box_index_{i + 1}"; // Tags are 1-based (box_index_1, box_index_2, etc.)

        // Try to find the corresponding data item
        if (itemsByBoxIndex.TryGetValue(boxIndex, out DataItem dataItem))
        {
            // Find ALL components with the tag, including deeply nested ones
            GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(tagToFind);
            
            // Filter to only those that are children of our spawned prefab
            foreach (GameObject taggedObject in taggedObjects)
            {
                // Check if this object is a child of our spawned prefab
                Debug.Log($"Checking if {taggedObject.name} is a child of {spawnedPrefab.name}");
                if (taggedObject.transform.IsChildOf(spawnedPrefab.transform))
                {
                    Debug.Log($"Found object with tag {tagToFind} {taggedObject.name} called that is a child of {spawnedPrefab.name}");
                    TMP_Text[] textComponents = taggedObject.GetComponentsInChildren<TMP_Text>(true);
                    if (textComponents.Length > 0)
                    {
                        foreach (TMP_Text tmpText in textComponents)
                        {
                            tmpText.text = dataItem.transcription;
                        }
                      
                    }
                    else{
                        Debug.Log("No text components found");
                    }
                }
            }
        }
        else
        {
            Debug.Log($"No data found for box_index {boxIndex}");
        }
    }
}
        



}