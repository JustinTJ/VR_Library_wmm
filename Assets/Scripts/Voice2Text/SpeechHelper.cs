using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.WebSockets;
using WebSocketSharp;
using System.Text;
using System.Security.Cryptography;
using LitJson;
using Newtonsoft.Json;
using UnityEngine.Networking.PlayerConnection;
using UnityEngine.UI;

public class SpeechHelper : MonoBehaviour
{
    private int last_length = -1;
    private float[] volumeData = new float[9999];
    private short[] intData = new short[9999];
    bool isRunning = true;
    public event Action<string> 语音识别完成事件;   //语音识别回调事件
    [HideInInspector]
    public AudioClip RecordedClip;
    private string micphoneName = string.Empty;
    WebSocketSharp.WebSocket speechWebSocket;
    private System.Action<string> resultCallback;
    public Text contentText;
    private bool isEnd = false;

    public Button startButton;
    private void Start()
    {
        InitSpeechHelper(SpeechToText);
    }
    
    public void InitSpeechHelper(System.Action<string> textCallback)
    {
        resultCallback = textCallback;
    }

    private void SpeechToText(string s)
    {
        contentText.text += s;
        isRunning = true;
    }
    
    
    //开始语音转文字
    public void StartSpeech()
    {
        startButton.interactable = false;
        if (speechWebSocket != null && speechWebSocket.ReadyState == WebSocketSharp.WebSocketState.Open)
        {
            Debug.LogWarning("开始语音识别失败！，等待上次识别连接结束");
            return;
        }
        if(Microphone.devices.Length <= 0)
        {
            Debug.LogWarning("找不到麦克风");
            return;
        }
        messageQueue.Clear();
        micphoneName = Microphone.devices[0];
        Debug.Log("micphoneName:" + micphoneName);
        isRunning = true;
        isEnd = false;
        try
        {
            RecordedClip = Microphone.Start(micphoneName, false, 60, 16000);
            ConnectSpeechWebSocket();
        }
        catch(Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }
    //adb forward tcp:34999 localabstract:Unity-com.example.iflyteka
    //停止语音转文字
    public void StopSpeech()
    {
        if (!isEnd)
        {
            SendEndMsg(null);
            Microphone.End(micphoneName);
            isEnd = true;
            startButton.interactable = true;
        }
        Debug.Log("识别结束，停止录音");
    }
 
    void ConnectSpeechWebSocket()
    {
        try
        {
            speechWebSocket = new WebSocketSharp.WebSocket(GetWebSocketUrl());
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError(ex.Message);
            return;
        }
 
        speechWebSocket.OnOpen += (sender, e) =>
        {
            Debug.Log("OnOpen");
            speechWebSocket.OnClose += OnWebSocketClose;
        };
        speechWebSocket.OnMessage += OnInitMessage;
        speechWebSocket.OnError += OnError;
        speechWebSocket.ConnectAsync();
        StartCoroutine(SendVoiceData());
    }
    void OnWebSocketClose(object sender, CloseEventArgs e)
    {
        Debug.Log("OnWebSocketClose");
    }
    private static Queue<string> messageQueue = new Queue<string>();
    void OnInitMessage(object sender, WebSocketSharp.MessageEventArgs e)
    {
        UnityEngine.Debug.Log("WebSocket数据返回：" + e.Data);
        messageQueue.Enqueue(e.Data);
    }
    private void MainThreadOnMessage(string message)
    {
        try
        {
            XFResponse response = JsonConvert.DeserializeObject<XFResponse>(message);
            Debug.Log("response.code:"+response.code+"response.data"+response.data);
            if (0 != response.code)
            {
                return;
            }
            if (response.action.Equals("result"))
            {
                var result = ParseXunfeiRecognitionResult(response.data);
                Debug.Log("result:"+result);
                if(result.IsFinal)
                {
                    Debug.Log("Text最终:" + result.Text);
                    resultCallback?.Invoke(result.Text);
                }else
                {
                    Debug.Log("Text中间:" + result.Text);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }
    void OnError(object sender, ErrorEventArgs e)
    {
        UnityEngine.Debug.Log("WebSoclet:发生错误:" + e.Message);
 
    }
 
    public SpeechRecognitionResult ParseXunfeiRecognitionResult(string dataJson)
    {
        StringBuilder builder = new StringBuilder();
        SpeechRecognitionResult res = new SpeechRecognitionResult();
        try
        {
            JsonData data = JsonMapper.ToObject(dataJson);
            JsonData cn = data["cn"];
            JsonData st = cn["st"];
            if (st["ed"].ToString().Equals("0"))
            {
                res.IsFinal = false;
            }
            else
            {
                res.IsFinal = true;
            }
            JsonData rtArry = st["rt"];
            foreach (JsonData rtObject in rtArry)
            {
                JsonData wsArr = rtObject["ws"];
                foreach (JsonData wsObject in wsArr)
                {
                    JsonData cwArr = wsObject["cw"];
                    foreach (JsonData cwObject in cwArr)
                    {
                        builder.Append(cwObject["w"].ToString());
                    }
                }
            }
        }catch(Exception ex)
        {
            Debug.LogError(ex.Message);
        }
        res.Text = builder.ToString();
        return res;
    }
 
    void SendData(byte[] voiceData)
    {
        Debug.Log("SendData:" + voiceData.Length + ",time:" + Time.realtimeSinceStartup);
        if (speechWebSocket.ReadyState != WebSocketSharp.WebSocketState.Open)
        {
            return;
        }
        try
        {
            if (speechWebSocket != null && speechWebSocket.IsAlive)
            {
                speechWebSocket.SendAsync(voiceData, success =>
                {
                    if (success)
                    {
                        UnityEngine.Debug.Log("WebSoclet:发送成功：" + voiceData.Length);
                    }
                    else
                    {
                        UnityEngine.Debug.Log("WebSoclet:发送失败：");
                    }
                });
            }
        }
        catch
        {
 
        }
    }
    
    void SendEndMsg(System.Action callback)
    {
        string endMsg = "{\"end\": true}";
        byte[] data = Encoding.UTF8.GetBytes(endMsg);
        try
        {
            if (speechWebSocket != null && speechWebSocket.IsAlive)
            {
                speechWebSocket.SendAsync(data, success =>
                {
                    if (success)
                    {
                        UnityEngine.Debug.Log("WebSoclet:发送END成功：" + data.Length);
                    }
                    else
                    {
                        UnityEngine.Debug.Log("WebSoclet:发送END失败：");
                    }
                    callback?.Invoke();
                });
            }
        }
        catch
        {
 
        }
    }
 
    IEnumerator SendVoiceData()
    {
        yield return new WaitUntil(()=> (speechWebSocket.ReadyState == WebSocketSharp.WebSocketState.Open));
        yield return new WaitWhile(() => Microphone.GetPosition(micphoneName) <= 0);
        float t = 0;
        int position = Microphone.GetPosition(micphoneName);
        const float waitTime = 0.04f;//每隔40ms发送音频
        int lastPosition = 0;
        const int Maxlength = 640;//最大发送长度
        Debug.Log("position:" + position + ",samples:" + RecordedClip.samples);
        while (position < RecordedClip.samples && speechWebSocket.ReadyState == WebSocketSharp.WebSocketState.Open)
        {
            t += waitTime;
            yield return new WaitForSecondsRealtime(waitTime);
            if (Microphone.IsRecording(micphoneName)) position = Microphone.GetPosition(micphoneName);
            //Debug.Log("录音时长：" + t + "position=" + position + ",lastPosition=" + lastPosition);
            if (position <= lastPosition)
            {
                Debug.LogWarning("字节流发送完毕！强制结束！");
                break;
            }
            int length = position - lastPosition > Maxlength ? Maxlength : position - lastPosition;
            byte[] date = GetClipData(lastPosition, length, RecordedClip);
            //SendData(date);
            lastPosition = lastPosition + length;
        }
        yield return new WaitForSecondsRealtime(waitTime);
        if (!isEnd)
        {
            SendEndMsg(null);
            Microphone.End(micphoneName);
            isEnd = true;
            startButton.interactable = true;
        }
        // SendEndMsg(null);
        // Microphone.End(micphoneName);
    }
    public byte[] GetClipData(int star, int length, AudioClip recordedClip)
    {
        float[] soundata = new float[length];
        recordedClip.GetData(soundata, star);
        int rescaleFactor = 32767;
        byte[] outData = new byte[soundata.Length * 2];
        for (int i = 0; i < soundata.Length; i++)
        {
            short temshort = (short)(soundata[i] * rescaleFactor);
            byte[] temdata = BitConverter.GetBytes(temshort);
            outData[i * 2] = temdata[0];
            outData[i * 2 + 1] = temdata[1];
        }
        return outData;
    }
 
    private string GetWebSocketUrl()
    {
        string appid = "4c846351";
        string ts = GetCurrentUnixTimestampMillis().ToString();
        string baseString = appid + ts;
        string md5 = GetMD5Hash(baseString);
        UnityEngine.Debug.Log("baseString:" + baseString + ",md5:" + md5);
        string sha1 = CalculateHmacSha1(md5, "de380814aeae4685e490fc010ec853ad");
        string signa = sha1;
        string url = string.Format("ws://rtasr.xfyun.cn/v1/ws?appid={0}&ts={1}&signa={2}", appid, ts, signa);
        UnityEngine.Debug.Log(url);
        return url;
    }
    private long GetCurrentUnixTimestampMillis()
    {
        DateTime unixStartTime = new DateTime(1970, 1, 1).ToLocalTime();
        DateTime now = DateTime.Now;// DateTime.UtcNow;
        TimeSpan timeSpan = now - unixStartTime;
        long timestamp = (long)timeSpan.TotalSeconds;
        return timestamp;
    }
    public string GetMD5Hash(string input)
    {
        MD5 md5Hasher = MD5.Create();
        byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));
        StringBuilder sBuilder = new StringBuilder();
        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }
        return sBuilder.ToString();
    }
    public string CalculateHmacSha1(string data, string key)
    {
        HMACSHA1 hmac = new HMACSHA1(Encoding.UTF8.GetBytes(key));
        byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hashBytes);
    }
 
