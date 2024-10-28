using UnityEngine;
using UnityEngine.UI;
using System;
using System.Reflection;
using Oculus.Interaction;
using Oculus.Interaction.Surfaces;
using System.Collections.Generic;
using UnityEngine.EventSystems;
public class BoxButton : MonoBehaviour
{
    public int BoxIndex { get; set; }
}
public class JourneyNine : MonoBehaviour
{
    public int gridSize = 3;
    public float spacing = 50f;
    public float boxSize = 100f;


    private Canvas canvas;
    private AudioClip[] recordings;
    private bool[] isRecording;
    private AudioSource audioSource;

    private Dictionary<int, AudioClip> boxAudioClips = new Dictionary<int, AudioClip>();



    void Start()
    {
        CreateCanvasIfNeeded();
        InitializeAudioRecording();
        CreateGrid();
    }

    void InitializeAudioRecording()
    {
        recordings = new AudioClip[gridSize * gridSize];
        isRecording = new bool[gridSize * gridSize];
        audioSource = gameObject.AddComponent<AudioSource>();
    }
    void CreateCanvasIfNeeded()
    {
        canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
        GameObject canvasObject = new GameObject("Canvas");
        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        // Set the canvas size and position
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(500, 500); // Adjust size as needed
        canvasRect.position = new Vector3(8, 3, 3); // Adjust position as needed
        canvasRect.localScale = Vector3.one * 0.01f; // Adjust scale as needed

        // Add Canvas Scaler
        CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080); // Set to your target resolution
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = 0.5f; // Adjust this value to balance between width and height

        var graphicRaycaster = canvasObject.AddComponent<GraphicRaycaster>();


            // Add Pointable Canvas script
            var pointableCanvas = canvasObject.AddComponent<PointableCanvas>();

            // pointableCanvas.Canvas = canvas;
            // Check if pointableCanvas is valid and use the InjectCanvas method
            if (pointableCanvas != null)
            {
                var injectCanvasMethod = pointableCanvas.GetType().GetMethod("InjectCanvas", BindingFlags.Public | BindingFlags.Instance);
                if (injectCanvasMethod != null)
                {
                    injectCanvasMethod.Invoke(pointableCanvas, new object[] { canvas });
                }
                else
                {
                    Debug.LogError("InjectCanvas method not found.");
                }
            }
            var rayInteractable = canvasObject.AddComponent<RayInteractable>();

            // the structure of is this
            // the big canvas has Graphic Raycaster, Pointable Canvas, Ray Interactable

            // Should have a child that is Surface is PlaneSurface, Clipped Plane Surface which has the earlier Plane Surface defined, and the Bounds Clipper

            var createSurface = CreateSurface(canvasObject);


            // Add Ray Interactable script
            if (rayInteractable != null && createSurface != null)
            {
                var injectSurfaceMethod = rayInteractable.GetType().GetMethod("InjectSurface", BindingFlags.Public | BindingFlags.Instance);
                if (injectSurfaceMethod != null)
                {
                    injectSurfaceMethod.Invoke(rayInteractable, new object[] { createSurface });
                }
                else
                {
                    Debug.LogError("InjectSurface method not found.");
                }
            }

        // Camera mainCamera = Camera.main;
        // if (mainCamera != null)
        // {
        //     mainCamera.transform.position = new Vector3(0, 0, 0);
        //     mainCamera.transform.LookAt(canvasRect.position);
        // }

            // Create Surface object
            // CreateSurface(canvasObject);
        }

    }
    void OnSelect()
    {
        Debug.Log("Object Selected!");
        // Add your logic for when the object is selected
    }

    // Example of what happens when something is unselected
    void OnUnselect()
    {
        Debug.Log("Object Unselected!");
        // Add your logic for when the object is unselected
    }

    void CreateGrid()
    {
        float totalWidth = gridSize * boxSize + (gridSize - 1) * spacing;
        float startX = -totalWidth / 2 + boxSize / 2;
        float startY = totalWidth / 2 - boxSize / 2;

        for (int i = 0; i < gridSize * gridSize; i++)
        {
            int row = i / gridSize;
            int col = i % gridSize;

            GameObject box = CreateBox(i);
            RectTransform rectTransform = box.GetComponent<RectTransform>();

            float xPos = startX + col * (boxSize + spacing);
            float yPos = startY - row * (boxSize + spacing);

            rectTransform.anchoredPosition = new Vector2(xPos, yPos);
        }
    }

    GameObject CreateBox(int index)
    {
        GameObject box = new GameObject($"Box {index + 1}");
        box.transform.SetParent(canvas.transform, false);

        RectTransform rectTransform = box.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(boxSize, boxSize);

        Image image = box.AddComponent<Image>();
        image.color = Color.white;

        // Create buttons for this box
        CreateBoxButtons(box.transform, index);

        return box;
    }

