using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


public class TopRatedMoviesManager : MonoBehaviour
{
    private string bearerToken = "eyJhbGciOiJIUzI1NiJ9.eyJhdWQiOiI2Y2E2Mzk4ZjNhYTgzNzIyNGFkMTNhMWU0NmYxNzg1OSIsInN1YiI6IjY1MmI3NzQ4MzU4ZGE3MDBlM2JkYjQzYiIsInNjb3BlcyI6WyJhcGlfcmVhZCJdLCJ2ZXJzaW9uIjoxfQ.RJtry7TldZwvNqda0o8ZsNiQAQ8MBq76fMUiemoDCpw";
    private List<Models.MovieData> topRatedMovieData = new List<Models.MovieData>();
    private Dictionary<Models.MovieData, Sprite> topRatedMovieSpriteDictionary = new Dictionary<Models.MovieData, Sprite>();
    public List<GameObject> spawnedButtons = new List<GameObject>();
    private float rotateSpeed = -600;
    private bool isLoaded = false;

    public int pageNumber, minPageNumber, maxPageNumber;
    public float value;
    public Sprite placeholder;
    public Button movieButtonPrefab, prevPageBtn, nextPageBtn;
    public Transform topRatedparent;
    public RectTransform loadingRectComponent;
    public GameObject loadingPage, moviePageUI;
    public Scrollbar scrollbar;
    public List<GameObject> allPages;
    public event Action<TopRatedDataWithImage> topRatedImageDownloaded;

    // Start is called before the first frame update
    void Start()
    {
        prevPageBtn.onClick.AddListener(() => LoadPreviousPage());
        nextPageBtn.onClick.AddListener(() => LoadNextPage());

        isLoaded = true;
        pageNumber = 1;
        StartCoroutine(GetTopRatedMovies());
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

            value = 1f;
            ResetUI();
        }
    }

    private void OnDisable()
    {
        value = 0f;
    }

    private void ResetUI()
    {
        moviePageUI.SetActive(false);
        scrollbar.value = value;
        if (spawnedButtons.Count > 0)
        {
            for (int i = 0; i < spawnedButtons.Count; i++)
            {
                Destroy(spawnedButtons[i]);
            }
        }
        pageNumber = 1;
        StartCoroutine(GetTopRatedMovies());
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
            StartCoroutine(GetTopRatedMovies());
        }
    }

    private void LoadPreviousPage()
    {
        if (pageNumber > minPageNumber)
        {
            for (int i = 0; i < spawnedButtons.Count; i++)
            {
                Destroy(spawnedButtons[i]);
            }

            pageNumber--;
            StartCoroutine(GetTopRatedMovies());
        }
    }

    private void LoadTopRatedUI()
    {
        loadingPage.SetActive(false);
        spawnedButtons.Clear();
        for (int i = 0; i < topRatedMovieData.Count; i++)
        {
            GameObject movieDataButton;

            movieDataButton = Instantiate(movieButtonPrefab.gameObject, topRatedparent);
            spawnedButtons.Add(movieDataButton);

            movieDataButton.GetComponent<MovieDataSetup>().Initialize(topRatedMovieData[i].name, topRatedMovieData[i].releaseDate, topRatedMovieData[i].movieRating, topRatedMovieData[i].movieID, placeholder, LoadMoviePageCallback);

            // Retrieve the associated sprite from the dictionary and update the button's image when available
            if (topRatedMovieSpriteDictionary.ContainsKey(topRatedMovieData[i]))
            {
                Sprite sprite = topRatedMovieSpriteDictionary[topRatedMovieData[i]];
                movieDataButton.GetComponent<MovieDataSetup>().SetImage(sprite);
            }

            // Start downloading the image for this movie data entry
            StartCoroutine(DownloadTopRatedMoviesSpriteFromURL(topRatedMovieData[i].spriteURL, topRatedMovieData[i]));
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

    private IEnumerator GetTopRatedMovies()
    {
        loadingPage.SetActive(true);
        using (UnityWebRequest www = UnityWebRequest.Get("https://api.themoviedb.org/3/discover/movie?include_adult=false&include_video=false&language=en-US&page=" + pageNumber + "&sort_by=vote_average.desc&without_genres=99,10755&vote_count.gte=200"))
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
                Debug.Log("Get Top Rated Movies = Sucessfull");
                Debug.Log(www.downloadHandler.text);
            }

            JSONNode jsondata = JSON.Parse(System.Text.Encoding.UTF8.GetString(www.downloadHandler.data));
            topRatedMovieData.Clear();

            for (int i = 0; i < jsondata["results"].Count; i++)
            {
                Models.MovieData topRated = new Models.MovieData();
                topRated.spriteURL = jsondata["results"][i]["poster_path"];
                topRated.name = jsondata["results"][i]["title"];
                topRated.releaseDate = jsondata["results"][i]["release_date"];
                topRated.movieRating = jsondata["results"][i]["vote_average"];
                topRated.movieID = jsondata["results"][i]["id"];
                topRatedMovieData.Add(topRated);
            }

            if (jsondata["results"].Count > 0)
            {
                LoadTopRatedUI();
            }
        }
    }

    private IEnumerator DownloadTopRatedMoviesSpriteFromURL(string url, Models.MovieData data)
    {
        if (url == null)
        {
            // Add the placeholder sprite to the dictionary with the movie data as the key
            topRatedMovieSpriteDictionary[data] = placeholder;

            data.IsDownloaded = true;

            // Trigger the ImageDownloaded event with the data and image placeholder
            topRatedImageDownloaded?.Invoke(new TopRatedDataWithImage(data, placeholder));
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
                    topRatedMovieSpriteDictionary[data] = sprite;

                    data.IsDownloaded = true;

                    // Trigger the ImageDownloaded event with the data and image
                    topRatedImageDownloaded?.Invoke(new TopRatedDataWithImage(data, sprite));

                    // Update the corresponding button's image with the downloaded sprite
                    UpdateButtonImage(data, sprite);
                }

                // Check if all downloads are complete
                bool allDownloadsComplete = true;
                foreach (var item in topRatedMovieData)
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
                    Debug.Log("All Top Rated movie posters downloads are complete.");
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
    public class TopRatedDataWithImage
    {
        public Models.MovieData Data { get; }
        public Sprite Image { get; }

        public TopRatedDataWithImage(Models.MovieData data, Sprite image)
        {
            Data = data;
            Image = image;
        }
    }
}
