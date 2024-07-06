using UnityEngine;
using LightShaft.Scripts;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class VideoManager : MonoBehaviour
{
    public YoutubePlayer youtubePlayerPrefab, youtubePlayer;
    public List<GameObject> spawnedYoutubePlayer;
    public GameObject overlay;
    public TextMeshProUGUI videoName;
    public RawImage videoRenderer;
    public Sprite playSprite, pauseSprite;
    public Button playButton;
    public VideoPlayer player;
    public MovieMenuManager movieMenuManager;
    public string URL;
    public bool isTrailer = false;

    private bool isPaused = false;

    private void Start()
    {
        playButton.onClick.AddListener(() => GetVideoData());
    }

    private void OnEnable()
    {
        if (isTrailer && movieMenuManager)
        {
            URL = "https://www.youtube.com/watch?v=" + movieMenuManager.trailerKey;
            videoName.text = movieMenuManager.trailerName;
        }
    }

    private void OnDisable()
    {
        playButton.image.sprite = playSprite;
        DestroyYoutubePlayer();
    }

    public void Initialize(string name, string key)
    {
        videoName.text = name;
        URL = "https://www.youtube.com/watch?v=" + key;
    }

    public void TogglePlayPause()
    {
        // Toggle the state
        isPaused = !isPaused;

        // Change the button sprite based on the state
        playButton.image.sprite = isPaused ? pauseSprite : playSprite;
    }

    private void GetVideoData()
    {
        if (youtubePlayer == null)
        {
            CreateANewYoutubePlayer();
        }

        if (videoRenderer.texture == null)
        {
            CreateRenderTexture();
        }

        youtubePlayer.youtubeUrl = URL;
        youtubePlayer.gameObject.SetActive(true);
        youtubePlayer.PlayPause();
        overlay.SetActive(false);
    }

    private void CreateANewYoutubePlayer()
    {
        if(youtubePlayer == null)
        {
            youtubePlayer = Instantiate(youtubePlayerPrefab);
            spawnedYoutubePlayer.Add(youtubePlayer.gameObject);
            youtubePlayer.videoPlayer = player;
        }
    }

    private void CreateRenderTexture()
    {
        // Create a new Render Texture
        RenderTexture renderTexture = new RenderTexture(640, 360, 0);

        player.targetTexture = renderTexture;
        videoRenderer.texture = renderTexture;
    }

    private void DestroyYoutubePlayer()
    {
        if (spawnedYoutubePlayer.Count > 0)
        {
            for (int i = 0; i < spawnedYoutubePlayer.Count; i++)
            {
                Destroy(spawnedYoutubePlayer[i]);
            }
            spawnedYoutubePlayer.Clear();
        }
    }
}
