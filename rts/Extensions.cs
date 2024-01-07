using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Assertions;

public static class Extensions
{
    public static bool GetMouseButtonDownNoUI(int button)
    {
        return Input.GetMouseButtonDown(button) && !EventSystem.current.IsPointerOverGameObject();
    }
    public static bool GetMouseButtonNoUI(int button)
    {
        return Input.GetMouseButton(button) && !EventSystem.current.IsPointerOverGameObject();
    }

    static List<GameObject> listOfChildren;
    public static List<GameObject> GetChildrenRecursive(this GameObject obj, bool root = true)
    {
        if (root)
        {
            listOfChildren = new List<GameObject>();
            listOfChildren.Add(obj);
        }
        if (null == obj)
            return listOfChildren;

        foreach (Transform child in obj.transform)
        {
            if (null == child)
                continue;
            listOfChildren.Add(child.gameObject);
            GetChildrenRecursive(child.gameObject, false);
        }
        return listOfChildren;
    }

    public static void AddTag(this GameObject obj, ActiveObject.ObjectTag tag)
    {
        var ao = obj.GetComponent<ActiveObject>();
        Assert.IsNotNull(ao, "AddTag used on object with no ActiveObject component");
        ao.ObjectTags.Add(tag);
    }
}

public static class RTSMath
{
    public static Vector3 GetClosestPointOnLine(ref Vector3 a, ref Vector3 b, Vector3 from)
    {
        Vector3 atob = b - a;
        from = from - a;
        float atob2 = Vector3.Dot(atob, atob); // squared length
        float atopDotAtob = Vector3.Dot(from, atob);
        float t = atopDotAtob / atob2;
        t = Mathf.Clamp01(t);
        atob *= t;
        return a + atob;
    }

    public static bool LineIntersectsCircle(ref Vector3 linestart, ref Vector3 lineend, ref Vector3 circleCenter, float circleRadius2, out Vector3 closestIntersection)
    {
        Vector3 closestPoint = GetClosestPointOnLine(ref linestart, ref lineend, circleCenter);
        bool ret = Vector3.SqrMagnitude(closestPoint - circleCenter) < circleRadius2;
        closestIntersection = closestPoint;
        return ret;
    }

    public static float GetLimitedYawToTarget(Vector3 directionToTarget, Quaternion currentRotation, float maxTurnRate, out float difference)
    {
        directionToTarget.y = 0;
        var desiredAngle = Quaternion.LookRotation(directionToTarget, Vector3.up).eulerAngles.y;
        Vector3 currentEuler = currentRotation.eulerAngles;
        difference = Mathf.DeltaAngle(currentEuler.y, desiredAngle);
        float defaultRate = Mathf.Sign(difference) * maxTurnRate;
        return Mathf.Abs(difference) < Mathf.Abs(defaultRate) ? difference : defaultRate;
    }
}
