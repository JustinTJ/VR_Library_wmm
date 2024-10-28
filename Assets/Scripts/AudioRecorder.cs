using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.Net;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[System.Serializable]
public class TranscriptionResponse
{
    public string info;
    public string transcription;
}

public class AudioRecorder : MonoBehaviour
{
    public string backendUrl = "https://work.manakin-gecko.ts.net:8443/";
    public string outputFileName = "recorded_audio.wav";
    public GameObject recordingIndicatorPrefab; // Prefab for the recording indicator

    private AudioClip recordedClip;
    private bool isRecording = false;
    private GameObject recordingIndicatorInstance;

    public TextMeshProUGUI transcriptionText;

    public int numberOfButtons = 8;
    private GameObject radialMenuInstance;
    public GameObject radialMenuPrefab;

    public string username = "Mingming";
    public string location = "Zone 5";
    public string journey = "Journey pt 1";

    private Material defaultButtonMaterial;
    private Material highlightedButtonMaterial;
    private int currentHighlightedButton = -1;


    private void Start()
    {
        // Request microphone permission
        if (Microphone.devices.Length > 0)
        {
            Debug.Log("Microphone found.");
        }
        else
        {
            Debug.LogError("No microphone found!");
        }

        defaultButtonMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        defaultButtonMaterial.color = Color.white;

        highlightedButtonMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        highlightedButtonMaterial.color = Color.yellow;
        
        Debug.Log($"Default material color: {defaultButtonMaterial.color}");
        Debug.Log($"Highlighted material color: {highlightedButtonMaterial.color}");
        
    }

    private void Update()
    {
        // Check for trigger press
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            Debug.Log("Trigger Pressed");
            ToggleRecording();
        }

        if (OVRInput.GetDown(OVRInput.Button.Two)){
            Debug.Log("Button Two Pressed");
            ShowRadialMenu();
        }
        if (OVRInput.GetUp(OVRInput.Button.Two)){
            Debug.Log("Button Two Released");
            HideRadialMenu();
        }

    if (radialMenuInstance != null && radialMenuInstance.activeSelf)
    {
        UpdateRadialMenuHighlight();
    }
        // Debug.Log("Index Trigger");
        // Debug.Log(OVRInput.Axis1D.SecondaryIndexTrigger);
    }

    private void ToggleRecording()
    {
        if (!isRecording)
        {
            Debug.Log("Start Recording");
            StartRecording();
        }
        else
        {
            Debug.Log("Stop Recording");
            StopRecording();
        }
    }

private void StartRecording()
{
    isRecording = true;
    recordedClip = Microphone.Start(null, false, 10, 44100);
    Debug.Log("Recording started.");

    if (recordingIndicatorPrefab != null)
    {
        OVRCameraRig cameraRig = FindObjectOfType<OVRCameraRig>();
        if (cameraRig != null)
        {
            Transform rightControllerTransform = cameraRig.rightHandAnchor;
            recordingIndicatorInstance = Instantiate(recordingIndicatorPrefab, rightControllerTransform.position, rightControllerTransform.rotation);
            recordingIndicatorInstance.transform.SetParent(rightControllerTransform, false);

            // Position the entire indicator relative to the controller
            recordingIndicatorInstance.transform.localPosition = new Vector3(0, 0.05f, 0.1f);
            recordingIndicatorInstance.transform.localRotation = Quaternion.identity;

            // Find the IndicatorSphere
            Transform indicatorSphere = recordingIndicatorInstance.transform.Find("IndicatorSphere");
            // Inside StartRecording method
            if (indicatorSphere != null)
            {
                // Reset IndicatorSphere position to origin
                indicatorSphere.localPosition = Vector3.zero;
                indicatorSphere.localRotation = Quaternion.identity;

                // Change the color of the sphere to red
                Renderer sphereRenderer = indicatorSphere.GetComponent<Renderer>();
                if (sphereRenderer != null)
                {
                    sphereRenderer.material.color = Color.red;
                }

                // Adjust positions of other elements relative to IndicatorSphere
                AdjustElementPositions(indicatorSphere);
            }
            else
            {
                Debug.LogError("IndicatorSphere not found in the prefab.");
            }
        }
        else
        {
            Debug.LogError("OVRCameraRig not found in the scene.");
        }
    }
    else
    {
        Debug.LogError("Recording Indicator Prefab is not assigned.");
    }
}

