using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Models : MonoBehaviour
{
   
    public enum PlayerPrefsSaveKeys
    {
        MOVIE_ID
    }

    [Serializable]
    public class HomePage
    {
        public string name;
        public string description;
        public string releaseDate;
        public string spriteURL;
        public int movieID;
        public Sprite backdropSprite;
        public bool IsDownloaded { get; set; } = false;
    }

    [Serializable]
    public class MovieData
    {
        public string name;
        public string releaseDate;
        public string spriteURL;
        public float movieRating;
        public int movieID;
        public Sprite posterImage;
        public bool IsDownloaded { get; set; } = false;
    }
    
    [Serializable]
    public class MovieIDData
    {
        public List<string> imageLink;
        public List<string> genres;
        public string title;
        public string release_date;
        public string tagline;
        public string overview;
        public float vote_average;
        public float runtime;
        public int revenue;
        public int budget;
        public string status;
        public bool IsDownloaded { get; set; } = false;
    }

    [Serializable]
    public class SearchMovieData
    {
        public string name;
        public string releaseDate;
        public string description;
        public string spriteURL;
        public float movieRating;
        public int movieID;
        public bool IsDownloaded { get; set; } = false;
    }

    [Serializable]
    public class CastData
    {
        public string name;
        public string character;
        public string profile_path;
        public bool IsDownloaded { get; set; } = false;
    }

    [Serializable]
    public class VideoData
    {
        public string name;
        public string youtubeKey;
        public string type;
        public bool IsDownloaded { get; set; } = false;
    }
}
