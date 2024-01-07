using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class UI : ManagedMonoBehaviour {

    public ProgressInterface progressInterface;
    public HealthbarInterface healthInterface;
    public SwitchInterface switchInterface;
    public ItemSelectInterface itemSelectInterface;

    public GameObject button;
    public GameObject gameOverObj;

    static GameObject GameOverObj;
    public static GameObject Button;

    public static ProgressInterface ProgressInterface;
    public static HealthbarInterface HealthInterface;
    public static SwitchInterface SwitchInterface;
    public static ItemSelectInterface ItemSelectInterface;
    public static WarehouseInterface WarehouseInterface;

    public static Dictionary<object, Action> PendingPopups = new Dictionary<object, Action>();
    public static HashSet<object> Nopopups = new HashSet<object>();

    public static UI Instance;

    public static bool PopupsAllowed
    {
        get { return Nopopups.Count == 0; }
    }

    public static void RequestPopup(object requester, Action show)
    {
        if(!PopupsAllowed)
        {
            PendingPopups.Add(requester, show);
        }
        else
        {
            show();
        }
    }

    public static void HidePopup(object requester, Action hide)
    {
        Action got;
        if(PendingPopups.TryGetValue(requester, out got))
        {
            PendingPopups.Remove(requester);
        }
        else
        {
            hide();
        }
    }

    public static void DisablePopups(object requester)
    {
        Nopopups.Add(requester);
    }
    public static void EnablePopups(object requester)
    {
        Nopopups.Remove(requester);
    }

    public static void GameOver()
    {
        GameOverObj.SetActive(true);
    }

    void Awake()
    {
        ProgressInterface = progressInterface;
        HealthInterface = healthInterface;
        SwitchInterface = switchInterface;
        ItemSelectInterface = itemSelectInterface;
        WarehouseInterface = GetComponentsInChildren<WarehouseInterface>(true).FirstOrDefault();
        if (WarehouseInterface == null)
            Debug.LogError("UI didn't have WarehouseInterface!");

        GameOverObj = gameOverObj;
        Button = button;
        Instance = this;
    }
}
