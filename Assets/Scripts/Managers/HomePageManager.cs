using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static Models;

public class HomePageManager : MonoBehaviour
{
    private string bearerToken = "eyJhbGciOiJIUzI1NiJ9.eyJhdWQiOiI2Y2E2Mzk4ZjNhYTgzNzIyNGFkMTNhMWU0NmYxNzg1OSIsInN1YiI6IjY1MmI3NzQ4MzU4ZGE3MDBlM2JkYjQzYiIsInNjb3BlcyI6WyJhcGlfcmVhZCJdLCJ2ZXJzaW9uIjoxfQ.RJtry7TldZwvNqda0o8ZsNiQAQ8MBq76fMUiemoDCpw";
    public List<Models.HomePage> homePageData = new List<Models.HomePage>();
    public List<Models.MovieData> popularMovieData = new List<Models.MovieData>();
    public List<Models.MovieData> nowPlayingMovieData = new List<Models.MovieData>();
    public List<Models.MovieData> upcomingMovieData = new List<Models.MovieData>();
    public List<Models.MovieData> topRatedMovieData = new List<Models.MovieData>();
    public Dictionary<Models.HomePage, Sprite> movieSpriteDictionary = new Dictionary<Models.HomePage, Sprite>();
    private Dictionary<Models.MovieData, Sprite> popularMovieSpriteDictionary = new Dictionary<Models.MovieData, Sprite>();
    private Dictionary<Models.MovieData, Sprite> nowPlayingMovieSpriteDictionary = new Dictionary<Models.MovieData, Sprite>();
    private Dictionary<Models.MovieData, Sprite> upcomingMovieSpriteDictionary = new Dictionary<Models.MovieData, Sprite>();
    private Dictionary<Models.MovieData, Sprite> topRatedMovieSpriteDictionary = new Dictionary<Models.MovieData, Sprite>();
    private string nowPlayingMinimumDate, nowPlayingMaximumDate, upcomingMinimumDate, upcomingMaximumDate;
    private float rotateSpeed = -600;

    public Button portfolio, movieButtonPrefab, nowplayingBtn, popularBtn, topRatedBtn, upcomingBtn, homeBtn;
    public RectTransform loadingRectComponent;
    public GameObject homePageUI, loadingPage, nowplayingUI, popularUI, topRatedUI, upcomingUI, moviePageUI;
    public Image backdropImage, headliner;
    public Sprite placeholder;
    public TextMeshProUGUI movieName, releaseDate, description;
    public Transform popularMoviesParent, nowPlayingParent, upcomingParent, topRatedparent;
    public List<GameObject> allPages;
    public event Action<DataWithImage> homePageImageDownloaded;
    public event Action<PopularDataWithImage> popularImageDownloaded;
    public event Action<NowPlayingDataWithImage> nowPlayingImageDownloaded;
    public event Action<UpcomingDataWithImage> upcomingImageDownloaded;
    public event Action<TopRatedDataWithImage> topRatedImageDownloaded;
    public List<GameObject> spawnedButtons = new List<GameObject>();


    // Start is called before the first frame update
    void Start()
    {
        portfolio.onClick.AddListener(() => OpenPortfolio());
        nowplayingBtn.onClick.AddListener(() => LoadNowPlayingPage());
        popularBtn.onClick.AddListener(() => LoadPopularPage());
        topRatedBtn.onClick.AddListener(() => LoadTopRatedPage());
        upcomingBtn.onClick.AddListener(() => LoadUpcomingPage());
        homeBtn.onClick.AddListener(() => LoadHomePage());

        GetNowPlayingDateData();
        GetUpcomingDateData();
        StartCoroutine(GetHomePopularMovies());
        StartCoroutine(GetPopularMovies());
        StartCoroutine(GetNowPlayingMovies());
        StartCoroutine(GetUpcomingMovies());
        StartCoroutine(GetTopRatedMovies());
    }

    private void Update()
    {
        loadingRectComponent.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
    }