    private void Update()
    {
        if (isRunning)
        {
            byte[] voiceData = GetVoiveData();
            if (voiceData != null)
            {
                SendData(voiceData);
            }
        }
        if (messageQueue.Count > 0)
        {
            MainThreadOnMessage(messageQueue.Dequeue());
        }
    }

    private byte[] GetVoiveData()
    {
        if (RecordedClip == null)
        {
            return null;
        }
        int new_length = Microphone.GetPosition(null);
        if (new_length == last_length)
        {
            if (Microphone.devices.Length == 0)
            {
                isRunning = false;
            }
            return null;
        }
        int length = new_length - last_length;
        int offset = last_length + 1;
        last_length = new_length;
        if (offset < 0)
        {
            return null;
        }
        if (length < 0)
        {
            float[] temp = new float[RecordedClip.samples];
            RecordedClip.GetData(temp, 0);
            int lengthTail = RecordedClip.samples - offset;
            int lengthHead = new_length + 1;
            try
            {
                Array.Copy(temp, offset, volumeData, 0, lengthTail);
                Array.Copy(temp, 0, volumeData, lengthTail + 1, lengthHead);
                length = lengthTail + lengthHead;
 
            }
            catch (Exception)
            {
                return null;
            }
        }
        else
        {
            if (length > volumeData.Length)
            {
                volumeData = new float[length];
                intData = new short[length];
            }
            RecordedClip.GetData(volumeData, offset);
        }
        byte[] bytesData = new byte[length * 2];
        int rescaleFactor = 32767; //to convert float to Int16
        for (int i = 0; i < length; i++)
        {
            intData[i] = (short)(volumeData[i] * rescaleFactor);
            byte[] byteArr = BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }
        return bytesData;
    }
}

[Serializable]
public struct XFResponse
{
    public string action;
    public int code;
    public string data;
    public string desc;
    public string sid;
}
[Serializable]
public struct SpeechRecognitionResult
{
    public string Text;        
    public bool IsFinal;        
}
