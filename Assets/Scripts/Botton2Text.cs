using UnityEngine;
using UnityEngine.UI;
using TMPro; // ���� TextMeshPro �����ռ�

public class ButtonTextGenerator : MonoBehaviour
{
    public Button[] buttons;
    public TextMeshProUGUI[] tmpTexts; // ʹ�� TextMeshProUGUI ����

    void Start()
    {
        // ȷ����ť���ı����鳤��һ��
        if (buttons.Length != tmpTexts.Length)
        {
            Debug.LogError("��ť���ı����鳤�Ȳ�һ��!");
            return;
        }

        // ��ʼʱ���������ı�
        HideAllTexts();

        // Ϊÿ����ť��ӵ���¼�
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i; // ����һ���ֲ�����������ǰ����
            buttons[i].onClick.AddListener(() => ShowText(index));
        }
    }

    void ShowText(int index)
    {
        // ���������ı�
        HideAllTexts();

        // ��ʾ��Ӧ���ı�
        tmpTexts[index].text = "�����ı� " + (index + 1);
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