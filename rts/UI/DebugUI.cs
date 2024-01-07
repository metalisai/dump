using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class DebugUI
{
    struct DebugParameter
    {
        public string label;
        public Func<string> value;
        public int guid;
    }

	class DebugMessage
	{
		public Vector3 startPosition;
		public string message;
		public float time;
	}

    static List<DebugParameter> debugParameters = new List<DebugParameter>();
	static List<DebugMessage> debugMessages = new List<DebugMessage> ();
    static Dictionary<int, DebugMessage> persistentDebugMessages = new Dictionary<int, DebugMessage>();
    public static bool active = true;

    static int GUID = 0;
    static int GUID2 = 0;

    public static int AddParameter(string label, Func<string> value)
    {
        if (!active)
            return -1;
#if DEBUG
        var dp = new DebugParameter() { label = label, value = value, guid = GUID++};
        debugParameters.Add(dp);
        return dp.guid;
#else
        return 0;
#endif
    }

    public static void RemoveParameter(int guid)
    {
        if (guid == -1)
            return;
#if DEBUG
        var val = debugParameters.Where(x => x.guid == guid).FirstOrDefault();
        debugParameters.Remove(val);
#endif
    }

    static List<DebugMessage> removeList = new List<DebugMessage>();

    public static void OnGUI()
    {
#if DEBUG
		GUI.color = Color.white;
        if (active)
        {
            StringBuilder sb = new StringBuilder();

            if (Event.current.type == EventType.Repaint)
            {
                foreach (var par in debugParameters)
                {
                    sb.Append(par.label);
                    sb.Append(": ");
                    sb.Append(par.value());
                    sb.Append(Environment.NewLine);
                }
            }

            Rect dpos = new Rect(new Vector2(50.0f, 50.0f), new Vector2(500.0f, 500.0f));
            GUI.Label(dpos, sb.ToString());

            removeList.Clear();
            foreach (var dmgs in debugMessages)
            {
                Vector2 pos = Camera.main.WorldToScreenPoint(dmgs.startPosition);
                pos.y = Screen.height - pos.y;
                pos.y -= dmgs.time * 20.0f;
                pos.x -= 50.0f;

                Color col = Color.black;
                col.a = 1.0f - (dmgs.time / 4.0f);
                GUI.color = col;
                GUI.Label(new Rect(pos.x, pos.y, 200.0f, 90.0f), dmgs.message);
                dmgs.time += Time.unscaledDeltaTime;
                if (dmgs.time >= 4.0f)
                    removeList.Add(dmgs);
            }
            foreach (var msg in removeList)
                debugMessages.Remove(msg);

            foreach(var dmsg in persistentDebugMessages.Values)
            {
                Vector2 pos = Camera.main.WorldToScreenPoint(dmsg.startPosition);
                pos.y = Screen.height - pos.y;
                pos.x -= 50.0f;

                GUI.color = Color.black;
                GUI.Label(new Rect(pos.x, pos.y, 200.0f, 90.0f), dmsg.message);
            }
        }
#endif
    }

	public static void ShowMessage(Vector3 position, string text)
	{
#if DEBUG
        DebugMessage msg = new DebugMessage();
		msg.message = text;
		msg.startPosition = position;
		msg.time = UnityEngine.Random.Range(0.0f,1.0f);
		debugMessages.Add (msg);
#endif
    }

    public static int AddPersistentMessage(Vector3 position, string initialText)
    {
        int guid = -1;
#if DEBUG
        guid = GUID2 ++;
        DebugMessage msg = new DebugMessage();
        msg.message = initialText;
        msg.startPosition = position;
        msg.time = 0.0f;
        persistentDebugMessages.Add(guid, msg);
#endif
        return guid;
    }

    public static void UpdatePersistentMessage(int handle, string newText)
    {
        if (handle == -1)
            return;
        DebugMessage msg;
        if(persistentDebugMessages.TryGetValue(handle, out msg))
        {
            msg.message = newText;
            persistentDebugMessages[handle] = msg;
        }
    }

    public static void RemovePersistentMessage(int handle)
    {
        if (handle == -1)
            return;
        persistentDebugMessages.Remove(handle);
    }
}
