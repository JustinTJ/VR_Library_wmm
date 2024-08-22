using UnityEngine;
using UnityEngine.UI;

public class OpenUrl : MonoBehaviour
{
    // 引用 UI 中的 InputField，用于获取用户输入的 URL
    public InputField input;

    // 当用户点击按钮时调用此方法
    public void OpenUrlByUnity()
    {
        // 获取 InputField 中的文本
        string inputStr = input.text;

        // 检查输入的文本是否为空
        if (!string.IsNullOrEmpty(inputStr))
        {
            // 如果不为空，则在默认浏览器中打开输入的 URL
            Application.OpenURL(inputStr);
        }
        else
        {
            // 如果输入为空，可以在这里显示错误信息或提示用户输入有效的 URL
            Debug.LogWarning("请输入有效的 URL。");
        }
    }
}
