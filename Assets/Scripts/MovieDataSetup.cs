using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MovieDataSetup : MonoBehaviour
{
    public TextMeshProUGUI movieTitle, releaseDate, rating;
    public Image poster, ratingProgress;
    public Button movieButton;
    public int movieID;

    public void Initialize(string name, string date, float ratingScore, int _movieID, Sprite posterSprite, Action<int> callback)
    {
        float ratingPercentage = (ratingScore / 10 * 100);
        float calculatedFillAmount = ratingPercentage / 100;
        int formattedRate = (int)ratingPercentage;
        rating.text = formattedRate.ToString();
        ratingProgress.fillAmount = calculatedFillAmount;
        movieTitle.text = name;
        releaseDate.text = date;
        poster.sprite = posterSprite;
        FormatDate(releaseDate.text, releaseDate);
        movieButton.onClick.AddListener(() => callback?.Invoke(_movieID));
        movieID = _movieID;
    }

    public void SetImage(Sprite sprite)
    {
        poster.sprite = sprite;
    }

    public static void FormatDate(string date, TextMeshProUGUI releaseDate)
    {
        if (date != "")
        {
            string inputDate = date;

            // Parse the input date string
            DateTime dateFormat = DateTime.ParseExact(inputDate, "yyyy-MM-dd", null);

            // Format the date as "dd MMMM yyyy" (e.g., "26 Apr 2023")
            string formattedDate = dateFormat.ToString("dd MMM yyyy");

            releaseDate.text = formattedDate;
        }
    }
}