    private void ResetUI()
    {
        if (spawnedButtons.Count > 0)
        {
            for (int i = 0; i < spawnedButtons.Count; i++)
            {
                Destroy(spawnedButtons[i]);
            }
            spawnedButtons.Clear();

            foreach (GameObject go in allPages)
            {
                go.SetActive(false);
            }
        }

        GetNowPlayingDateData();
        GetUpcomingDateData();
        StartCoroutine(GetHomePopularMovies());
        StartCoroutine(GetPopularMovies());
        StartCoroutine(GetNowPlayingMovies());
        StartCoroutine(GetUpcomingMovies());
        StartCoroutine(GetTopRatedMovies());
    }

    private void LoadHomePage()
    {
        foreach (GameObject go in allPages)
        {
            go.SetActive(false);
        }

        homePageUI.SetActive(true);
        ResetUI();
    }
    
    private void LoadNowPlayingPage()
    {
        foreach (GameObject go in allPages)
        {
            go.SetActive(false);
        }

        nowplayingUI.SetActive(true);
    }

    private void LoadPopularPage()
    {
        foreach (GameObject go in allPages)
        {
            go.SetActive(false);
        }

        popularUI.SetActive(true);
    }

    private void LoadTopRatedPage()
    {
        foreach (GameObject go in allPages)
        {
            go.SetActive(false);
        }

        topRatedUI.SetActive(true);
    }

    private void LoadUpcomingPage()
    {
        foreach (GameObject go in allPages)
        {
            go.SetActive(false);
        }

        upcomingUI.SetActive(true);
    }

    private void LoadMoviePageCallback(int movieID)
    {
        foreach (GameObject go in allPages)
        {
            go.SetActive(false);
        }

        PlayerPrefs.SetInt(Models.PlayerPrefsSaveKeys.MOVIE_ID.ToString(), movieID);
        moviePageUI.SetActive(true);
    }

    #region HOMEPAGE DATA

    public static void FormatDate(string date, TextMeshProUGUI releaseDate)
    {
        string inputDate = date;

        // Parse the input date string
        DateTime dateFormat = DateTime.ParseExact(inputDate, "yyyy-MM-dd", null);

        // Format the date as "dd MMMM yyyy" (e.g., "26 April 2023")
        string formattedDate = dateFormat.ToString("dd MMMM yyyy");

        releaseDate.text = "Release date: " + formattedDate;
        Console.WriteLine("QWERTYUIOP" + formattedDate);
    }

    private void LoadHomePageUI()
    {
        loadingPage.SetActive(false);
        homePageUI.SetActive(true);
        // Randomly select a movie to display
        int randomIndex = UnityEngine.Random.Range(0, homePageData.Count);
        int headlinerRandomIndex = UnityEngine.Random.Range(0, homePageData.Count);
        Models.HomePage selectedMovie = homePageData[randomIndex];
        Models.HomePage selectedHeadliner = homePageData[headlinerRandomIndex];

        // Load the UI elements with the selected movie's data
        movieName.text = selectedMovie.name;
        description.text = selectedMovie.description;

        FormatDate(selectedMovie.releaseDate, releaseDate);

        //StartCoroutine(LoadSpriteFromURL(homePageData[randomIndex].spriteURL, homePageData[randomIndex]));
        //StartCoroutine(LoadSpriteFromURL(homePageData[headlinerRandomIndex].spriteURL, homePageData[headlinerRandomIndex]));

        // Retrieve the associated sprite from the dictionary
        if (movieSpriteDictionary.ContainsKey(selectedMovie))
        {
            backdropImage.sprite = movieSpriteDictionary[selectedMovie];
        }

        // Retrieve the associated sprite from the dictionary
        if (movieSpriteDictionary.ContainsKey(selectedHeadliner))
        {
            headliner.sprite = movieSpriteDictionary[selectedHeadliner];
        }
    }

    private IEnumerator GetHomePopularMovies()
    {
        loadingPage.SetActive(true);
        int pageNumber = UnityEngine.Random.Range(1, 20);
        using (UnityWebRequest www = UnityWebRequest.Get("https://api.themoviedb.org/3/discover/movie?include_adult=false&include_video=false&language=en-US&page=" + pageNumber + "&sort_by=popularity.desc"))
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
                Debug.Log("Get Home Popular Movies = Sucessfull");
                Debug.Log(www.downloadHandler.text);
            }

