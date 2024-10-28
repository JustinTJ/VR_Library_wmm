using UnityEngine;
using UnityEngine.UI;

public class ReplacePrefabOnClick : MonoBehaviour
{
    public Button button; // 引用按钮
    public GameObject prefabB; // 要生成的新预制体

    void Start()
    {
        // 添加监听器到按钮
        button.onClick.AddListener(ReplacePrefab);
    }

    void ReplacePrefab()
    {
        // 获取当前按钮所在的预制体的引用
        GameObject currentPrefab = button.gameObject.transform.parent.parent.parent.gameObject;

        // 获取当前预制体的父对象
        Transform parentTransform = currentPrefab.transform.parent.parent;

        // 在销毁前获取按钮的父对象的索引
        int siblingIndex = currentPrefab.transform.GetSiblingIndex();

        // 销毁当前预制体
        Destroy(currentPrefab);
        
        parentTransform.GetComponentInChildren<ButtonToPrefabSwitch>().buttonA.gameObject.SetActive(true);

        // 实例化新的预制体并将其放置在原父对象中
        //GameObject newPrefab = Instantiate(prefabB, parentTransform);

        // 设置新的预制体的顺序以匹配原来的预制体
        //newPrefab.transform.SetSiblingIndex(siblingIndex);
    }
}