void CreateBoxButtons(Transform boxTransform, int boxIndex)
{
    string[] buttonTexts = { "Save", "Record", "Play" };
    float buttonWidth = boxSize / 3 - 5f;
    float buttonHeight = 30f;
    float yPosition = -boxSize / 2 - buttonHeight / 2 - 5f;

    // Calculate total width of all buttons
    float totalButtonWidth = buttonTexts.Length * buttonWidth + (buttonTexts.Length - 1) * 5f; // 5f is spacing between buttons
    float startX = -totalButtonWidth / 2 + buttonWidth / 2;

    for (int i = 0; i < buttonTexts.Length; i++)
    {
        float xPosition = startX + i * (buttonWidth + 5f); // 5f is spacing between buttons
        GameObject buttonObj;
        if (buttonTexts[i] == "Record")
        {
            buttonObj = CreateRecordButton(boxTransform, new Vector2(xPosition, yPosition), boxIndex);
            // Add Button add listener
            buttonObj.GetComponent<Button>().onClick.AddListener(() => ToggleRecording(boxIndex, buttonObj.GetComponentInChildren<Text>(), buttonObj.GetComponent<Image>()));

        }
        else if (buttonTexts[i] == "Play")
        {
            buttonObj = CreatePlayButton(boxTransform, new Vector2(xPosition, yPosition), boxIndex);
            // Add Button add listener
            buttonObj.GetComponent<Button>().onClick.AddListener(() => PlayRecording(boxIndex));
        }
        else
        {
            buttonObj = CreateButton(boxTransform, buttonTexts[i], new Vector2(xPosition, yPosition), boxIndex);
            // Add Button add listener
            buttonObj.GetComponent<Button>().onClick.AddListener(() => Debug.Log($"{buttonTexts[i]} button clicked for box {boxIndex + 1}"));

        }

        // Add BoxButton component to each button
        BoxButton boxButton = buttonObj.AddComponent<BoxButton>();
        boxButton.BoxIndex = boxIndex;
    }
}
Component CreateSurface(GameObject parent)
{
    GameObject surfaceObject = new GameObject("Surface");
    surfaceObject.transform.SetParent(parent.transform, false);

    // Add Plane Surface component
    var planeSurface = surfaceObject.AddComponent<PlaneSurface>();
    if (planeSurface != null)
    {
        planeSurface.InjectNormalFacing(PlaneSurface.NormalFacing.Forward);
        planeSurface.InjectDoubleSided(true);
    }

    // Get the canvas RectTransform
    RectTransform canvasRect = parent.GetComponent<RectTransform>();

    // Add Bounds Clipper component
    var boundsClipper = surfaceObject.AddComponent<BoundsClipper>();
    if (boundsClipper != null && canvasRect != null)
    {
        // Set the size to match the canvas size
        boundsClipper.Size = new Vector3(canvasRect.rect.width, canvasRect.rect.height, 0.1f);
    }

    // Add Clipped Plane Surface component
    var clippedPlaneSurface = surfaceObject.AddComponent<ClippedPlaneSurface>();

    if (clippedPlaneSurface != null)
    {
        List<IBoundsClipper> clippers = new List<IBoundsClipper> { boundsClipper };
        clippedPlaneSurface.InjectAllClippedPlaneSurface(planeSurface, clippers);
    }
    else
    {
        Debug.LogError("ClippedPlaneSurface component not found.");
    }

    return clippedPlaneSurface;
}

