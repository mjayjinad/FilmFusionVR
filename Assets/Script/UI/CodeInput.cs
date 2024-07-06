using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.IO;
using UnityEngine.UI;
using System.Linq;

public class CodeInput : MonoBehaviour
{
    public GameObject VRKeyboard;
    public TMP_InputField user_inputField;
    public int maxLength, minLength;
    private string maxUsername;

    public void EnableKeyboard()
    {
        VRKeyboard.SetActive(true);
    }

    public void DisableKeyboard()
    {
        VRKeyboard.SetActive(false);
    }

    //input number
    public void InsertInput(string alphabet)
    {
        InsertUsernameInput(alphabet);
    }

    private void InsertUsernameInput(string nameInput)
    {
        user_inputField.text = user_inputField.text + nameInput;

        if (user_inputField.text.Length == maxLength)
        {
            maxUsername = user_inputField.text;
        }

        if (user_inputField.text.Length > maxLength)
        {
            user_inputField.text = maxUsername;
        }
    }

    //Delete last number
    public void DeleteInput()
    {
        DeleteUsernameInput();
    }

    private void DeleteUsernameInput()
    {
        if (user_inputField.text.Length >= 1)
        {
            user_inputField.text = user_inputField.text.Remove(user_inputField.text.Length - 1);
        }
    }
}
