using UnityEngine;
using UnityEngine.UI;

public class EmojiControl : MonoBehaviour
{
    public Button[] button1List; // 第一个按钮组
    public Button[] button2List; // 第二个按钮组

    private bool isDefaultState = true; // 默认状态为true,表示第一个按钮组激活，第二个按钮组不激活

    void Start()
    {
        // 在游戏开始时调用SetButtonsState函数，将按钮组状态设置为默认状态
        SetButtonsState(isDefaultState);
    }

    // public void OnButtonClick(int index)
    // {
    //     if (index <= button1List.Length && index >= 0)
    //     {
    //         // 如果点击的是第一个按钮组中的某个按钮，则执行以下操作
    //         button1List[index].gameObject.SetActive(false); // 将对应的第一个按钮组中的按钮设为不激活状态
    //         button2List[index].gameObject.SetActive(true); // 将对应的第二个按钮组中的按钮设为激活状态
    //     }
    //     else if (index <= button2List.Length && index >= 0)
    //     {
    //         // 如果点击的是第二个按钮组中的某个按钮，则执行以下操作
    //         button1List[index].gameObject.SetActive(true); // 将对应的第一个按钮组中的按钮设为激活状态
    //         button2List[index].gameObject.SetActive(false); // 将对应的第二个按钮组中的按钮设为不激活状态
    //     }
    // }

    public void OnButtonClick(int index)
    {
        if (index <= button1List.Length && index >= 0)
        {
            // 如果点击的是第一个按钮组中的某个按钮，则执行以下操作
            button1List[index].gameObject.SetActive(false); // 将对应的第一个按钮组中的按钮设为不激活状态
            button2List[index].gameObject.SetActive(true); // 将对应的第二个按钮组中的按钮设为激活状态
        }
        else if (index <= button2List.Length && index >= 0)
        {
            // 如果点击的是第二个按钮组中的某个按钮，则执行以下操作
            button1List[index].gameObject.SetActive(true); // 将对应的第一个按钮组中的按钮设为激活状态
            button2List[index].gameObject.SetActive(false); // 将对应的第二个按钮组中的按钮设为不激活状态

            // 添加逻辑：将 button1List 中的按钮设为亮起状态
            for (int i = 0; i < button1List.Length; i++)
            {
                button1List[i].interactable = true; // 设置按钮为可交互状态
            }
        }
    }


    private void SetButtonsState(bool toDefault)
    {
        for (int i = 0; i < button1List.Length; i++)
        {
            button1List[i].gameObject.SetActive(toDefault); // 根据参数toDefault设置第一个按钮组中对应按钮的状态
            button2List[i].gameObject.SetActive(!toDefault); // 根据参数toDefault设置第二个按钮组中对应按钮的状态
        }
    }
}

