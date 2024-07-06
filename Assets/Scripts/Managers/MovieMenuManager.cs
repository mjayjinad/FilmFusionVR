using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static Models;

public class MovieMenuManager : MonoBehaviour
{
    private string bearerToken = "eyJhbGciOiJIUzI1NiJ9.eyJhdWQiOiI2Y2E2Mzk4ZjNhYTgzNzIyNGFkMTNhMWU0NmYxNzg1OSIsInN1YiI6IjY1MmI3NzQ4MzU4ZGE3MDBlM2JkYjQzYiIsInNjb3BlcyI6WyJhcGlfcmVhZCJdLCJ2ZXJzaW9uIjoxfQ.RJtry7TldZwvNqda0o8ZsNiQAQ8MBq76fMUiemoDCpw";
    public Models.MovieIDData movieIDData;
    public List<Models.CastData> movieCastData = new List<Models.CastData>();
    public List<Models.VideoData> movieVideoData = new List<Models.VideoData>();
    private Dictionary<Models.MovieIDData, Sprite> movieIDSpriteDictionary = new Dictionary<Models.MovieIDData, Sprite>();
    private Dictionary<Models.CastData, Sprite> castDataSpriteDictionary = new Dictionary<Models.CastData, Sprite>();
    private List<GameObject> spawnedCast = new List<GameObject>();
    public List<GameObject> spawnedVideo = new List<GameObject>();
    private float rotateSpeed = -600;
    private string releaseDate, runtime;
    private bool isloaded;

    public TextMeshProUGUI title, description, tagline, detailsTxt, rating, revenue, budget, status;
    public Button traillerBtn, closeBtn;
    public Image ratingProgress, poster, backdrop;
    public Sprite placeholder;
    public Transform movieCastParent, videoParent;
    public RectTransform loadingRectComponent;
    public GameObject loadingPage, moviePageUI, castUIPrefab, videoUIPrefab, trailerVideoUI;
    public string trailerKey, trailerName;
    public List<GameObject> allPages;
    public event Action<MovieIDDataWithImage> movieIDImageDownloaded;
    public event Action<CastDataWithImage> castDataImageDownloaded;

    // Start is called before the first frame update
    void Start()
    {
        traillerBtn.onClick.AddListener(() => GetTraillerKey());
        closeBtn.onClick.AddListener(() => trailerVideoUI.SetActive(false));

        StartCoroutine(GetMoviesData());
        StartCoroutine(GetCastDetails());
        StartCoroutine(GetVideoKey());
        isloaded = true;
    }

    private void Update()
    {
        loadingRectComponent.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
    }

    private void OnEnable()
    {
        if (isloaded)
        {
            ResetUI();
            StartCoroutine(GetMoviesData());
            StartCoroutine(GetCastDetails());
            StartCoroutine(GetVideoKey());
        }
    }

    private void ResetUI()
    {
        if (spawnedCast.Count > 0)
        {
            for (int i = 0; i < spawnedCast.Count; i++)
            {
                Destroy(spawnedCast[i]);
            }
        }

        if (spawnedVideo.Count > 0)
        {
            for (int i = 0; i < spawnedVideo.Count; i++)
            {
                Destroy(spawnedVideo[i]);
            }
        }
    }

    public void FormatDate(string date)
    {
        if (date != "")
        {
            // Parse the input date string
            DateTime dateFormat = DateTime.ParseExact(date, "yyyy-MM-dd", null);

            // Format the date as "dd MMMM yyyy" (e.g., "26 Apr 2023")
            string formattedDate = dateFormat.ToString("dd MMM yyyy");

            releaseDate = formattedDate;
        }
    }

    private void FormatTime(float time)
    {
        TimeSpan timeSpan = TimeSpan.FromMinutes(time);
        string formattedTime = string.Format("{0:D2}h {1:D2}m", timeSpan.Hours, timeSpan.Minutes);
        runtime = formattedTime;
    }

    private void GetTraillerKey()
    {
        trailerKey = GetFirstTrailerKey();
        trailerVideoUI.SetActive(true);
    }