private void AdjustElementPositions(Transform indicatorSphere)
{
    float offset = 0.05f; // Adjust this value to change the spacing between elements

    // Adjust AudioSource
    Transform audioSource = recordingIndicatorInstance.transform.Find("AudioSource");
    if (audioSource != null)
    {
        // audioSource.SetParent(indicatorSphere);
        audioSource.localPosition = Vector3.zero;
        audioSource.localRotation = Quaternion.identity;
    }
    else{
        Debug.LogError("AudioSource not found in the recording indicator prefab.");
    }

    // Adjust Button (on the left)
    Transform button = recordingIndicatorInstance.transform.Find("Button");
    if (button != null)
    {
        button.SetParent(indicatorSphere);
        button.localPosition = new Vector3(-offset, 0, 0);
        button.localRotation = Quaternion.identity;
    }

    // Adjust Canvas and TranscriptionText (on the right)
    Transform canvas = recordingIndicatorInstance.transform.Find("Canvas");
    if (canvas != null)
    {
        canvas.SetParent(indicatorSphere);
        canvas.localPosition = new Vector3(offset, 0, 0);
        canvas.localRotation = Quaternion.identity;

        Transform transcriptionText = canvas.Find("TranscriptionText");
        if (transcriptionText != null)
        {
            transcriptionText.localPosition = Vector3.zero;
        }
    }

    // Adjust PlayButton (at the bottom)
    Transform playButton = recordingIndicatorInstance.transform.Find("PlayButton");
    if (playButton != null)
    {
        playButton.SetParent(indicatorSphere);
        playButton.localPosition = new Vector3(0, -offset, 0);
        playButton.localRotation = Quaternion.identity;
        
        // Adjust the scale to make it wider
        playButton.localScale = new Vector3(2f, 1f, 1f); // Adjust these values as needed
    }
}

private void StopRecording()
{
    if (isRecording)
    {
        isRecording = false;
        Microphone.End(null);
        Debug.Log("Recording stopped.");

        if (recordingIndicatorInstance != null)
        {
            // Detach the recording indicator from the controller
            recordingIndicatorInstance.transform.SetParent(null);

            // Keep its current world position and rotation
            Vector3 currentPosition = recordingIndicatorInstance.transform.position;
            Quaternion currentRotation = recordingIndicatorInstance.transform.rotation;
            recordingIndicatorInstance.transform.SetPositionAndRotation(currentPosition, currentRotation);

            // Change the color of the sphere to blue
            Renderer sphereRenderer = recordingIndicatorInstance.transform.Find("IndicatorSphere").GetComponent<Renderer>();
            if (sphereRenderer != null)
            {
                sphereRenderer.material.color = Color.blue;
            }

            // Set up play button
            Button playButton = recordingIndicatorInstance.GetComponentInChildren<Button>();
            if (playButton != null)
            {
                playButton.gameObject.SetActive(true);
                playButton.onClick.RemoveAllListeners();
                playButton.onClick.AddListener(PlayRecordedAudio);

                TextMeshProUGUI buttonText = playButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = "Play";
                }
                else
                {
                    Debug.LogError("TextMeshProUGUI component not found on PlayButton.");
                }
                Debug.Log("Play button set up correctly.");
            }
            else
            {
                Debug.LogError("Button component not found in recordingIndicatorInstance.");
            }

            // Set up AudioSource
            AudioSource prefabAudioSource = recordingIndicatorInstance.GetComponentInChildren<AudioSource>();

            if (prefabAudioSource != null)
            {
                prefabAudioSource.clip = recordedClip;
                Debug.Log("AudioSource set up correctly.");
            }
            else
            {
                Debug.LogError("AudioSource not found in the recording indicator prefab.");
            }
        }
        else
        {
            Debug.LogError("Recording Indicator Instance is null.");
        }

        SaveAudioFile();
        SendAudioToBackend(recordingIndicatorInstance.transform.position, recordingIndicatorInstance.transform.rotation);
        // send Vector3 postion and rotation to backend

    }
}


private void ShowRadialMenu()
{
    if (radialMenuInstance == null)
    {
        OVRCameraRig cameraRig = FindObjectOfType<OVRCameraRig>();
        if (cameraRig == null)
        {
            Debug.LogError("OVRCameraRig not found in the scene.");
            return;
        }

        // Change this to left controller
        Transform controllerTransform = cameraRig.leftHandAnchor;

        // Instantiate the radial menu at the controller's position and rotation
        radialMenuInstance = Instantiate(radialMenuPrefab, controllerTransform.position, controllerTransform.rotation);

        radialMenuInstance.transform.SetParent(controllerTransform, true);
        CreateRadialButtons();
    }
    radialMenuInstance.SetActive(true);
    PositionRadialMenu();
}

    private void HideRadialMenu()
    {
        if (radialMenuInstance != null)
        {
            radialMenuInstance.SetActive(false);
        }
    }

    private void PositionRadialMenu()
    {
        if (radialMenuInstance != null)
        {
            // Position the menu slightly in front of and above the controller
            radialMenuInstance.transform.localPosition = new Vector3(0, 0.1f, 0.15f);
            radialMenuInstance.transform.localRotation = Quaternion.Euler(30, 0, 0);
        }
    }

