using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ButtonReplaceManagement : MonoBehaviour
{
    public Button button; // 引用按钮
    public GameObject prefabB; // 要生成的新预制体

    [HideInInspector]
    public Transform deactivatedCanvas;

    void Start()
    {
        // 添加监听器到按钮
        button.onClick.AddListener(ReplacePrefab);
    }

    void ReplacePrefab()
    {
        var canvas = button.transform.parent.parent;
        deactivatedCanvas = canvas;
        var prefabRoot = canvas.transform.parent;
        canvas.gameObject.SetActive(false);
        Instantiate(prefabB, prefabRoot);

        // 实例化新的预制体并将其放置在原父对象中
        //GameObject newPrefab = Instantiate(prefabB, parentTransform);

        // 设置新的预制体的顺序以匹配原来的预制体
        //newPrefab.transform.SetSiblingIndex(siblingIndex);
    }
}
