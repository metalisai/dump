// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using UnityEngine.UI;

public class DefaultSelectionWindow : MonoBehaviour
{
    public Text ItemName;
    public Button DeleteButton;
    public Image Icon;

    protected ItemSelector _selector;
    protected GameObject curSelection;
    private SelectionWindow _selWin;

    protected bool _active = false;

    public GameObject currentSelection
    {
        get
        {
            return curSelection;
        }
    }

    void Awake()
    {
        _selector = FindObjectOfType<ItemSelector>();
        _selWin = FindObjectOfType<SelectionWindow>();
    }

    public void SetSelected(PlaceableObject pobj)
    {
        curSelection = pobj.gameObject;
        this.gameObject.SetActive(true);
        ItemName.text = pobj.DisplayName;
        DeleteButton.onClick.RemoveAllListeners();
        DeleteButton.onClick.AddListener(pobj.Delete);
        _active = true;
    }

    public void Hide()
    {
        if (_active)
        {
            curSelection = null;
            gameObject.SetActive(false);
            _selector.CancelSelection();
            _selWin.CancelSelection();
            _active = false;
        }
    }
}