private void CreateRadialButtons()
{
    OVRCameraRig cameraRig = FindObjectOfType<OVRCameraRig>();
    if (cameraRig == null)
    {
        Debug.LogError("OVRCameraRig not found in the scene.");
        return;
    }

    Transform rightHandAnchor = cameraRig.rightHandAnchor;
    float angleStep = 360f / numberOfButtons;
    float radius = 0.1f; // Adjust this value to change the size of the radial sphere

    for (int i = 0; i < numberOfButtons; i++)
    {
        float angle = i * angleStep;
        Vector3 buttonPosition = new Vector3(
            Mathf.Sin(angle * Mathf.Deg2Rad) * radius,
            0.05f, // Slight offset above the hand
            Mathf.Cos(angle * Mathf.Deg2Rad) * radius
        );

            GameObject buttonObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Renderer buttonRenderer = buttonObj.GetComponent<Renderer>();
            if (buttonRenderer != null)
            {
                buttonRenderer.material = new Material(defaultButtonMaterial);
                Debug.Log($"Button {i} material color: {buttonRenderer.material.color}");
            }
            else
            {
                Debug.LogError($"Renderer component not found on button {i}");
            }
        buttonObj.name = $"Button_{i}";
        buttonObj.transform.SetParent(radialMenuInstance.transform, false);
        buttonObj.transform.localPosition = buttonPosition;
        buttonObj.transform.localScale = Vector3.one * 0.02f; // Adjust button size as needed

        // Add a collider for interaction
        SphereCollider collider = buttonObj.GetComponent<SphereCollider>();
        if (collider == null)
        {
            collider = buttonObj.AddComponent<SphereCollider>();
        }

        // Add text to the button
        GameObject textObj = new GameObject("ButtonText");
        textObj.transform.SetParent(buttonObj.transform, false);
        textObj.transform.localPosition = Vector3.zero;
        textObj.transform.localRotation = Quaternion.identity;

        TextMeshPro tmpText = textObj.AddComponent<TextMeshPro>();
        tmpText.text = (i + 1).ToString();
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.fontSize = 10;
        tmpText.color = Color.black;

        // Adjust text position and rotation to face outward
        textObj.transform.localPosition = Vector3.zero;
        textObj.transform.LookAt(radialMenuInstance.transform.position);
        textObj.transform.Rotate(Vector3.up, 180f);
    }
}
private void UpdateRadialMenuHighlight()
{
    if (radialMenuInstance == null || !radialMenuInstance.activeSelf)
    {
        return;
    }

    Vector2 joystickPosition = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.LTouch);
    
    // Diagnostic logs
    Debug.Log($"Left Thumbstick Position: ({joystickPosition.x:F2}, {joystickPosition.y:F2})");
    Debug.Log($"Magnitude: {joystickPosition.magnitude:F2}");

    if (joystickPosition.magnitude > 0.5f) // Adjust this threshold as needed
    {
        float angle = Mathf.Atan2(joystickPosition.y, joystickPosition.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        int buttonIndex = Mathf.FloorToInt(angle / (360f / numberOfButtons));

        // More diagnostic logs
        Debug.Log($"Angle: {angle:F2}°");
        Debug.Log($"Selected Button Index: {buttonIndex}");

        if (buttonIndex != currentHighlightedButton)
        {
            // Unhighlight the previously highlighted button
            if (currentHighlightedButton != -1 && currentHighlightedButton < radialMenuInstance.transform.childCount)
            {
                SetButtonHighlight(currentHighlightedButton, false);
                Debug.Log($"Unhighlighted Button: {currentHighlightedButton}");
            }

            // Highlight the new button
            if (buttonIndex < radialMenuInstance.transform.childCount)
            {
                SetButtonHighlight(buttonIndex, true);
                currentHighlightedButton = buttonIndex;
                Debug.Log($"Highlighted Button: {currentHighlightedButton}");
            }
            else
            {
                Debug.LogError($"Button index {buttonIndex} is out of bounds. Child count: {radialMenuInstance.transform.childCount}");
            }
        }
    }
    else if (currentHighlightedButton != -1)
    {
        // If joystick is centered, unhighlight all buttons
        if (currentHighlightedButton < radialMenuInstance.transform.childCount)
        {
            SetButtonHighlight(currentHighlightedButton, false);
            Debug.Log($"Joystick centered. Unhighlighted Button: {currentHighlightedButton}");
        }
        currentHighlightedButton = -1;
    }
}

