using UnityEngine;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public VideoClip[] videoClips;

    public void PlayVideo(int index)
    {
        Debug.Log("PlayVideo called with index: " + index); // 添加日志
        if (index >= 0 && index < videoClips.Length)
        {
            Debug.Log("Playing video clip: " + videoClips[index].name); // 添加日志
            videoPlayer.clip = videoClips[index];
            videoPlayer.Play();
        }
        else
        {
            Debug.Log("Invalid index: " + index); // 添加日志
        }
    }
}
