using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.Net;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SheetProcessor;
using System.Linq;
using System.Text;

[RequireComponent(typeof(Collider))]
public class UIManager : MonoBehaviour
{
    public BoxCollider[] planeColliders;
    public GameObject mainPanel;
    public GameObject[] inputPanels;
    public Button[] uiButtons;
    public Button[] backButtons;
    public Button[] recordButtons;
    public Button[] playButtons;
    public Button[] addButtons;
    public Button[] deleteButton;
    public TextMeshProUGUI[] textValues;
    public TMP_InputField[] inputFields;
    private TMP_InputField selectedInputField;
    private bool[] recording;
    private AudioClip recordedClip;
    private string area = "DefaultLocation";
    private GameObject transform_org;

    
    public GameObject inputFieldPrefab;
    public Transform inputFieldContainer;
    private int recordButtonClickCount = 0;

    private string current_zone;

    private void PlayToPanel(int index)
    {
        Debug.Log("Play button pressed on panel " + index);
    }

    private void DeleteToPanel(int index)
    {
        Debug.Log("Delete button pressed on panel " + index);
    }


    private float checkTimer;
    public float checkInterval = 0.5f;
    private GameObject player;
    public string playerTag = "Player";


    public string backendUrl = "https://work.manakin-gecko.ts.net:10000";
    public string audioEndpoint = "/uploadaudio_cplx";
    public string outputFileName = "recorded_audio.wav";
    public string username = "Mingming";
    public string journey = "Journey pt 1";
    private string currentLocation = "DefaultLocation";

    // ... (keep all existing variables)

    [Header("Zone Detection")]
    public string zoneTag = "Zone";
    public List<ZoneInfo> zones = new List<ZoneInfo>();
    public TextMeshProUGUI currentZoneText;


    [System.Serializable]
    public class ZoneInfo
    {
        public string zoneName;
        public Collider zoneCollider;
    }

    private class PersistentData
    {
        public string username;
        public string journey;
        public string currentLocation;
        public Vector3 position;
        public Quaternion rotation;
        public List<string> transcriptions = new List<string>();
    }

    private PersistentData persistentData;
    private string previousLocation;
    


    void Start()
    {
        InitializeRecordingArray();
        SetInitialPanelStates();
        SetupButtonListeners();
        UpdateZoneDisplay();
       // LoadPersistentData();


        // Find the player GameObject
        player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null)
        {
            Debug.LogError("Player GameObject not found. Make sure it has the correct tag.");
        }