private void SetButtonHighlight(int buttonIndex, bool highlight)
{
    if (radialMenuInstance == null)
    {
        Debug.LogError("Radial menu instance is null");
        return;
    }

    if (buttonIndex < 0 || buttonIndex >= radialMenuInstance.transform.childCount)
    {
        Debug.LogError($"Button index {buttonIndex} is out of bounds. Child count: {radialMenuInstance.transform.childCount}");
        return;
    }

    GameObject buttonObj = radialMenuInstance.transform.GetChild(buttonIndex).gameObject;
    Renderer buttonRenderer = buttonObj.GetComponent<Renderer>();
    
    if (buttonRenderer != null)
    {
        buttonRenderer.material = highlight ? new Material(highlightedButtonMaterial) : new Material(defaultButtonMaterial);
        Debug.Log($"Button {buttonIndex} highlight set to {highlight}. Color: {buttonRenderer.material.color}");
    }
    else
    {
        Debug.LogError($"Renderer component not found on button {buttonIndex}");
    }
}
    private void OnRadialButtonClick(int buttonIndex)
    {
        Debug.Log($"Radial button {buttonIndex} clicked");
        // Add your button-specific logic here
        switch (buttonIndex)
        {
            // case 0:
            //     ToggleRecording();
            //     break;
            // case 1:
            //     PlayRecordedAudio();
            //     break;
            // Add more cases for additional buttons
        }
    }


    private void SaveAudioFile()
    {
        string filePath = Path.Combine(Application.persistentDataPath, outputFileName);
        SavWav.Save(filePath, recordedClip);
        Debug.Log("Audio saved as: " + filePath);
    }

    private void SendAudioToBackend(Vector3 position, Quaternion rotation)
    {
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        string filePath = Path.Combine(Application.persistentDataPath, outputFileName);
        string uploadURL = "https://work.manakin-gecko.ts.net:8443/uploadaudio";  // replace with your FastAPI server URL
        StartCoroutine(UploadAudioFile(filePath, uploadURL, position, rotation));
    }
    private void PlayRecordedAudio()
    {
        Debug.Log("Play button clicked");
        AudioSource prefabAudioSource = recordingIndicatorInstance.transform.Find("AudioSource").GetComponent<AudioSource>();
        if (prefabAudioSource != null && prefabAudioSource.clip != null)
        {
            prefabAudioSource.Play();
            Debug.Log("Playing recorded audio.");
        }
        else
        {
            Debug.LogError("No audio recorded or AudioSource not set up correctly in the prefab.");
        }
    }

    IEnumerator UploadAudioFile(string filePath, string uploadURL, Vector3 position, Quaternion rotation)
    {
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        byte[] audioBytes = File.ReadAllBytes(filePath);
        formData.Add(new MultipartFormFileSection("audiofile", audioBytes, filePath, "audio/wav"));
        formData.Add(new MultipartFormDataSection("location", location));
        formData.Add(new MultipartFormDataSection("user_name", username));
        formData.Add(new MultipartFormDataSection("journey_map", journey));
        formData.Add(new MultipartFormDataSection("position", position.ToString()));
        formData.Add(new MultipartFormDataSection("rotation", rotation.ToString()));


        UnityWebRequest www = UnityWebRequest.Post(uploadURL, formData);

        yield return www.SendWebRequest();

    if (www.result != UnityWebRequest.Result.Success)
    {
        Debug.LogError("Upload failed: " + www.error);
    }
    else
    {
        TranscriptionResponse response = JsonUtility.FromJson<TranscriptionResponse>(www.downloadHandler.text);

        Debug.Log("Upload complete! Response: " + www.downloadHandler.text);

        if (response != null && !string.IsNullOrEmpty(response.transcription))
        {
            if (recordingIndicatorInstance != null)
            {
                TextMeshProUGUI transcriptionText = recordingIndicatorInstance.GetComponentInChildren<TextMeshProUGUI>();
                if (transcriptionText != null)
                {
                    transcriptionText.text = response.transcription;
                    Debug.Log("Transcription set: " + response.transcription);
                }
                else
                {
                    Debug.LogError("TranscriptionText component not found in the recording indicator prefab.");
                }
            }
            else
            {
                Debug.LogError("Recording Indicator Instance is null when trying to set transcription.");
            }
        }
        else
        {
            Debug.LogError("Invalid or empty transcription response.");
        }
    }
}
}