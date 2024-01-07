using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class GameTime
{
    static int startHour = 12;
    static int startMinute = 59;

    static double secondsSinceStart = 0;
    static double transformedSecondsSinceStart = 0; // real world seconds transformed into ingame time

    const int FASTTICKSPERSECOND = 60;
    const int MEDIUMTICKSPERSECOND = 10;
    const int SLOWTICKSPERSECOND = 1;

    const float FASTTICKDELTA = 1.0f / FASTTICKSPERSECOND;
    const float MEDIUMTICKDELTA = 1.0f / MEDIUMTICKSPERSECOND;
    const float SLOWTICKDELTA = 1.0f / SLOWTICKSPERSECOND;

    const double TIMESCALE = 100.0;

    public static int Hour { get { return ((int)transformedSecondsSinceStart / 3600) % 24; } }
    public static int Minute { get { return ((int)transformedSecondsSinceStart / 60) % 60; } }
    public static int Second { get { return (int)transformedSecondsSinceStart % 60; } }
    public static float DayProgress { get { return (float)((int)transformedSecondsSinceStart % 86400) / 86400; } }

    public delegate void TickDelegate(float deltaTime);
    public static event TickDelegate FastTick;
    public static event TickDelegate MediumTick;
    public static event TickDelegate SlowTick;

    static double fastTickTimer = 0.0;
    static double mediumTickTimer = 0.0;
    static double slowTickTimer = 0.0;

    public static void Update(float dt)
    {
        secondsSinceStart += (double)dt;
        transformedSecondsSinceStart = (secondsSinceStart*TIMESCALE) + (startHour * 60 * 60) + (startMinute*60);

        fastTickTimer += dt;
        mediumTickTimer += dt;
        slowTickTimer += dt;

        if (fastTickTimer >= FASTTICKDELTA)
        {
            int count = (int)(fastTickTimer / FASTTICKDELTA);
            fastTickTimer -= count * FASTTICKDELTA;
            for(int i = 0; i < count; i++)
            {
                if(FastTick != null)
                    FastTick.Invoke(FASTTICKDELTA);
            }
        }

        if (mediumTickTimer >= MEDIUMTICKDELTA)
        {
            int count = (int)(mediumTickTimer / MEDIUMTICKDELTA);
            mediumTickTimer -= count * MEDIUMTICKDELTA;
            for (int i = 0; i < count; i++)
            {
                if (MediumTick != null)
                    MediumTick.Invoke(MEDIUMTICKDELTA);
            }
        }

        if (slowTickTimer >= SLOWTICKDELTA)
        {
            int count = (int)(slowTickTimer / SLOWTICKDELTA);
            slowTickTimer -= count * SLOWTICKDELTA;
            for (int i = 0; i < count; i++)
            {
                if (SlowTick != null)
                    SlowTick.Invoke(SLOWTICKDELTA);
            }
        }
    }
}
