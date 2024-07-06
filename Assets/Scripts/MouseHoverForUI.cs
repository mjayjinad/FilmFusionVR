using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MouseHoverForUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject[] objectToEnable;
    public GameObject[] objectToDisable;
     
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        if (objectToEnable != null)
        {
            foreach (GameObject go in objectToEnable)
            {
                go.SetActive(true);
            }
        }
        else
            return;

        if (objectToDisable != null)
        {
            foreach (GameObject go in objectToDisable)
            {
                go.SetActive(false);
            }
        }
        else
            return;
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        if (objectToEnable != null)
        {
            foreach (GameObject go in objectToEnable)
            {
                go.SetActive(false);
            }
        }
        else
            return;

        if (objectToDisable != null)
        {
            foreach (GameObject go in objectToDisable)
            {
                go.SetActive(true);
            }
        }
        else
            return;
    }
}


