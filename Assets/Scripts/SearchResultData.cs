using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Models;

public class SearchResultData : MonoBehaviour
{
    public TextMeshProUGUI movieTitle, releaseDate, descriptionTxt, rating;
    public Image poster, ratingProgress;
    public Button movieButton;


    public void Initialize(string name, string date, string description, float ratingScore, Sprite posterSprite, int movieID, Action<int> callback)
    {
        float ratingPercentage = (ratingScore / 10 * 100);
        float calculatedFillAmount = ratingPercentage / 100;
        int formattedRate = (int)ratingPercentage;
        rating.text = formattedRate.ToString();
        ratingProgress.fillAmount = calculatedFillAmount;
        movieTitle.text = name;
        releaseDate.text = date;
        descriptionTxt.text = description;
        poster.sprite = posterSprite;
        FormatDate(releaseDate.text, releaseDate);
        movieButton.onClick.AddListener(() => callback?.Invoke(movieID));
    }

    public static void FormatDate(string date, TextMeshProUGUI releaseDate)
    {
        if(date != "")
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
