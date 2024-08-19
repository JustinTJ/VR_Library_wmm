using UnityEngine;

public class OpenWebPage : MonoBehaviour
{
    // 在 Inspector 面板中指定要打开的 URL
    public string url = "https://www.google.com";

    // 这个方法将绑定到按钮点击事件上
    public void OpenUrl()
    {
        // 直接打开指定的 URL
        Application.OpenURL(url);
    }
}
