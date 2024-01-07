using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class HealthbarInterface : ManagedMonoBehaviour {

    public Image ProgressBar;
    public RectTransform ProgressBarTransform;
    Vector2 origSize;
    public bool original = true;

    void Awake()
    {
        origSize = ProgressBarTransform.sizeDelta;
    }

    public HealthbarInterface Create()
    {
        Assert.IsTrue(original);
        var newhi = Instantiate(gameObject).GetComponent<HealthbarInterface>();
        Game.Instance.RegisterDynamicObject(newhi.gameObject, false);
        newhi.transform.SetParent(transform.parent);
        newhi.gameObject.SetActive(true);
        newhi.original = false;
        return newhi;
    }

    public void Destroy()
    {
        Assert.IsFalse(original);
        Game.Instance.DestroyDynamicObject(gameObject);
    }

    public void UpdateData(Vector3 pos, bool isEnemy, float value)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(pos);
        screenPos.y += origSize.y;
        transform.position = screenPos;
        value = Mathf.Clamp(value, 0.0f, 1.0f);
        ProgressBarTransform.sizeDelta = new Vector2(value * origSize.x, origSize.y);
        if (isEnemy)
            ProgressBar.color = Color.red;
        else
            ProgressBar.color = Color.green;
    }
}
