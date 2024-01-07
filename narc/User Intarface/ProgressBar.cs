// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour 
{
    public Text PrecentageText;
    public RectTransform ProgressIndicatorBG;
    public RectTransform ProgressIndicator;

    private IProgressable _trackedItem;
    public IProgressable TrackedItem
    {
        get
        {
            return _trackedItem;
        }

        set
        {
            _trackedItem = value;
        }
    }

    public float Progress
    {
        get
        {
            return _progress;
        }
        set
        {
            SetProgress(value);
        }
    }

    private float _progress = 0f;

	private void SetProgress(float progress)
    {
        if(progress < 0f || progress > 1f)
        {
            Debug.LogError("Progressbar progress out of range! (0-1)");
            return;
        }
        // Note: multiply width/height by canvas.scalefactor if different scales used
        float width = ProgressIndicatorBG.rect.width * progress;
        float height = ProgressIndicatorBG.rect.height;
        Vector2 scale;
        scale.x = width;
        scale.y = height;
        ProgressIndicator.sizeDelta = scale;
        PrecentageText.text = Mathf.Round(progress*100f) + "%";
        _progress = progress;
    }

    void Start()
    {
        Progress = 0f;
    }

    void Update()
    {
        if(_trackedItem != null)
        {
            Progress = _trackedItem.GetProgress();
        }
    }
}
