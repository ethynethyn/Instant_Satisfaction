using UnityEngine;
using UnityEngine.Video;

public class DisableOnVideoEnd : MonoBehaviour
{
    private VideoPlayer videoPlayer;

    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();

        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
        }
        else
        {
            Debug.LogWarning("No VideoPlayer component found on this object.");
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}