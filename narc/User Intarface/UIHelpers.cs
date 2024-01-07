// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using UnityEngine.UI;

public class UIHelpers : MonoBehaviour {

    // TODO: this really isn't neccessary
    public static  GameObject CreateButton(Transform panel, Vector3 position, Vector2 size, UnityEngine.Events.UnityAction method, Sprite image, string text = "")
    {
        GameObject button = new GameObject();

        button.transform.parent = panel;
        button.layer = 5;
        RectTransform rectt = button.AddComponent<RectTransform>();
        Button buttonC = button.AddComponent<Button>();
        button.AddComponent<Image>();
        //button.transform.position = position;

        rectt.anchorMin = new Vector2(0f, 1f);
        rectt.anchorMax = new Vector2(0f, 1f);
        rectt.anchoredPosition = position;
        //rectt.
        rectt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
        rectt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);

        Button.ButtonClickedEvent bce = new Button.ButtonClickedEvent();
        bce.AddListener(method);
        buttonC.onClick = bce;

        ColorBlock cols = buttonC.colors;
        cols.highlightedColor = new Color(0.8f,0.8f,0.8f);
        cols.pressedColor = new Color(0.6f, 0.6f, 0.6f);
        buttonC.colors = cols;

        Image img = button.GetComponent<Image>();
        //img.color = new Color(1f,1f,1f,0f);
        img.sprite = image;
        buttonC.targetGraphic = button.GetComponent<Image>();

        if (text != "")
        {
            GameObject buttonText = new GameObject();
            buttonText.transform.SetParent(button.transform);
            buttonText.layer = 5;
            RectTransform trectt = buttonText.AddComponent<RectTransform>();
            trectt.anchorMin = new Vector2(0f, 1f);
            trectt.anchorMax = new Vector2(0f, 1f);
            trectt.anchoredPosition = new Vector2(size.x / 2f, -size.y / 1.6f);
            //rectt.
            trectt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            trectt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
            Text buttonCText = buttonText.AddComponent<Text>();
            buttonCText.font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
            buttonCText.text = text;
            buttonCText.color = Color.white;
            buttonCText.resizeTextForBestFit = true;

            buttonCText.alignment = TextAnchor.MiddleCenter;
        }
        return button;
    }

    public static GameObject CreateImage(Transform panel, Vector3 position, Vector2 size, Sprite image)
    {
        GameObject sprite = new GameObject();

        sprite.transform.parent = panel;
        sprite.layer = 5;
        RectTransform rectt = sprite.AddComponent<RectTransform>();
        sprite.AddComponent<Image>();

        rectt.anchorMin = new Vector2(0f, 1f);
        rectt.anchorMax = new Vector2(0f, 1f);
        rectt.anchoredPosition = position;
        rectt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
        rectt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);

        sprite.GetComponent<Image>().sprite = image;
        return sprite;
    }
}
