using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class SpeechTest : MonoBehaviour
{
    [HideInInspector]
    public AudioSource audioSource;

    private AudioClip recordedAudioClip;
    private bool recording = false;

    public Button recordButton; // 录音按钮
    public Button pauseButton;  // 暂停按钮

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // 绑定按钮事件
        if (recordButton != null)
        {
            recordButton.onClick.AddListener(StartRecording);
        }
        
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(PauseRecording);
        }

        // 测试语音合成
        SendTextToSpeechMsg("你好啊，我是讯飞语音助手！", audioClip =>
        {
            if (audioClip != null)
            {
                audioSource.clip = audioClip;
                audioSource.Play();
            }
        });
    }

    public void StartRecording()
    {
        if (!recording)
        {
            recording = true;
            StartRecord();
            Debug.Log("Recording started");
        }
        else
        {
            Debug.LogWarning("Already recording");
        }
    }

    public void PauseRecording()
    {
        if (recording)
        {
            recording = false;
            EndRecord((text, _) =>
            {
                Debug.Log($"讯飞语音转文本成功！文本为：{text}");
            });
            Debug.Log("Recording paused");
        }
        else
        {
            Debug.LogWarning("No recording to pause");
        }
    }

    #region 讯飞文本转语音

    public void SendTextToSpeechMsg(string text, Action<AudioClip> callback)
    {
        JObject jObject = new JObject
        {
            ["text"] = text,
            ["voice"] = "xiaoyan" // 选择语音
        };

        StartCoroutine(SendTextToSpeechMsgCoroutine(jObject, callback));
    }

    private IEnumerator SendTextToSpeechMsgCoroutine(JObject message, Action<AudioClip> callback)
    {
        Task<string> resultJson = XunFeiManager.Instance.TextToSpeech(message);
        yield return new WaitUntil(() => resultJson.IsCompleted);

        if (resultJson.IsCompletedSuccessfully)
        {
            JObject obj = JObject.Parse(resultJson.Result);
            string base64Audio = obj["data"]?.ToString();

            if (!string.IsNullOrEmpty(base64Audio))
            {
                float[] audioData = BytesToFloat(Convert.FromBase64String(base64Audio));
                if (audioData.Length > 0)
                {
                    AudioClip audioClip = AudioClip.Create("SynthesizedAudio", audioData.Length, 1, 16000, false);
                    audioClip.SetData(audioData, 0);
                    callback?.Invoke(audioClip);
                }
                else
                {
                    Debug.LogWarning("讯飞文本转语音失败，音频数据为空");
                    callback?.Invoke(null);
                }
            }
            else
            {
                Debug.LogWarning("讯飞文本转语音失败，未获取到音频数据");
                callback?.Invoke(null);
            }
        }
        else
        {
            Debug.LogError($"讯飞文本转语音消息发送失败，错误信息：{resultJson.Result}");
            callback?.Invoke(null);
        }
    }

    private static float[] BytesToFloat(byte[] byteArray)
    {
        float[] sounddata = new float[byteArray.Length / 2];
        for (int i = 0; i < sounddata.Length; i++)
        {
            sounddata[i] = BytesToFloat(byteArray[i * 2], byteArray[i * 2 + 1]);
        }
        return sounddata;
    }

    private static float BytesToFloat(byte firstByte, byte secondByte)
    {
        short s = BitConverter.ToInt16(new byte[] { firstByte, secondByte }, 0);
        return s / 32768.0f;
    }

    #endregion

    #region 讯飞语音转文本

    public void StartRecord()
    {
        recordedAudioClip = Microphone.Start(null, true, 40, 16000);
    }

    public void EndRecord(Action<string, AudioClip> speechToTextCallback)
    {
        if (speechToTextCallback == null) return;

        Microphone.End(null);
        recordedAudioClip = TrimSilence(recordedAudioClip, 0.01f);
        SendSpeechToTextMsg(recordedAudioClip, text =>
        {
            speechToTextCallback?.Invoke(text, recordedAudioClip);
        });
    }

    public void SendSpeechToTextMsg(AudioClip audioClip, Action<string> callback)
    {
        byte[] bytes = AudioClipToBytes(audioClip);
        JObject jObject = new JObject
        {
            ["data"] = Convert.ToBase64String(bytes)
        };

        StartCoroutine(SendSpeechToTextMsgCoroutine(jObject, callback));
    }

    private IEnumerator SendSpeechToTextMsgCoroutine(JObject message, Action<string> callback)
    {
        Task<string> resultJson = XunFeiManager.Instance.SpeechToText(message);
        yield return new WaitUntil(() => resultJson.IsCompleted);

        if (resultJson.IsCompletedSuccessfully)
        {
            JObject obj = JObject.Parse(resultJson.Result);
            string text = obj["text"]?.ToString();
            callback?.Invoke(text);
        }
        else
        {
            Debug.LogError("讯飞语音转文本消息发送失败");
            callback?.Invoke(string.Empty);
        }
    }

    private static byte[] AudioClipToBytes(AudioClip audioClip)
    {
        float[] data = new float[audioClip.samples];
        audioClip.GetData(data, 0);
        byte[] outData = new byte[data.Length * 2];
        for (int i = 0; i < data.Length; i++)
        {
            short tempShort = (short)(data[i] * 32767);
            BitConverter.GetBytes(tempShort).CopyTo(outData, i * 2);
        }
        return outData;
    }

    private static AudioClip TrimSilence(AudioClip clip, float min)
    {
        var samples = new List<float>(clip.samples);
        clip.GetData(samples.ToArray(), 0);
        return TrimSilence(samples, min, clip.channels, clip.frequency);
    }

    private static AudioClip TrimSilence(List<float> samples, float min, int channels, int hz)
    {
        int startIndex = 0, endIndex = samples.Count - 1;

        while (startIndex < samples.Count && Mathf.Abs(samples[startIndex]) <= min)
        {
            startIndex++;
        }

        while (endIndex > startIndex && Mathf.Abs(samples[endIndex]) <= min)
        {
            endIndex--;
        }

        if (endIndex - startIndex <= 0)
        {
            Debug.LogWarning("剔除后的AudioClip长度为0");
            return null;
        }

        var trimmedSamples = samples.GetRange(startIndex, endIndex - startIndex + 1);
        var clip = AudioClip.Create("TrimmedClip", trimmedSamples.Count, channels, hz, false);
        clip.SetData(trimmedSamples.ToArray(), 0);
        return clip;
    }

    #endregion
}
