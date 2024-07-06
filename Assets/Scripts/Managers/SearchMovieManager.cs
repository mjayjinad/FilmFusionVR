using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static Models;

public class SearchMovieManager : MonoBehaviour
{
    private string bearerToken = "eyJhbGciOiJIUzI1NiJ9.eyJhdWQiOiI2Y2E2Mzk4ZjNhYTgzNzIyNGFkMTNhMWU0NmYxNzg1OSIsInN1YiI6IjY1MmI3NzQ4MzU4ZGE3MDBlM2JkYjQzYiIsInNjb3BlcyI6WyJhcGlfcmVhZCJdLCJ2ZXJzaW9uIjoxfQ.RJtry7TldZwvNqda0o8ZsNiQAQ8MBq76fMUiemoDCpw";
    private List<Models.SearchMovieData> searchResultMovieData = new List<Models.SearchMovieData>();
    private Dictionary<Models.SearchMovieData, Sprite> searchResultMovieSpriteDictionary = new Dictionary<Models.SearchMovieData, Sprite>();
    public List<GameObject> spawnedButtons = new List<GameObject>();
    private float rotateSpeed = -600;

    public int pageNumber, minPageNumber, maxPageNumber;
    public float value;
    public Sprite placeholder;
    public Button movieButtonPrefab, prevPageBtn, nextPageBtn, searchBtn1, searchBtn2;
    public Transform SearchParent;
    public RectTransform loadingRectComponent;
    public GameObject loadingPage, searchPageUI, errorReport, moviePageUI;
    public Scrollbar scrollbar;
    public TMP_InputField searchField1, searchField2;
    public List<GameObject> allPages;
    public event Action<SearchResultDataWithImage> searchResultImageDownloaded;

    // Start is called before the first frame update
    void Start()
    {
        prevPageBtn.onClick.AddListener(() => LoadPreviousPage());
        nextPageBtn.onClick.AddListener(() => LoadNextPage());
        searchBtn1.onClick.AddListener(() => RunCoroutine());
        searchBtn2.onClick.AddListener(() => RunCoroutine());


        // Attach a listener to InputField1 to synchronize text to InputField2
        searchField1.onValueChanged.AddListener(SyncInputField2);

        // Attach a listener to InputField2 to synchronize text to InputField1
        searchField2.onValueChanged.AddListener(SyncInputField1);
        pageNumber = 1;
    }

    private void Update()
    {
        loadingRectComponent.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
    }
    private void SyncInputField2(string newText)
    {
        searchField2.text = newText;
    }

    private void SyncInputField1(string newText)
    {
        searchField1.text = newText;
    }
    private void ActivateLoadingUI()
    {
        foreach (GameObject go in allPages)
        {
            go.SetActive(false);
        }

        searchPageUI.SetActive(true);
        loadingPage.SetActive(true);
        value = 1f;
    }

    private void OnDisable()
    {
        value = 0f;
    }

    public void RunCoroutine()
    {
        StartCoroutine(GetSearchResult());
    }

    private void ResetUI()
    {
        //scrollbar.value = value;
        if (spawnedButtons.Count > 0)
        {
            for (int i = 0; i < spawnedButtons.Count; i++)
            {
                Destroy(spawnedButtons[i]);
            }
        }
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
            Debug.Log("qwerty");
            StartCoroutine(GetSearchResult());
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
            StartCoroutine(GetSearchResult());
        }
    }

    private void LoadSearchResultUI()
    {
        loadingPage.SetActive(false);
        spawnedButtons.Clear();
        for (int i = 0; i < searchResultMovieData.Count; i++)
        {
            GameObject movieDataButton;

            //movieButtonPrefab.image.sprite = spawnButtonImage[buttonImageNumber];
            movieDataButton = Instantiate(movieButtonPrefab.gameObject, SearchParent);
            spawnedButtons.Add(movieDataButton);

            // Retrieve the associated sprite from the dictionary
            if (searchResultMovieSpriteDictionary.ContainsKey(searchResultMovieData[i]))
            {
                movieDataButton.GetComponent<SearchResultData>().Initialize(searchResultMovieData[i].name, searchResultMovieData[i].releaseDate, searchResultMovieData[i].description, searchResultMovieData[i].movieRating, searchResultMovieSpriteDictionary[searchResultMovieData[i]], searchResultMovieData[i].movieID, LoadMoviePageCallback);
            }
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
    }

    private IEnumerator GetSearchResult()
    {
        ResetUI();
        ActivateLoadingUI();
        if (searchField1.text != "")
        {
            string query = searchField1.text;
            using (UnityWebRequest www = UnityWebRequest.Get("https://api.themoviedb.org/3/search/movie?query=" + query + "&include_adult=false&language=en-US&page=" + pageNumber))
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
                    Debug.Log("Get Search Result = Sucessfull");
                    Debug.Log(www.downloadHandler.text);
                }

                JSONNode jsondata = JSON.Parse(System.Text.Encoding.UTF8.GetString(www.downloadHandler.data));
                searchResultMovieData.Clear();

                if (jsondata["total_results"] == 0)
                {
                    loadingPage.SetActive(false);
                    errorReport.SetActive(true);
                }
                else
                {
                    errorReport.SetActive(false);
                }

                for (int i = 0; i < jsondata["results"].Count; i++)
                {
                    Models.SearchMovieData searchPage = new Models.SearchMovieData();
                    searchPage.spriteURL = jsondata["results"][i]["poster_path"];
                    searchPage.name = jsondata["results"][i]["title"];
                    searchPage.description = jsondata["results"][i]["overview"];
                    searchPage.releaseDate = jsondata["results"][i]["release_date"];
                    searchPage.movieRating = jsondata["results"][i]["vote_average"];
                    searchPage.movieID = jsondata["results"][i]["id"];
                    searchResultMovieData.Add(searchPage);

                    StartCoroutine(DownloadSearchResultSpriteFromURL(jsondata["results"][i]["poster_path"], searchPage));
                }
            }
        }
    }

    private IEnumerator DownloadSearchResultSpriteFromURL(string url, Models.SearchMovieData data)
    {
        if (url == null)
        {
            // Add the placeholder sprite to the dictionary with the movie data as the key
            searchResultMovieSpriteDictionary[data] = placeholder;

            data.IsDownloaded = true;

            // Trigger the ImageDownloaded event with the data and image placeholder
            searchResultImageDownloaded?.Invoke(new SearchResultDataWithImage(data, placeholder));
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
                    searchResultMovieSpriteDictionary[data] = sprite;

                    data.IsDownloaded = true;

                    // Trigger the ImageDownloaded event with the data and image
                    searchResultImageDownloaded?.Invoke(new SearchResultDataWithImage(data, sprite));
                }

                // Check if all downloads are complete
                bool allDownloadsComplete = true;
                foreach (var item in searchResultMovieData)
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
                    Debug.Log("All search result movie posters downloads are complete.");

                    // After all downloads are complete, you can load your UI
                    LoadSearchResultUI();
                }
            }
        }
    }

    [Serializable]
    public class SearchResultDataWithImage
    {
        public Models.SearchMovieData Data { get; }
        public Sprite Image { get; }

        public SearchResultDataWithImage(Models.SearchMovieData data, Sprite image)
        {
            Data = data;
            Image = image;
        }
    }
}