            JSONNode jsondata = JSON.Parse(System.Text.Encoding.UTF8.GetString(www.downloadHandler.data));
            homePageData.Clear();

            for (int i = 0; i < jsondata["results"].Count; i++)
            {
                Models.HomePage homepage = new Models.HomePage();
                homepage.spriteURL = jsondata["results"][i]["backdrop_path"];
                homepage.name = jsondata["results"][i]["title"];
                homepage.releaseDate = jsondata["results"][i]["release_date"];
                homepage.description = jsondata["results"][i]["overview"];
                homePageData.Add(homepage);

                StartCoroutine(LoadSpriteFromURL(jsondata["results"][i]["backdrop_path"], homepage));
            }

            if (jsondata["results"].Count > 0)
            {
                LoadHomePageUI();
            }
        }
    }

    private IEnumerator LoadSpriteFromURL(string url, Models.HomePage data)
    {
        if (url == null)
        {
            // Add the placeholder sprite to the dictionary with the movie data as the key
            movieSpriteDictionary[data] = placeholder;

            data.IsDownloaded = true;

            // Trigger the ImageDownloaded event with the data and image placeholder
            homePageImageDownloaded?.Invoke(new DataWithImage(data, placeholder));
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
                    movieSpriteDictionary[data] = sprite;

                    data.IsDownloaded = true;

                    // Trigger the ImageDownloaded event with the data and image
                    homePageImageDownloaded?.Invoke(new DataWithImage(data, sprite));

                    // Update the corresponding button's image with the downloaded sprite
                    UpdateButtonImage(data, sprite);
                }

                // Check if all downloads are complete
                bool allDownloadsComplete = true;
                foreach (var item in homePageData)
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
                    Debug.Log("All downloads are complete.");
                    LoadHomePageUI();
                }
            }
        }
    }

    private void UpdateButtonImage(Models.HomePage data, Sprite sprite)
    {
        // Iterate through spawnedButtons
        for (int i = 0; i < homePageData.Count; i++)
        {
            HomePage homeData = homePageData[i];

            // Check if the button is destroyed or null
            if (homeData == null)
            {
                continue; // Skip this button and continue with the next one
            }

            if (homePageData[i].movieID == data.movieID)
            {
                homePageData[i].backdropSprite = sprite;
                break; // Exit the loop once the button is found and updated
            }
        }
    }

    public void OpenPortfolio()
    {
        Application.OpenURL("https://abdulmaliqjinad.swiftxr.app/");
    }

    [Serializable]
    public class DataWithImage
    {
        public Models.HomePage Data { get; }
        public Sprite Image { get; }

        public DataWithImage(Models.HomePage data, Sprite image)
        {
            Data = data;
            Image = image;
        }
    }
    #endregion

    #region POPULAR MOVIES

    private void LoadPopularMoviesUI()
    {
        for (int i = 0; i < popularMovieData.Count; i++)
        {
            GameObject movieDataButton;

            movieDataButton = Instantiate(movieButtonPrefab.gameObject, popularMoviesParent);
            spawnedButtons.Add(movieDataButton);

            movieDataButton.GetComponent<MovieDataSetup>().Initialize(popularMovieData[i].name, popularMovieData[i].releaseDate, popularMovieData[i].movieRating, popularMovieData[i].movieID, placeholder, LoadMoviePageCallback);

            // Retrieve the associated sprite from the dictionary and update the button's image when available
            if (popularMovieSpriteDictionary.ContainsKey(popularMovieData[i]))
            {
                Sprite sprite = popularMovieSpriteDictionary[popularMovieData[i]];
                movieDataButton.GetComponent<MovieDataSetup>().SetImage(sprite);
            }

            // Start downloading the image for this movie data entry
            StartCoroutine(DownloadPopularMoviesSpriteFromURL(popularMovieData[i].spriteURL, popularMovieData[i]));
        }
    }

    private IEnumerator GetPopularMovies()
    {
        int pageNumber = UnityEngine.Random.Range(1, 20);
        using (UnityWebRequest www = UnityWebRequest.Get("https://api.themoviedb.org/3/discover/movie?include_adult=false&include_video=false&language=en-US&page=" + pageNumber + "& sort_by=popularity.desc"))
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
                Debug.Log("Get Popular Movies = Sucessfull");
                Debug.Log(www.downloadHandler.text);
            }

            JSONNode jsondata = JSON.Parse(System.Text.Encoding.UTF8.GetString(www.downloadHandler.data));
            popularMovieData.Clear();

            for (int i = 0; i < jsondata["results"].Count; i++)
            {
                Models.MovieData homepage = new Models.MovieData();
                homepage.spriteURL = jsondata["results"][i]["poster_path"];
                homepage.name = jsondata["results"][i]["title"];
                homepage.releaseDate = jsondata["results"][i]["release_date"];
                homepage.movieRating = jsondata["results"][i]["vote_average"];
                homepage.movieID = jsondata["results"][i]["id"];
                popularMovieData.Add(homepage);
            }

            if(jsondata["results"].Count > 0)
            {
                LoadPopularMoviesUI();
            }
        }
    }

    private IEnumerator DownloadPopularMoviesSpriteFromURL(string url, Models.MovieData data)
    {
        if (url == null)
        {
            // Add the placeholder sprite to the dictionary with the movie data as the key
            popularMovieSpriteDictionary[data] = placeholder;

            data.IsDownloaded = true;

            // Trigger the ImageDownloaded event with the data and image placeholder
            popularImageDownloaded?.Invoke(new PopularDataWithImage(data, placeholder));
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
                    popularMovieSpriteDictionary[data] = sprite;

                    data.IsDownloaded = true;

                    // Trigger the ImageDownloaded event with the data and image
                    popularImageDownloaded?.Invoke(new PopularDataWithImage(data, sprite));

                    PopularButtonImageUpdate(data, sprite);
                }

                // Check if all downloads are complete
                bool allDownloadsComplete = true;
                foreach (var item in popularMovieData)
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
                    Debug.Log("All popular movie posters downloads are complete.");
                }
            }
        }
    }

    private void PopularButtonImageUpdate(Models.MovieData data, Sprite sprite)
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
    public class PopularDataWithImage
    {
        public Models.MovieData Data { get; }
        public Sprite Image { get; }

        public PopularDataWithImage(Models.MovieData data, Sprite image)
        {
            Data = data;
            Image = image;
        }
    }

    #endregion

    #region NOW PLAYING

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

    private IEnumerator GetNowPlayingMovies()
    {
        int pageNumber = UnityEngine.Random.Range(1, 20);
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

                    NowPlayingButtonImageUpdate(data, sprite);
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

    private void NowPlayingButtonImageUpdate(Models.MovieData data, Sprite sprite)
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
    #endregion

    #region UPCOMING MOVIES


    private void GetUpcomingDateData()
    {
        DateTime exactDate = DateTime.Now;
        exactDate.AddDays(1);
        upcomingMinimumDate = exactDate.ToString("yyyy-MM-dd");

        // Calculate two months later
        DateTime twoMonthsLater = exactDate.AddMonths(2);
        upcomingMaximumDate = twoMonthsLater.ToString("yyyy-MM-dd");
    }

    private void LoadUpcomingMoviesUI()
    {
        for (int i = 0; i < upcomingMovieData.Count; i++)
        {
            GameObject movieDataButton;

            movieDataButton = Instantiate(movieButtonPrefab.gameObject, upcomingParent);
            spawnedButtons.Add(movieDataButton);

            movieDataButton.GetComponent<MovieDataSetup>().Initialize(upcomingMovieData[i].name, upcomingMovieData[i].releaseDate, upcomingMovieData[i].movieRating, upcomingMovieData[i].movieID, placeholder, LoadMoviePageCallback);

            // Retrieve the associated sprite from the dictionary and update the button's image when available
            if (upcomingMovieSpriteDictionary.ContainsKey(upcomingMovieData[i]))
            {
                Sprite sprite = upcomingMovieSpriteDictionary[upcomingMovieData[i]];
                movieDataButton.GetComponent<MovieDataSetup>().SetImage(sprite);
            }

            // Start downloading the image for this movie data entry
            StartCoroutine(DownloadUpcomingMoviesSpriteFromURL(upcomingMovieData[i].spriteURL, upcomingMovieData[i]));
        }
    }

    private IEnumerator GetUpcomingMovies()
    {
        int pageNumber = UnityEngine.Random.Range(1, 20);
        using (UnityWebRequest www = UnityWebRequest.Get("https://api.themoviedb.org/3/discover/movie?include_adult=false&include_video=false&language=en-US&page=" + pageNumber + "&sort_by=popularity.desc&with_release_type=3|2&release_date.gte=" + upcomingMinimumDate + "&release_date.lte=" + upcomingMaximumDate))
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
                Debug.Log("Get Upcoming Movies = Sucessfull");
                Debug.Log(www.downloadHandler.text);
            }

            JSONNode jsondata = JSON.Parse(System.Text.Encoding.UTF8.GetString(www.downloadHandler.data));
            upcomingMovieData.Clear();

            for (int i = 0; i < jsondata["results"].Count; i++)
            {
                Models.MovieData homepage = new Models.MovieData();
                homepage.spriteURL = jsondata["results"][i]["poster_path"];
                homepage.name = jsondata["results"][i]["title"];
                homepage.releaseDate = jsondata["results"][i]["release_date"];
                homepage.movieRating = jsondata["results"][i]["vote_average"];
                homepage.movieID = jsondata["results"][i]["id"];
                upcomingMovieData.Add(homepage);
            }

            if(jsondata["results"].Count > 0)
            {
                LoadUpcomingMoviesUI();
            }
        }
    }

    private IEnumerator DownloadUpcomingMoviesSpriteFromURL(string url, Models.MovieData data)
    {
        if (url == null)
        {
            // Add the placeholder sprite to the dictionary with the movie data as the key
            upcomingMovieSpriteDictionary[data] = placeholder;

            data.IsDownloaded = true;

            // Trigger the ImageDownloaded event with the data and image placeholder
            upcomingImageDownloaded?.Invoke(new UpcomingDataWithImage(data, placeholder));
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
                    upcomingMovieSpriteDictionary[data] = sprite;

                    data.IsDownloaded = true;

                    // Trigger the ImageDownloaded event with the data and image
                    upcomingImageDownloaded?.Invoke(new UpcomingDataWithImage(data, sprite));
                    UpcomingButtonImageUpdate(data, sprite);
                }

                // Check if all downloads are complete
                bool allDownloadsComplete = true;
                foreach (var item in upcomingMovieData)
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
                    Debug.Log("All upcoming movie posters downloads are complete.");
                }
            }
        }
    }

    private void UpcomingButtonImageUpdate(Models.MovieData data, Sprite sprite)
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
    public class UpcomingDataWithImage
    {
        public Models.MovieData Data { get; }
        public Sprite Image { get; }

        public UpcomingDataWithImage(Models.MovieData data, Sprite image)
        {
            Data = data;
            Image = image;
        }
    }

    #endregion

    #region TOP RATED

    private void LoadTopRatedUI()
    {
        for (int i = 0; i < topRatedMovieData.Count; i++)
        {
            GameObject movieDataButton;

            //movieButtonPrefab.image.sprite = spawnButtonImage[buttonImageNumber];
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

    private IEnumerator GetTopRatedMovies()
    {
        int pageNumber = UnityEngine.Random.Range(1, 20);
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
                Models.MovieData homepage = new Models.MovieData();
                homepage.spriteURL = jsondata["results"][i]["poster_path"];
                homepage.name = jsondata["results"][i]["title"];
                homepage.releaseDate = jsondata["results"][i]["release_date"];
                homepage.movieRating = jsondata["results"][i]["vote_average"];
                homepage.movieID = jsondata["results"][i]["id"];
                topRatedMovieData.Add(homepage);
            }

            if(jsondata["results"].Count > 0)
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
                    TopRatedButtonImageUpdate(data, sprite);
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
    private void TopRatedButtonImageUpdate(Models.MovieData data, Sprite sprite)
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

    #endregion
}
