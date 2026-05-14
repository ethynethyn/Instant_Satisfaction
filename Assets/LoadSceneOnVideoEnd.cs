using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class LoadSceneOnVideoEnd : MonoBehaviour
{
    [Header("Scene Loading")]
    [SerializeField] private string sceneToLoad;

    private VideoPlayer videoPlayer;
    private AsyncOperation loadOperation;

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

    private void Start()
    {
        // Begin loading the scene asynchronously in the background
        loadOperation = SceneManager.LoadSceneAsync(sceneToLoad);

        if (loadOperation != null)
        {
            // Prevent automatic scene activation
            loadOperation.allowSceneActivation = false;
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (loadOperation != null)
        {
            // Instantly switch to the loaded scene
            // If loading is incomplete, Unity will switch as soon as possible
            loadOperation.allowSceneActivation = true;
        }
        else
        {
            // Fallback if async load failed
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}