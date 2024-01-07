using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressInterface : ManagedMonoBehaviour {

    public Text PrecentageText;
    public Text NameText;
    public RectTransform ProgressBarTransform;

    float initialWidth;

	void Awake () {
        initialWidth = ProgressBarTransform.sizeDelta.x;
	}

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void SetData(Vector3 pos, string name, float progress)
    {
        NameText.text = name;
        transform.position = Camera.main.WorldToScreenPoint(pos);
        PrecentageText.text = ((int)(progress*100.0f)).ToString() + "%";
        ProgressBarTransform.sizeDelta = new Vector2(initialWidth * progress, ProgressBarTransform.sizeDelta.y);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

}
