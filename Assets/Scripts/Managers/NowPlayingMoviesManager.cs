using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NowPlayingMoviesManager : MonoBehaviour
{
    private string bearerToken = "eyJhbGciOiJIUzI1NiJ9.eyJhdWQiOiI2Y2E2Mzk4ZjNhYTgzNzIyNGFkMTNhMWU0NmYxNzg1OSIsInN1YiI6IjY1MmI3NzQ4MzU4ZGE3MDBlM2JkYjQzYiIsInNjb3BlcyI6WyJhcGlfcmVhZCJdLCJ2ZXJzaW9uIjoxfQ.RJtry7TldZwvNqda0o8ZsNiQAQ8MBq76fMUiemoDCpw";
    private List<Models.MovieData> nowPlayingMovieData = new List<Models.MovieData>();
    private Dictionary<Models.MovieData, Sprite> nowPlayingMovieSpriteDictionary = new Dictionary<Models.MovieData, Sprite>();
    private List<GameObject> spawnedButtons = new List<GameObject>();
    private string nowPlayingMinimumDate, nowPlayingMaximumDate;
    private float rotateSpeed = -600;
    private bool isLoaded = false;

    public int pageNumber, minPageNumber, maxPageNumber;
    public Sprite placeholder;
    public Button movieButtonPrefab, nextPageBtn, prevPageBtn;
    public Transform nowPlayingParent;
    public RectTransform loadingRectComponent;
    public GameObject loadingPage, moviePageUI;
    public List<GameObject> allPages;
    public event Action<NowPlayingDataWithImage> nowPlayingImageDownloaded;

    // Start is called before the first frame update
    void Start()
    {
        nextPageBtn.onClick.AddListener(() => LoadNextPage());
        prevPageBtn.onClick.AddListener(() => LoadPreviousPage());

        isLoaded = true;
        pageNumber = 1;
        GetNowPlayingDateData();
        StartCoroutine(GetNowPlayingMovies());
    }

    private void Update()
    {
        loadingRectComponent.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
    }

    private void OnEnable()
    {
        if (isLoaded)
        {
            foreach (GameObject go in allPages)
            {
                go.SetActive(false);
            }

            ResetUI();
        }
    }

    private void ResetUI()
    {
        moviePageUI.SetActive(false);
        if (spawnedButtons.Count > 0)
        {
            for (int i = 0; i < spawnedButtons.Count; i++)
            {
                Destroy(spawnedButtons[i]);
            }
        }
        pageNumber = 1;
        StartCoroutine(GetNowPlayingMovies());
    }

    private void LoadNextPage()
    {
        if (pageNumber < maxPageNumber)
        {
            for (int i = 0; i < spawnedButtons.Count; i++)
            {
                Destroy(spawnedButtons[i]);
            }

            pageNumber++;
            StartCoroutine(GetNowPlayingMovies());
        }
    }

    private void LoadPreviousPage()
    {
        if(pageNumber > minPageNumber)
        {
            for (int i = 0; i < spawnedButtons.Count; i++)
            {
                Destroy(spawnedButtons[i]);
            }

            pageNumber--;
            StartCoroutine(GetNowPlayingMovies());
        }
    }

    private void GetNowPlayingDateData()
    {
        DateTime exactDate = DateTime.Now;
        nowPlayingMaximumDate = exactDate.ToString("yyyy-MM-dd");

        // Calculate two months before
        DateTime twoMonthsLater = exactDate.AddMonths(-2);
        nowPlayingMinimumDate = twoMonthsLater.ToString("yyyy-MM-dd");
    }

    private void LoadNowPlayingUI()
    {
        loadingPage.SetActive(false);
        spawnedButtons.Clear();
        for (int i = 0; i < nowPlayingMovieData.Count; i++)
        {
            GameObject movieDataButton;
            movieDataButton = Instantiate(movieButtonPrefab.gameObject, nowPlayingParent);
            spawnedButtons.Add(movieDataButton);

            movieDataButton.GetComponent<MovieDataSetup>().Initialize(nowPlayingMovieData[i].name, nowPlayingMovieData[i].releaseDate, nowPlayingMovieData[i].movieRating, nowPlayingMovieData[i].movieID, placeholder, LoadMoviePageCallback);

            // Retrieve the associated sprite from the dictionary and update the button's image when available
            if (nowPlayingMovieSpriteDictionary.ContainsKey(nowPlayingMovieData[i]))
            {
                Sprite sprite = nowPlayingMovieSpriteDictionary[nowPlayingMovieData[i]];
                movieDataButton.GetComponent<MovieDataSetup>().SetImage(sprite);
            }

            // Start downloading the image for this movie data entry
            StartCoroutine(DownloadNowPlayingMoviesSpriteFromURL(nowPlayingMovieData[i].spriteURL, nowPlayingMovieData[i]));
        }
    }

    private void LoadMoviePageCallback(int movieID)
    {
        foreach (GameObject go in allPages)
        {
            go.SetActive(false);
        }

        PlayerPrefs.SetInt(Models.PlayerPrefsSaveKeys.MOVIE_ID.ToString(), movieID);
        moviePageUI.SetActive(true);
        gameObject.SetActive(false);
    }

    private IEnumerator GetNowPlayingMovies()
    {
        loadingPage.SetActive(true);
        using (UnityWebRequest www = UnityWebRequest.Get("https://api.themoviedb.org/3/discover/movie?include_adult=false&include_video=false&language=en-US&page=" + pageNumber + "&sort_by=popularity.desc&with_release_type=2|3&release_date.gte=" + nowPlayingMinimumDate + "&release_date.lte=" + nowPlayingMaximumDate))
        {
            www.SetRequestHeader("Authorization", "Bearer " + bearerToken);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(www.error);
                Debug.Log(www.downloadHandler.text);
            }
            else
            {
                Debug.Log("Get now playing Movies = Sucessfull");
                Debug.Log(www.downloadHandler.text);
            }

            JSONNode jsondata = JSON.Parse(System.Text.Encoding.UTF8.GetString(www.downloadHandler.data));
            nowPlayingMovieData.Clear();

            for (int i = 0; i < jsondata["results"].Count; i++)
            {
                Models.MovieData homepage = new Models.MovieData();
                homepage.spriteURL = jsondata["results"][i]["poster_path"];
                homepage.name = jsondata["results"][i]["title"];
                homepage.releaseDate = jsondata["results"][i]["release_date"];
                homepage.movieRating = jsondata["results"][i]["vote_average"];
                homepage.movieID = jsondata["results"][i]["id"];
                nowPlayingMovieData.Add(homepage);
            }

            if(jsondata["results"].Count > 0)
            {
                LoadNowPlayingUI();
            }
        }
    }

    private IEnumerator DownloadNowPlayingMoviesSpriteFromURL(string url, Models.MovieData data)
    {
        if (url == null)
        {
            // Add the placeholder sprite to the dictionary with the movie data as the key
            nowPlayingMovieSpriteDictionary[data] = placeholder;

            data.IsDownloaded = true;

            // Trigger the ImageDownloaded event with the data and image placeholder
            nowPlayingImageDownloaded?.Invoke(new NowPlayingDataWithImage(data, placeholder));
        }
        else
        {
            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture("https://image.tmdb.org/t/p/w500" + url))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("Download error: " + www.error);
                    Debug.LogError("Download response: " + www.downloadHandler.text);
                }
                else
                {
                    Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);

                    // Add the sprite to the dictionary with the movie data as the key
                    nowPlayingMovieSpriteDictionary[data] = sprite;

                    data.IsDownloaded = true;

                    // Trigger the ImageDownloaded event with the data and image
                    nowPlayingImageDownloaded?.Invoke(new NowPlayingDataWithImage(data, sprite));

                    // Update the corresponding button's image with the downloaded sprite
                    UpdateButtonImage(data, sprite);
                }

                // Check if all downloads are complete
                bool allDownloadsComplete = true;
                foreach (var item in nowPlayingMovieData)
                {
                    if (!item.IsDownloaded)
                    {
                        allDownloadsComplete = false;
                        break;
                    }
                }

                if (allDownloadsComplete)
                {
                    // All downloads are complete, you can continue with your logic here
                    Debug.Log("All now playing movies poster downloads are complete.");
                }
            }
        }
    }

    private void UpdateButtonImage(Models.MovieData data, Sprite sprite)
    {
        // Iterate through spawnedButtons
        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            GameObject button = spawnedButtons[i];

            // Check if the button is destroyed or null
            if (button == null)
            {
                continue; // Skip this button and continue with the next one
            }

            MovieDataSetup setup = button.GetComponent<MovieDataSetup>();

            if (setup != null && setup.movieID == data.movieID)
            {
                setup.SetImage(sprite);
                break; // Exit the loop once the button is found and updated
            }
        }
    }

    [Serializable]
    public class NowPlayingDataWithImage
    {
        public Models.MovieData Data { get; }
        public Sprite Image { get; }

        public NowPlayingDataWithImage(Models.MovieData data, Sprite image)
        {
            Data = data;
            Image = image;
        }
    }
}
