// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;

public class GameTime : MonoBehaviour
{
    public delegate void GameTick();
    public static float TimeOffset = 720f;
    public const float GameTickInterval = 0.1f; // gametick interval in seconds

    private static float _lastRealtime;
    private static float _smoothedRealDeltaTime;
    private static float[] _realDeltaBuffer = new float[5];
    private static int frameId = 0;
    private static int _weekDayLastTick = 0;
    private static int _hourLastTick = Mathf.RoundToInt(TimeOffset/60f);
    private static int _minuteLastTick = 0;
    private float _curTickTime = 0f;
    private float _curReal100msTickTime = 0f;
    private bool _paused = false;

    /// <summary>
    /// Game simulation tick (every GameTickInterval seconds)
    /// </summary>
    public static event GameTick OnTick;
    public static event GameTick OnReal100ms;
    public static event GameTick OnPayday;
    public static event GameTick OnHour;
    public static event GameTick OnDay;
    public static event GameTick OnMinute;

    /// <summary>
    /// Timescale independent deltaTime
    /// </summary>
    public static float RealDeltaTime
    {
        get { return _smoothedRealDeltaTime; }
    }

    /// <summary>
    /// Time since game start in seconds
    /// </summary>
    public static float Time
    {
        get; set;
    }

    /// <summary>
    /// Time since the start of the current day. (in game minutes)
    /// </summary>
    public static float DayTime
    {
        get
        {
            return (Time + TimeOffset)%(24*60);
        }
        set
        {
            TimeOffset = value - Time;
        }
    }

    /// <summary>
    /// Number of game hours currently
    /// </summary>
    public static float TimeInHours
    {
        get { return ( Time + TimeOffset )/ 60f; }
    }

    /// <summary>
    /// Current game clock hour
    /// </summary>
    public static int Hour
    {
        get { return (int) (((Time + TimeOffset) / 60f) % 24); }
    }

    /// <summary>
    /// Current game clock minute
    /// </summary>
    public static int Minute
    {
        get { return (int)((Time + TimeOffset) % 60f); }
    }

    /// <summary>
    /// The number of day currently in progress
    /// </summary>
    public static int Day
    {
        get { return (int)((Time + TimeOffset) / (24f * 60f)); }
    }

    /// <summary>
    /// The index of day currently in progress
    /// </summary>
    public static int WeekDay
    {
        get { return (int)((Time + TimeOffset) / (24f * 60f)%7); }
    }

    /// <summary>
    /// Progress of day from 0...1
    /// </summary>
    public static float DayProgress
    {
        get { return (TimeInHours%24f)/24f; }
    }

    void Update()
    {
        _realDeltaBuffer[frameId % _realDeltaBuffer.Length] = UnityEngine.Time.realtimeSinceStartup - _lastRealtime;
        _smoothedRealDeltaTime = UnityEngine.Time.realtimeSinceStartup - _lastRealtime;

        _curReal100msTickTime += _smoothedRealDeltaTime;
        if(_curReal100msTickTime >= 0.1f && OnReal100ms!=null)
        {
            OnReal100ms();
            _curReal100msTickTime = 0f;
        }

        _lastRealtime = UnityEngine.Time.realtimeSinceStartup;
        if (frameId > 11)
        {
            float sum = 0;
            for(int i = 0; i < _realDeltaBuffer.Length; i++)
            {
                sum += _realDeltaBuffer[i];
            }
            _smoothedRealDeltaTime = sum / _realDeltaBuffer.Length;
        }
        else
        {
            _smoothedRealDeltaTime = UnityEngine.Time.realtimeSinceStartup - _lastRealtime;
        }

        Time += UnityEngine.Time.deltaTime;

        _curTickTime += UnityEngine.Time.deltaTime;

        while(_curTickTime >= GameTickInterval)
        {
            _curTickTime -= GameTickInterval;
            Tick();
        }

        if(_hourLastTick == 23 && Hour == 0)
        {
            if(OnDay != null)
            {
                OnDay();
            }
        }
        else if (_hourLastTick == 11 && Hour == 12)
        {
            if (OnPayday != null)
            {
                OnPayday();
            }
        }

        if (_hourLastTick != Hour)
        {
            if(OnHour != null)
            {
                OnHour();
            }
            _hourLastTick = Hour;
        }

        if(_minuteLastTick != Minute)
        {
            if(OnMinute != null)
            {
                OnMinute();
            }
            _minuteLastTick = Minute;
        }

        frameId++;
    }

    void Tick()
    {
        if (OnTick != null)
            OnTick();
        _weekDayLastTick = WeekDay;
    }

    public void TogglePause()
    {
        UnityEngine.Time.timeScale = _paused ? 1f : 0f;
        _paused = !_paused;
    }
}
