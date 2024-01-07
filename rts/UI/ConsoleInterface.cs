using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ConsoleInterface : ManagedMonoBehaviour {

    public Text HistoryText;
    public InputField CommandInput;

    List<string> history = new List<string>();
    public Dictionary<string, Func<string,string>> Commands = new Dictionary<string, Func<string,string>>();
    bool active = false;

    void Awake()
    {
        Func<string, string> sersettings = (string args) =>
        {
            Game.Instance.SerializeCurrentSettings();
            return "Settings serialized";
        };

        Func<string, string> timescale = (string args) =>
        {
            float timescaleArg;
            if(float.TryParse(args, out timescaleArg))
            {
                timescaleArg = Mathf.Clamp(timescaleArg, 0.0f, 50.0f);
                Time.timeScale = timescaleArg;
                return "Timescale changed to " + timescaleArg;
            }
            Game.Instance.SerializeCurrentSettings();
            return "Invalid arguments ([float] expected)";
        };

        Func<string, string> pgen = (string args) =>
        {
            GameObjectID id;
            try // EXCEPTIONS ARE SO DUMB WTF
            {
                id = (GameObjectID)Enum.Parse(typeof(GameObjectID), args);
                bool success = Game.localPlayer.Placer.PlaceObject(id);
                if (success)
                    return "Started placing...";
                return "No placeable object named " + args;
            }
            catch(ArgumentException)
            {
                return "No such object found!";
            }
        };

        Func<string, string> disableShop = (string args) =>
        {
            bool val;
            if (bool.TryParse(args, out val))
            {
                Game.Instance.ItemShop.disabled = !val;
                return "Shop set to " + val;
            }
            return "Invalid params (bool expected)";
        };

        Func<string, string> noDamage = (string args) =>
        {
            Destroyable.NoDamage = !Destroyable.NoDamage;
            if (Destroyable.NoDamage)
                return "Damage disabled";
            else
                return "Damage enabled";
        };

        Func<string, string> tdebug = (string args) =>
        {
            DebugUI.active = !DebugUI.active;
            if (!DebugUI.active)
                return "DebugUI disabled";
            else
                return "DebugUI enabled";
        };

        Func<string, string> grsrc = (string args) =>
        {
            string[] argss = args.Split(' ');
            if (argss.Length < 2)
                return "2 arguments expected";
            try
            {
                var rt = (ResourceType)Enum.Parse(typeof(ResourceType), argss[0]);
                var amount = int.Parse(argss[1]);
                var store = Game.ResourceStores.Where(x => x.IsGenericStorage).FirstOrDefault();
                if (store == null)
                    return "You need warehouse to store the resource";
                var result = store.StoreResource(new ResourcePack() { ResourceType = rt, Amount = amount });
                if (result)
                    return "Resource given";
                return "Not enough room for resource";
            }
            catch
            {
                return "One or more parameters invalid";
            }
        };

        Commands.Add("sersettings", sersettings);
        Commands.Add("timescale", timescale);
        Commands.Add("place", pgen);
        Commands.Add("setshop", disableShop);
        Commands.Add("toggledamage", noDamage);
        Commands.Add("toggledebug", tdebug);
        Commands.Add("gr", grsrc);
    }

    public void Execute()
    {
        string text = CommandInput.text;
        var split = text.Split(new [] { ' ' }, 2);
        string command = split[0];
        Func<string, string> commandFunc;
        if(Commands.TryGetValue(command, out commandFunc))
        {
            string result = commandFunc(split.Length == 2 ? split[1] : "");
            WriteLine(result);
        }
        else
        {
            WriteLine("Command '"+command+"' not found!");
        }
        CommandInput.text = "";
        CommandInput.Select();
        CommandInput.ActivateInputField();
    }

    void WriteLine(string line)
    {
        history.Add(line);
        Display();
    }

    void Display()
    {
        StringBuilder sb = new StringBuilder();
        for(int i = history.Count > 5 ? history.Count-5 : 0; i < history.Count; i++)
        {
            sb.Append(history[i]);
            if(i != history.Count-1)
                sb.Append('\n');
        }
        HistoryText.text = sb.ToString();
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	public override void ManagedUpdate () {
        if(Input.GetKeyDown(KeyCode.BackQuote))
        {
            transform.GetChild(0).gameObject.SetActive(!active);
            active = !active;
            DebugUI.active = !active;
            if(active)
            {
                CommandInput.Select();
                CommandInput.ActivateInputField();
            }
        }
        if(active && Input.GetKeyDown(KeyCode.Return))
            Execute();
	}
}