GameObject CreateButton(Transform parent, string buttonText, Vector2 localPosition, int boxIndex)
{
    GameObject buttonObj = new GameObject(buttonText);
    buttonObj.transform.SetParent(parent, false);

    RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
    rectTransform.anchoredPosition = localPosition;
    rectTransform.sizeDelta = new Vector2(boxSize / 3 - 5f, 30f);

    Image image = buttonObj.AddComponent<Image>();
    image.color = new Color(0.8f, 0.8f, 0.8f);

    // Add Button component and listener
    Button button = buttonObj.AddComponent<Button>();
    button.onClick.AddListener(() => Debug.Log($"{buttonText} button clicked for box {boxIndex + 1}"));

    Text text = CreateText(buttonObj.transform, buttonText, Vector2.zero, 14);
    text.color = Color.black;
    // Ensure text fits within button
    text.resizeTextForBestFit = true;
    text.resizeTextMinSize = 10;
    text.resizeTextMaxSize = 14;
    return buttonObj;
}
    GameObject CreatePlayButton(Transform parent, Vector2 localPosition, int boxIndex)
    {
    GameObject buttonObj = new GameObject("PlayButton");
    buttonObj.transform.SetParent(parent, false);

    RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
    rectTransform.anchoredPosition = localPosition;
    rectTransform.sizeDelta = new Vector2(boxSize / 3 - 5f, 30f);

    Image image = buttonObj.AddComponent<Image>();
    image.color = new Color(0.8f, 0.8f, 0.8f);

    Text text = CreateText(buttonObj.transform, "Play", Vector2.zero, 14);
    text.color = Color.black;
    text.resizeTextForBestFit = true;
    text.resizeTextMinSize = 10;
    text.resizeTextMaxSize = 14;

    // Add Button and listener
    Button playButton = buttonObj.AddComponent<Button>();
    playButton.onClick.AddListener(() => PlayRecording(boxIndex));

    return buttonObj;
    }
void PlayRecording(int boxIndex)
{
    if (recordings[boxIndex] != null)
    {
        audioSource.Stop(); // Stop any currently playing audio
        audioSource.clip = recordings[boxIndex];
        audioSource.Play();
        Debug.Log($"Playing recording for box {boxIndex + 1}");
    }
    else
    {
        Debug.Log($"No recording found for box {boxIndex + 1}");
    }
}
    GameObject CreateRecordButton(Transform parent, Vector2 localPosition, int boxIndex)
    {
    GameObject buttonObj = new GameObject("RecordButton");
    buttonObj.transform.SetParent(parent, false);

    RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
    rectTransform.anchoredPosition = localPosition;
    rectTransform.sizeDelta = new Vector2(boxSize / 3 - 5f, 30f);

    Image image = buttonObj.AddComponent<Image>();
    image.color = Color.white; // Default color

    Text text = CreateText(buttonObj.transform, "Record", Vector2.zero, 14);
    text.color = Color.black;
    text.resizeTextForBestFit = true;
    text.resizeTextMinSize = 10;
    text.resizeTextMaxSize = 14;

    // Add Button and listener
    Button recordButton = buttonObj.AddComponent<Button>();
    recordButton.onClick.AddListener(() => ToggleRecording(boxIndex, text, image));

    return buttonObj;
    }
// Component AddComponentByName(GameObject gameObject, string componentName)
// {
//         return gameObject.AddComponent<componentName>;
// }



void ToggleRecording(int boxIndex, Text buttonText, Image buttonImage)
{
    Debug.Log($"Toggling recording for box {boxIndex + 1}");
    if (isRecording[boxIndex])
    {
        StopRecording(boxIndex);
        buttonText.text = "Record";
        buttonImage.color = Color.white; // Reset to default color
    }
    else
    {
        StartRecording(boxIndex);
        buttonText.text = "Stop";
        buttonImage.color = Color.red; // Set to red when recording
    }
    isRecording[boxIndex] = !isRecording[boxIndex];
}

    void StartRecording(int boxIndex)
    {
        recordings[boxIndex] = Microphone.Start(null, false, 60, 44100);
        Debug.Log($"Started recording for box {boxIndex + 1}");
    }

void StopRecording(int boxIndex)
{
    Microphone.End(null);
    Debug.Log($"Stopped recording for box {boxIndex + 1}");

    // Store the recording in the dictionary
    boxAudioClips[boxIndex] = recordings[boxIndex];

    // Optional: Play back the recording
    audioSource.clip = recordings[boxIndex];
    audioSource.Play();
}

Text CreateText(Transform parent, string content, Vector2 position, int fontSize)
{
    GameObject textObj = new GameObject("Text");
    textObj.transform.SetParent(parent, false);

    RectTransform rectTransform = textObj.AddComponent<RectTransform>();
    rectTransform.anchoredPosition = position;
    rectTransform.sizeDelta = new Vector2(boxSize / 3 - 5f, 30f);

    Text text = textObj.AddComponent<Text>();
    text.text = content;
    text.alignment = TextAnchor.MiddleCenter;
    text.color = Color.black;
    text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    text.fontSize = fontSize;
    // Ensure text fits within button
    text.resizeTextForBestFit = true;
    text.resizeTextMinSize = 10;
    text.resizeTextMaxSize = fontSize;

    return text;
}
}