        // Ensure this GameObject has a Collider component
        Collider collider = GetComponent<Collider>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider>();
            Debug.Log("Added BoxCollider to UIManager GameObject");
        }
        collider.isTrigger = true;
        Debug.Log("UIManager Collider set as trigger");

        Debug.Log("Current Locaiton is " + area);
        GetColliderAtPosition(transform.position);
        Debug.Log("Prefab Object location is at " + area);

        transform_org = new GameObject();
        transform_org.transform.SetPositionAndRotation(transform.position, transform.rotation);


        foreach (var inputField in inputFields)
        {
        inputField.onSelect.AddListener((string _) => OnInputFieldSelected(inputField));
        }
        
        UpdateUIWithLoadedData();
    }

    void Update()
    {

    }

    private void LoadPersistentData()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "persistentData.json");
        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            persistentData = JsonUtility.FromJson<PersistentData>(jsonData);
        }
        else
        {
            persistentData = new PersistentData
            {
                username = "Mingming",
                journey = "Journey pt 1",
                currentLocation = "DefaultLocation",
                position = Vector3.zero,
                rotation = Quaternion.identity
            };
        }

        // Update script variables with loaded data
        username = persistentData.username;
        journey = persistentData.journey;
        CurrentLocation = persistentData.currentLocation;
        transform.position = persistentData.position;
        transform.rotation = persistentData.rotation;
    }

    private void SavePersistentData()
    {
        persistentData.username = username;
        persistentData.journey = journey;
        persistentData.currentLocation = CurrentLocation;
        persistentData.position = transform.position;
        persistentData.rotation = transform.rotation;

        // Save transcriptions
        persistentData.transcriptions.Clear();
        foreach (var textValue in textValues)
        {
            persistentData.transcriptions.Add(textValue.text);
        }

        string jsonData = JsonUtility.ToJson(persistentData);
        string filePath = Path.Combine(Application.persistentDataPath, "persistentData.json");
        File.WriteAllText(filePath, jsonData);
    }

    private void UpdateUIWithLoadedData()
    {
        // Update text values with loaded transcriptions
        // for (int i = 0; i < textValues.Length && i < persistentData.transcriptions.Count; i++)
        // {
        //     textValues[i].text = persistentData.transcriptions[i];
        // }

        // // Update other UI elements as needed
        // if (currentZoneText != null)
        // {
        //     currentZoneText.text = $"Current Zone: {CurrentLocation}";
        // }
    }

    public void ExportTranscriptionsToExcel()
    {
        StartCoroutine(FetchAndExportTranscriptions());
    }

    private IEnumerator FetchAndExportTranscriptions()
    {
        string fetchUrl = "https://work.manakin-gecko.ts.net:10000/view_transcriptions";
        UnityWebRequest www = UnityWebRequest.Get(fetchUrl);

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Failed to fetch transcriptions: {www.error}");
            yield break;
        }

        string jsonResponse = www.downloadHandler.text;
        List<TranscriptionData> transcriptions = JsonUtility.FromJson<TranscriptionWrapper>("{\"items\":" + jsonResponse + "}").items;

        string csvContent = GenerateCsvContent(transcriptions);
        
        string filePath = Path.Combine(Application.persistentDataPath, "transcriptions.csv");
        File.WriteAllText(filePath, csvContent);
        
        Debug.Log($"Transcriptions exported to CSV at: {filePath}");

        // Optionally, you can open the file or provide a way for the user to access it
        // Application.OpenURL("file://" + filePath);
    }

    private string GenerateCsvContent(List<TranscriptionData> transcriptions)
    {
        var locations = transcriptions.Select(t => t.location).Distinct().OrderBy(l => l).ToList();
        var boxIndices = Enumerable.Range(0, 9).ToList();

        StringBuilder csvBuilder = new StringBuilder();

        // Header row
        csvBuilder.Append("Location,");
        csvBuilder.AppendLine(string.Join(",", boxIndices.Select(i => $"Box {i}")));

        // Data rows
        foreach (var location in locations)
        {
            csvBuilder.Append($"{EscapeCsvField(location)},");
            foreach (var index in boxIndices)
            {
                var transcription = transcriptions.FirstOrDefault(t => t.location == location && t.box_index == index);
                string content = transcription != null ? EscapeCsvField(transcription.transcription) : "";
                csvBuilder.Append(content);
                if (index < 8) csvBuilder.Append(",");
            }
            csvBuilder.AppendLine();
        }

        return csvBuilder.ToString();
    }

    private string EscapeCsvField(string field)
    {
        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
        {
            return "\"" + field.Replace("\"", "\"\"") + "\"";
        }
        return field;
    }

    [System.Serializable]
    private class TranscriptionWrapper
    {
        public List<TranscriptionData> items;
    }

    [System.Serializable]
    private class TranscriptionData
    {
        public string location;
        public int box_index;
        public string transcription;
    }




    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CurrentLocation = gameObject.name;
            Debug.Log($"Player entered zone: {CurrentLocation}");
            UpdateZoneDisplay();
            SendLocationToBackend();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CurrentLocation = "OutsideZone";
            Debug.Log($"Player exited zone. Current location: {CurrentLocation}");
            UpdateZoneDisplay();
            SendLocationToBackend();
        }
    }




    private void UpdateZoneDisplay()
    {
        if (currentZoneText != null)
        {
            currentZoneText.text = $"Current Zone: {CurrentLocation}";
        }
    }


    // Update the CurrentLocation property
    public string CurrentLocation
    {
        get { return currentLocation; }
        set
        {
            if (currentLocation != value)
            {
                previousLocation = currentLocation;
                currentLocation = value;
                Debug.Log($"Location changed from {previousLocation} to {currentLocation}");
                OnLocationChanged();
            }
        }
    }





    public TextMeshProUGUI errorText;

    private void OnLocationChanged()
    {
        // You can add any additional logic here that should occur when the location changes
        // For example, updating UI elements or sending data to the backend
        SendLocationToBackend();
    }

    private void SendLocationToBackend()
    {
        // This method will be called whenever the location changes
        // You can implement the logic to send the new location to your backend here
        Debug.Log($"Sending location change to backend: {previousLocation} -> {CurrentLocation}");
        // Implement your backend communication here
        // For example, you could call a coroutine to send a web request
        // StartCoroutine(SendLocationData());
    }


    private IEnumerator SendLocationData()
    {
        string uploadURL = backendUrl + "/update_location"; // Adjust this endpoint as needed

        WWWForm form = new WWWForm();
        form.AddField("user_name", username);
        form.AddField("journey_map", journey);
        form.AddField("previous_location", previousLocation);
        form.AddField("location", CurrentLocation);

        UnityWebRequest www = UnityWebRequest.Post(uploadURL, form);
        Debug.Log($"Sending location update to: {uploadURL}");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Failed to send location data: {www.error}");
            Debug.LogError($"Response Code: {www.responseCode}");
            Debug.LogError($"Full URL: {uploadURL}");
            Debug.LogError($"Request Body: user_name={username}, journey_map={journey}, previous_location={previousLocation}, location={CurrentLocation}");

            if (errorText != null)
            {
                errorText.text = $"Failed to update location. Error: {www.error}";
                errorText.gameObject.SetActive(true);
            }
        }
        else
        {
            Debug.Log("Location data sent successfully");
            Debug.Log($"Response: {www.downloadHandler.text}");

            if (errorText != null)
            {
                errorText.gameObject.SetActive(false);
            }
        }
    }



    private void InitializeRecordingArray()
    {
        recording = new bool[9];
        for (int i = 0; i < recording.Length; i++)
        {
            recording[i] = false;
        }
    }

    private void SetInitialPanelStates()
    {
        mainPanel.SetActive(true);
        foreach (var panel in inputPanels)
        {
            panel.SetActive(false);
        }
    }



    // Moved button setup to a separate method for clarity
    private void SetupButtonListeners()
    {
        // Setup button listeners for the 9 UI elements
        for (int i = 0; i < uiButtons.Length; i++)
        {
            int index = i; // Capture the cu[]rrent index
            uiButtons[i].onClick.AddListener(() => OpenInputPanel(index));
        }

        // Setup listeners for back buttons
        for (int i = 0; i < backButtons.Length; i++)
        {
            int index = i; // Capture the current index
            backButtons[i].onClick.AddListener(() => BackToMainPanel(index));
        }

        for (int i = 0; i < recordButtons.Length; i++)
        {
            int index = i; // Capture the current index
            recordButtons[i].onClick.AddListener(() => RecordToPanel(index));
        }

        for (int i = 0; i < playButtons.Length; i++)
        {
            int index = i; // Capture the current index
            playButtons[i].onClick.AddListener(() => PlayToPanel(index));
        }

        for (int i = 0; i < addButtons.Length; i++)
        {
            int index = i; // Capture the current index
            addButtons[i].onClick.AddListener(() => AddToPanel(index));
        }

        // Add listeners to input fields
        //foreach (var inputField in inputFields)
        //{
        //    inputField.onSelect.AddListener((string _) => OnInputFieldSelected(inputField));
        //}
        
        foreach (var inputField in inputFields)
        {
            if (inputField != null)
            {
                //inputField.onSelect.AddListener((string _) => OnInputFieldSelected(inputField));
                AddPointerClickListener(inputField);
                Debug.Log("Listener attached to: " + inputField.name);
            }
            else
            {
                Debug.LogWarning("Input field is null, listener not attached.");
            }
        }


        for (int i = 0; i < deleteButton.Length; i++)
        {
            int index = i;
            deleteButton[i].onClick.AddListener(DeleteSelectedInputField);
        }

    
        // Add listener for add button
        if (addButtons != null && addButtons.Length > 0)
        {
            for (int i = 0; i < addButtons.Length; i++)
            {
                if (addButtons[i] != null)
                {
                    addButtons[i].onClick.AddListener(AddNewInputField);
                }
            }
        }
        else
        {
            Debug.LogError("Add button not assigned in the Inspector.");
        }
    }

    private void AddPointerClickListener(TMP_InputField inputField)
    {
        EventTrigger trigger = inputField.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((eventData) => { OnInputFieldSelected(inputField); });

        trigger.triggers.Add(entry);
    }

    // Existing methods remain unchanged...
    //private void OnInputFieldSelected(TMP_InputField inputField)
    //{
        // Ŀǰ�÷���û��������
        //selectedInputField = inputField;
        //Debug.Log("Selected input field: " + inputField.name);
        //Debug.Log("Selected input field: " + (selectedInputField != null ? selectedInputField.name : "null"));
    //}

    public void ClearSelectedField()
    {
        if (selectedInputField != null)
        {
            selectedInputField.text = string.Empty;
            Debug.Log("Cleared selected input field: " + selectedInputField.name);
        }
        else
        {
            Debug.LogWarning("No input field is currently selected.");
        }
    }


    public void OpenInputPanel(int index)
    {
        mainPanel.SetActive(false);
        inputPanels[index].SetActive(true);
    }

    public void AddToPanel(int index)
    {
        TextMeshProUGUI transcription = textValues[index].GetComponent<TextMeshProUGUI>();

        Transform parent = transcription.transform.parent;

        GameObject obj = new GameObject("Some  text");
        obj.transform.SetParent(parent, false);
        TextMeshProUGUI newText = obj.AddComponent<TextMeshProUGUI>();
    }

    public void BackToMainPanel(int index)
    {
        // SheetManager.Instance.SaveCsv();
        inputPanels[index].SetActive(false);
        mainPanel.SetActive(true);
        // SavePersistentData(); // Save data when returning to main panel
    }

    public void RecordToPanel(int index)
    {
        if (index < inputFields.Length)
        {
            Debug.Log("Record To Panel Pressed on " + index);
            // If we're not recording, we need to decide whether to use existing field or create new one
            if (!recording[index])
            {
                // If the current input field is not empty, create a new one
                TMP_InputField currentField = GetCurrentInputFieldForPanel(index);
                if (currentField != null && !string.IsNullOrEmpty(currentField.text))
                {
                    AddNewInputFieldToPanel(index);
                }
            }

            ToggleRecording(index);
        }
        else
        {
            Debug.LogError("Invalid index for RecordToPanel: " + index);
        }
    }

    private TMP_InputField GetCurrentInputFieldForPanel(int panelIndex)
    {
        // Get all input fields in the current panel
        TMP_InputField[] panelInputFields = inputPanels[panelIndex].GetComponentsInChildren<TMP_InputField>();
        return panelInputFields.LastOrDefault();
    }

    private void AddNewInputFieldToPanel(int panelIndex)
    {
        if (inputFieldPrefab != null)
        {   
            GameObject newInputFieldObj = Instantiate(inputFieldPrefab, inputFieldContainer);
            TMP_InputField newInputField = newInputFieldObj.GetComponent<TMP_InputField>();

            
            if (newInputField != null)
            {
                
                newInputField.onSelect.AddListener((string _) => OnInputFieldSelected(newInputField));
                
                // Add this new input field to our tracking arrays if needed
                List<TMP_InputField> inputFieldList = new List<TMP_InputField>(inputFields);
                inputFieldList.Add(newInputField);
                inputFields = inputFieldList.ToArray();

                selectedInputField = newInputField;
                
                Debug.Log("New input field added to panel " + selectedInputField);
            }
            

            else
            {
                Debug.LogError("InputField component not found on the instantiated prefab.");
            }
        }
        else
        {
            Debug.LogError("InputField prefab or container not assigned in the Inspector.");
        }
    }



    // New method for Delete functionality
    public void DeleteSelectedInputField()
    {
        if (selectedInputField != null)
        {
            Debug.Log($"Deleting input field: {selectedInputField.name}");
            Destroy(selectedInputField.gameObject);
            selectedInputField = null;
        }
        else
        {
            Debug.LogWarning("No input field selected for deletion.");
        }
    }

    // New method for Add functionality
    public void AddNewInputField()
    {
        if (inputFieldPrefab != null && inputFieldContainer != null)
        {
            GameObject newInputFieldObj = Instantiate(inputFieldPrefab, inputFieldContainer);
            TMP_InputField newInputField = newInputFieldObj.GetComponent<TMP_InputField>();
            
            if (newInputField != null)
            {

                newInputField.onSelect.AddListener((string _) => OnInputFieldSelected(newInputField));
                
                // Update the inputFields array
                List<TMP_InputField> inputFieldList = new List<TMP_InputField>(inputFields);
                inputFieldList.Add(newInputField);
                inputFields = inputFieldList.ToArray();

                selectedInputField = newInputField;

                Debug.Log("New input field added successfully.New inputField is " + selectedInputField.name); // ��һ��û����ʾselectedInputField.name������
            }
            else
            {
                Debug.LogError("InputField component not found on the instantiated prefab.");
            }
        }
        else
        {
            Debug.LogError("InputField prefab or container not assigned in the Inspector.");
        }
    }


    // Method to handle record button click
    public void RecordButtonClicked(string transcription)
    {
        recordButtonClickCount++;

        if (recordButtonClickCount == 1 && selectedInputField != null)
        {
            // Update the current input field with the transcription
            selectedInputField.text = transcription;
        }
        else
        {
            // Create a new input field and update it with the transcription
            AddNewInputField();
            if (selectedInputField != null)
            {
                selectedInputField.text = transcription;
            }
        }
    }

    // Method to handle input field selection
    private void OnInputFieldSelected(TMP_InputField inputField)
    {
        selectedInputField = inputField;
        Debug.Log("Selected input field: " + (selectedInputField != null ? selectedInputField.name : "null"));
    }


    public class DataItem
    {
        public string box_index;
        public string transcription;
    }

    //private void AddNewRecordButton(Transform inputFieldTransform)
    //{
        //if (recordButtons.Length > 0)
        //{
            //Button newRecordButton = Instantiate(recordButtons[0], inputFieldTransform);
            //newRecordButton.onClick.RemoveAllListeners();
            //int newIndex = inputFields.Length - 1;
            //newRecordButton.onClick.AddListener(() => RecordToPanel(newIndex));
        
            // Resize the recordButtons array
            //System.Array.Resize(ref recordButtons, recordButtons.Length + 1);
            //recordButtons[recordButtons.Length - 1] = newRecordButton;
        
            // Resize the recording array
            //System.Array.Resize(ref recording, recording.Length + 1);
            //recording[recording.Length - 1] = false;
        //}
        //else
        //{
            //Debug.LogError("No record button prefab available.");
        //}
    //}


    private void ToggleRecording(int index)
    {
        if (recording[index] == false)
        {
            Debug.Log("Start Recording");
            StartRecording(index);
        }
        else
        {
            Debug.Log("Stop Recording");
            StopRecording(index);
        }
    }

    private void StartRecording(int index)
    {
        recording[index] = true;
        recordedClip = Microphone.Start(null, false, 120, 44100);

        //here change the button "record button " -> "recording" --> may be change the color the button.
        recordButtons[index].GetComponentInChildren<TextMeshProUGUI>().text = "Recording";
        recordButtons[index].GetComponent<Image>().color = Color.yellow;

        Debug.Log("Recording started.");
    }
    void GetColliderAtPosition(Vector3 position)
    {
        bool exit = false;
        foreach (BoxCollider collider in planeColliders)
        {
            if (exit)
            {
                break;
            }

            int layermask = 1 << collider.gameObject.layer;

            RaycastHit[] hits = Physics.RaycastAll(position, Vector3.down, Mathf.Infinity, layermask);

            if (hits.Length > 0)
            {
                foreach (RaycastHit hit in hits)
                {
                    Collider hitCollider = hit.collider;
                    Debug.Log($"Hit collider: {hitCollider.name} at position {hit.point}");

                    if (hitCollider.name.Contains("Plane"))
                    {
                        Debug.Log("Remove this one");
                    }


                    exit = true;
                    area = $"{hitCollider.name}";
                    break;
                }
            }
            else
            {
                Debug.Log("No Colliders");
            }
        }



        void GetColliderAtPosition(Vector3 position)
        {
            // Create a layermask that includes everything except the "Ignore Raycast" layer
            int layermask = ~(1 << LayerMask.NameToLayer("Ignore Raycast"));

            RaycastHit[] hits = Physics.RaycastAll(position, Vector3.down, Mathf.Infinity, layermask);

            foreach (RaycastHit hit in hits)
            {
                Collider hitCollider = hit.collider;
                Debug.Log($"Hit collider: {hitCollider.name} at position {hit.point}");
                // Process the collider here
                return; // Exit after finding the first collider
            }

        }

    }
    private void StopRecording(int index)
    {
        recording[index] = false;
        Microphone.End(null);
        SaveAudioFile();
        SendAudioToBackend(index);
        // send Vector3 postion and rotation to backend


        // Get the current input field for this panel
        TMP_InputField currentField = GetCurrentInputFieldForPanel(index);
        if (currentField != null)
        {
            selectedInputField = currentField; // Ensure the transcription goes to this field
        }
    
        SendAudioToBackend(index);

        // change back the color and text of the button to record. 
        recordButtons[index].GetComponentInChildren<TextMeshProUGUI>().text = "Record";
        recordButtons[index].GetComponent<Image>().color = Color.white;
    }


    private void SaveAudioFile()
    {
        string filePath = Path.Combine(Application.persistentDataPath, outputFileName);
        SavWav.Save(filePath, recordedClip);
        Debug.Log("Audio saved as: " + filePath);
    }


    private void SendAudioToBackend(int index)
    {
        Debug.Log($"Sending to backend from currentLocation: {CurrentLocation}");
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        string uploadURL = "https://work.manakin-gecko.ts.net:10000/uploadaudio_cplx";  // replace with your FastAPI server URL
        string filePath = Path.Combine(Application.persistentDataPath, outputFileName);
        StartCoroutine(UploadAudioFile(filePath, uploadURL, index));
    }

    public IEnumerator UploadAudioFile(string filePath, string uploadURL, int index)
    {
        Debug.Log("System is uploading file");
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        byte[] audioBytes = File.ReadAllBytes(filePath);
        formData.Add(new MultipartFormFileSection("audiofile", audioBytes, filePath, "audio/wav"));
        formData.Add(new MultipartFormDataSection("location", area));
        formData.Add(new MultipartFormDataSection("user_name", username));
        formData.Add(new MultipartFormDataSection("journey_map", journey));
        formData.Add(new MultipartFormDataSection("position", transform_org.transform.position.ToString()));
        formData.Add(new MultipartFormDataSection("rotation", transform_org.transform.rotation.ToString()));
        formData.Add(new MultipartFormDataSection("box_index", index.ToString()));

        UnityWebRequest www = UnityWebRequest.Post(uploadURL, formData);

        Debug.Log("Sent request");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error" + www.error);
            Debug.LogError("Upload failed: " + www.error);
        }
        else
        {
            TranscriptionResponse response = JsonUtility.FromJson<TranscriptionResponse>(www.downloadHandler.text);

            Debug.Log("Upload complete! Response: " + www.downloadHandler.text);

            if (response != null && !string.IsNullOrEmpty(response.transcription))
            {
                Debug.Log(textValues[index]);
                TextMeshProUGUI transcription = textValues[index].GetComponent<TextMeshProUGUI>();
                transcription.text += (string.IsNullOrEmpty(transcription.text) ? "" : "\n") + response.transcription;
                // SavePersistentData(); // Save data after updating transcription
                // get previous text string text = transcription.text 
                // transcription.text = transcription.text + "\n" +response.transcription
                Debug.Log(textValues[index].text);
                Debug.Log("Transcription set: " + response.transcription);
            }
            else
            {
                Debug.Log("Invalid");
                Debug.LogError("Invalid or empty transcription response.");
            }
        }
    }

    private void OnDestroy()
    {
        // SavePersistentData();
    }


}