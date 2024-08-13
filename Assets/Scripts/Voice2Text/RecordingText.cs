using UnityEngine;
using TMPro;  // 引入TextMeshPro命名空间

public class ToggleButtonTextTMP : MonoBehaviour
{
    public TextMeshProUGUI buttonText;  // 将按钮的TextMeshProUGUI组件拖到这个字段中
    private bool isRecording = false;

    private void Start()
    {
        // 确保初始状态为"Record"
        if (buttonText != null)
        {
            buttonText.text = "Record";
        }
    }

    public void OnButtonClick()
    {
        if (buttonText != null)
        {
            // 切换文本内容
            if (isRecording)
            {
                buttonText.text = "Record";
                isRecording = false;
            }
            else
            {
                buttonText.text = "Recording";
                isRecording = true;
            }
        }
    }
}