    private void LoadMoviesPageUI()
    {
        FormatTime(movieIDData.runtime);
        FormatDate(movieIDData.release_date);
        loadingPage.SetActive(false);
        title.text = movieIDData.title;
        description.text = movieIDData.overview;
        tagline.text = movieIDData.tagline;
        status.text = "Status: " + movieIDData.status;
        string formattedBudget = string.Format("${0:#,0}", movieIDData.budget);
        budget.text = "Budget: " + formattedBudget;
        string formattedRevenue = string.Format("${0:#,0}", movieIDData.revenue);
        revenue.text = "Revenue: " + formattedRevenue;
        float ratingPercentage = (movieIDData.vote_average / 10 * 100);
        float calculatedFillAmount = ratingPercentage / 100;
        int formattedRate = (int)ratingPercentage;
        rating.text = formattedRate.ToString();
        ratingProgress.fillAmount = calculatedFillAmount;

        // Convert the list of genre names to a single string
        string genresString = string.Join(", ", movieIDData.genres);
        detailsTxt.text = releaseDate + " . " + runtime + " . " + genresString;

        LoadCastDataUI();
        LoadVideoUI();
    }

    private void LoadCastDataUI()
    {
        spawnedCast.Clear();
        for (int i = 0; i < movieCastData.Count; i++)
        {
            GameObject movieDataButton;

            movieDataButton = Instantiate(castUIPrefab, movieCastParent);
            spawnedCast.Add(movieDataButton);

            // Retrieve the associated sprite from the dictionary
            if (castDataSpriteDictionary.ContainsKey(movieCastData[i]))
            {
                movieDataButton.GetComponent<CastDataSetup>().Initialize(movieCastData[i].name, movieCastData[i].character, castDataSpriteDictionary[movieCastData[i]]);
            }
        }
    }
    
    private void LoadVideoUI()
    {
        spawnedVideo.Clear();
        for (int i = 0; i < movieVideoData.Count; i++)
        {
            GameObject videoDataUI;

            videoDataUI = Instantiate(videoUIPrefab, videoParent);
            spawnedVideo.Add(videoDataUI);

            videoDataUI.GetComponent<VideoManager>().Initialize(movieVideoData[i].name, movieVideoData[i].youtubeKey);
        }
    }

    #region MOVIEDATA

    private IEnumerator GetMoviesData()
    {
        int movieID = PlayerPrefs.GetInt(Models.PlayerPrefsSaveKeys.MOVIE_ID.ToString());
        loadingPage.SetActive(true);
        using (UnityWebRequest www = UnityWebRequest.Get("https://api.themoviedb.org/3/movie/" + movieID + "?language=en-US"))
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
                Debug.Log("Get Movie Data = Sucessfull");
                Debug.Log(www.downloadHandler.text);
            }

            JSONNode jsondata = JSON.Parse(System.Text.Encoding.UTF8.GetString(www.downloadHandler.data));

            Models.MovieIDData homepage = new Models.MovieIDData();
            homepage.title = jsondata["title"];
            homepage.release_date = jsondata["release_date"];
            homepage.vote_average = jsondata["vote_average"];
            homepage.tagline = jsondata["tagline"];
            homepage.overview = jsondata["overview"];
            homepage.status = jsondata["status"];
            homepage.runtime = jsondata["runtime"];
            homepage.revenue = jsondata["revenue"];
            homepage.budget = jsondata["budget"];

            homepage.imageLink = new List<string>
            {
                jsondata["poster_path"],
                jsondata["backdrop_path"]
            };

            homepage.genres = new List<string>();
            for (int i = 0; i < jsondata["genres"].Count; i++)
            {
                homepage.genres.Add(jsondata["genres"][i]["name"]);
            }

            movieIDData = homepage;

