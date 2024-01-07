// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class AIEvents
{
    public enum AIEventId
    {
        Gardener_Harvest_Done
    }

    public delegate void AnimatorEvent(AIEventId eventId, Animator animator);
    public static event AnimatorEvent OnAnimatorEvent;
    // desperate times, desperate measures (completely violating the point of events)
    public static void Notify(AIEventId eventId, Animator animator)
    {
        if (OnAnimatorEvent != null)
        {
            OnAnimatorEvent(eventId, animator);
        }
    }
}
