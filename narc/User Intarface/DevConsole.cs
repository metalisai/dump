// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;
using System.Linq;
using System;

public class DevConsole : MonoBehaviour
{
    public Text ConsoleText;
    public InputField InputField;

    string[] _lines = new string[10];


    Dictionary<string, Action<string[]>> _commands = new Dictionary<string, Action<string[]>>();

    void Start()
    {
        _commands.Add("settime", SetTime);
        _commands.Add("settimescale", SetTimeScale);
        _commands.Add("saveconf", SaveConf);
        _commands.Add("loadconf", LoadConf);
        _commands.Add("toggleriskmap", ToggleRiskMap);
        _commands.Add("givecash", GiveCash);
        _commands.Add("givemoney", GiveMoney);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            transform.GetChild(0).gameObject.SetActive(!transform.GetChild(0).gameObject.activeInHierarchy);
        }
    }

    void OnBufferChanged()
    {
        var sb = new StringBuilder();
        foreach (var s in _lines)
        {
            sb.Append(s).Append(System.Environment.NewLine);
        }
        sb.Remove(sb.Length - 1, 1);
        ConsoleText.text = sb.ToString();
    }

    void AppendLine(string line)
    {
        bool found = false;

        for (int i = 0; i < _lines.Length; i++)
        {
            if (_lines[i] == null)
            {
                found = true;
                _lines[i] = line;
                break;
            }
        }

        if (!found)
        {
            for (int i = 0; i < _lines.Length - 1; i++)
            {
                _lines[i] = _lines[i + 1];
            }
            _lines[_lines.Length - 1] = line;
        }

        OnBufferChanged();
    }

    bool TryCommand(string command, string[] args)
    {
        Action<string[]> val;
        bool ret = _commands.TryGetValue(command, out val);
        if (ret)
        {
            val.Invoke(args);
        }
        return ret;
    }

    public void Submit()
    {
        string[] tokenized = InputField.text.Split(' ');
        if (TryCommand(tokenized[0], tokenized.Skip(1).Take(tokenized.Length - 1).ToArray()))
        {

        }
        else
        {
            AppendLine("Unknown command!");
        }
        InputField.text = "";
    }

    void SetTime(string[] args)
    {
        if (args.Length != 2)
        {
            AppendLine("Invalid parameter count");
            return;
        }

        int hour;
        int min;
        bool ok = int.TryParse(args[0], out hour);
        ok = int.TryParse(args[1], out min);

        if (!ok)
        {
            AppendLine("Invalid parameters!");
            return;
        }

        GameTime.DayTime = (hour * 60 + min);

        AppendLine("Gametime set to " + hour + ":" + min);
    }

    void SetTimeScale(string[] args)
    {
        if (args.Length != 1)
        {
            AppendLine("Invalid parameter count");
            return;
        }

        float scale;
        bool ok = float.TryParse(args[0], out scale);

        if (!ok)
        {
            AppendLine("Invalid parameters!");
            return;
        }

        Time.timeScale = scale;

        AppendLine("Timescale set to " + scale);
    }

    void SaveConf(string[] args)
    {
        var conf = FindObjectOfType<Configuration>();

        if(!Application.isEditor)
        {
            AppendLine("Configuration can only be saved in editor mode!");
            return;
        }

        if(conf != null)
        {
            conf.SaveCurrent();
            AppendLine("Configuration saved");
        }
        else
        {
            AppendLine("No configuration manager object found in the scene!");
        }
    }

    void LoadConf(string[] args)
    {
        var conf = FindObjectOfType<Configuration>();

        if (conf != null)
        {
            conf.Load();
            AppendLine("Configuration loaded");
        }
        else
        {
            AppendLine("No configuration manager object found in the scene!");
        }
    }

    void ToggleRiskMap(string[] args)
    {
        var conf = FindObjectOfType<RiskHeatMap>();

        if (conf != null)
        {
            conf.ToggleShow();
            AppendLine("Riskmap toggled");
        }
        else
        {
            AppendLine("No RiskHeatMap object found in the scene!");
        }
    }

    void GiveCash(string[] args)
    {
        if (args.Length != 1)
        {
            AppendLine("Invalid parameter count");
            return;
        }

        int cash;
        bool ok = int.TryParse(args[0], out cash);

        if (!ok)
        {
            AppendLine("Invalid parameters! (integer expected)");
            return;
        }

        FindObjectOfType<Player>().Cash += cash;
    }

    void GiveMoney(string[] args)
    {

        if (args.Length != 1)
        {
            AppendLine("Invalid parameter count");
            return;
        }

        int cash;
        bool ok = int.TryParse(args[0], out cash);

        if (!ok)
        {
            AppendLine("Invalid parameters! (integer expected)");
            return;
        }

        FindObjectOfType<Player>().BankMoney += cash;
    }
}