            for (int i = 0; i < homepage.imageLink.Count; i++)
            {
                StartCoroutine(DownloadPosterImage(homepage.imageLink[i], homepage));
            }
        }
    }

    private IEnumerator DownloadPosterImage(string url, Models.MovieIDData data)
    {
        if (url == null)
        {
            // Add the placeholder sprite to the dictionary with the movie data as the key
            movieIDSpriteDictionary[data] = placeholder;

            data.IsDownloaded = true;

            // Trigger the ImageDownloaded event with the data and image placeholder
            movieIDImageDownloaded?.Invoke(new MovieIDDataWithImage(data, placeholder));
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
                    movieIDSpriteDictionary[data] = sprite;

                    data.IsDownloaded = true;

                    // Trigger the ImageDownloaded event with the data and image
                    movieIDImageDownloaded?.Invoke(new MovieIDDataWithImage(data, sprite));

                    // Check if this is the poster or backdrop image and set the UI accordingly
                    if (data.imageLink[0] == url)
                    {
                        // This is the poster image
                        poster.sprite = sprite;
                    }
                    else if (data.imageLink[1] == url)
                    {
                        // This is the backdrop image
                        backdrop.sprite = sprite;
                        Debug.Log("Movie poster and backdrop downloads are complete.");
                    }
                }
            }
        }
    }

    [Serializable]
    public class MovieIDDataWithImage
    {
        public MovieIDData Data { get; }
        public Sprite Image { get; }

        public MovieIDDataWithImage(MovieIDData data, Sprite image)
        {
            Data = data;
            Image = image;
        }
    }

    #endregion

    #region CAST

    private IEnumerator GetCastDetails()
    {
        int movieID = PlayerPrefs.GetInt(Models.PlayerPrefsSaveKeys.MOVIE_ID.ToString());
        loadingPage.SetActive(true);
        using (UnityWebRequest www = UnityWebRequest.Get("https://api.themoviedb.org/3/movie/" + movieID + "/credits?language=en-US"))
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
                Debug.Log("Get cast details = Sucessfull");
                Debug.Log(www.downloadHandler.text);
            }

            JSONNode jsondata = JSON.Parse(System.Text.Encoding.UTF8.GetString(www.downloadHandler.data));
            movieCastData.Clear();

            for (int i = 0; i < 10; i++)
            {
                Models.CastData castData = new Models.CastData();
                castData.name = jsondata["cast"][i]["name"];
                castData.character = jsondata["cast"][i]["character"];
                castData.profile_path = jsondata["cast"][i]["profile_path"];
                movieCastData.Add(castData);

                StartCoroutine(DownloadCastImage(jsondata["cast"][i]["profile_path"], castData));
            }
        }
    }
    private IEnumerator DownloadCastImage(string url, Models.CastData data)
    {
        if (url == null)
        {
            // Add the placeholder sprite to the dictionary with the movie data as the key
            castDataSpriteDictionary[data] = placeholder;

            data.IsDownloaded = true;

            // Trigger the ImageDownloaded event with the data and image placeholder
            castDataImageDownloaded?.Invoke(new CastDataWithImage(data, placeholder));
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
                    castDataSpriteDictionary[data] = sprite;

                    data.IsDownloaded = true;

                    // Trigger the ImageDownloaded event with the data and image
                    castDataImageDownloaded?.Invoke(new CastDataWithImage(data, sprite));
                }

                // Check if all downloads are complete
                bool allDownloadsComplete = true;
                foreach (var item in movieCastData)
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
                    Debug.Log("All cast downloads are complete.");

                    // After all downloads are complete, you can load your UI
                    LoadMoviesPageUI();
                }
            }
        }
    }

    [Serializable]
    public class CastDataWithImage
    {
        public Models.CastData Data { get; }
        public Sprite Image { get; }

        public CastDataWithImage(Models.CastData data, Sprite image)
        {
            Data = data;
            Image = image;
        }
    }

    #endregion

    #region VIDEO

    private IEnumerator GetVideoKey()
    {
        int movieID = PlayerPrefs.GetInt(Models.PlayerPrefsSaveKeys.MOVIE_ID.ToString());
        //loadingPage.SetActive(true);
        using (UnityWebRequest www = UnityWebRequest.Get("https://api.themoviedb.org/3/movie/" + movieID + "/videos?language=en-US"))
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
                Debug.Log("Get videoe key = Sucessfull");
                Debug.Log(www.downloadHandler.text);
            }

            JSONNode jsondata = JSON.Parse(System.Text.Encoding.UTF8.GetString(www.downloadHandler.data));
             movieVideoData.Clear();

            for (int i = 0; i < jsondata["results"].Count; i++)
            {
                Models.VideoData videoData = new Models.VideoData();
                videoData.youtubeKey = jsondata["results"][i]["key"];
                videoData.name = jsondata["results"][i]["name"];
                videoData.type = jsondata["results"][i]["type"];
                movieVideoData.Add(videoData);
            }

            GetFirstTrailerKey();
        }
    }

    private string GetFirstTrailerKey()
    {
        Models.VideoData firstTrailer = movieVideoData.FirstOrDefault(video => video.type == "Trailer");

        if (firstTrailer != null)
        {
            trailerName = firstTrailer.name;
            return firstTrailer.youtubeKey;
        }
        else
        {
            return null;
        }
    }

    #endregion
}
