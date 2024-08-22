using UnityEngine;
using UnityEngine.UI;
using TMPro; // 引入 TextMeshPro 命名空间

public class ButtonTextGenerator : MonoBehaviour
{
    public Button[] buttons;
    public TextMeshProUGUI[] tmpTexts; // 使用 TextMeshProUGUI 数组

    void Start()
    {
        // 确保按钮和文本数组长度一致
        if (buttons.Length != tmpTexts.Length)
        {
            Debug.LogError("按钮和文本数组长度不一致!");
            return;
        }

        // 初始时隐藏所有文本
        HideAllTexts();

        // 为每个按钮添加点击事件
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i; // 创建一个局部变量来捕获当前索引
            buttons[i].onClick.AddListener(() => ShowText(index));
        }
    }

    void ShowText(int index)
    {
        // 隐藏所有文本
        HideAllTexts();

        // 显示对应的文本
        tmpTexts[index].text = "这是文本 " + (index + 1);
        tmpTexts[index].gameObject.SetActive(true);
    }

    void HideAllTexts()
    {
        foreach (TextMeshProUGUI tmpText in tmpTexts)
        {
            tmpText.gameObject.SetActive(false);
        }
    }
}