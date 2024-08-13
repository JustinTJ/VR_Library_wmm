using UnityEngine;
using UnityEngine.UI;  // 引入UI命名空间

public class ConsoleToUI : MonoBehaviour
{
    public Text uiText;  // 将你的UI Text拖到这个字段中
    private string log = "";

    void OnEnable()
    {
        // 订阅Console日志事件
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        // 取消订阅Console日志事件
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // 检查日志信息是否包含特定的字符串
        if (logString.Contains("讯飞语音转文本"))
        {
            // 清空现有日志
            log = "";
            // 将匹配的日志信息设置为新的日志
            log = logString + "\n";
            uiText.text = log;  // 更新UI Text内容为最新的日志信息
        }
    }
}

