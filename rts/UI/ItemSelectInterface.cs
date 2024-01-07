using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ItemSelectInterface : ManagedMonoBehaviour {

    public Button DestroyButton;
    public Text NameText;

	public void Show(string objectName, UnityAction onDestroyClick )
    {
        NameText.text = objectName;
        DestroyButton.onClick.RemoveAllListeners();
        DestroyButton.onClick.AddListener(onDestroyClick);
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        DestroyButton.onClick.RemoveAllListeners();
    }
}
