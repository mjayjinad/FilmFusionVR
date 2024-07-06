using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CastDataSetup : MonoBehaviour
{
    public TextMeshProUGUI castName, castCharacter;
    public Image castImage;

    public void Initialize(string name, string character, Sprite castSprite)
    {
        castName.text = name;
        castCharacter.text = character;
        castImage.sprite = castSprite;
    }
}
