using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ObjectLogicAttribute : Attribute
{
    public ActiveObject.ObjectTag Tag;
    public ObjectLogicAttribute(ActiveObject.ObjectTag tag)
    {
        Tag = tag;
    }
}

public struct LogicInfo
{
    public Type BehaviourType;
    public bool IsConnactable;
}

public static class BehaviourFinder
{
    public static Type FindBehaviour(ActiveObject.ObjectTag tag)
    {
        var asm = Assembly.GetExecutingAssembly();
        var suitableTypes = asm.GetTypes().Where(x => x.IsSubclassOf(typeof(ManagedMonoBehaviour)));
        foreach(var mmbType in suitableTypes)
        {
            foreach(var attr in (mmbType as MemberInfo).GetCustomAttributes(true))
            {
                if (attr is ObjectLogicAttribute && (attr as ObjectLogicAttribute).Tag == tag)
                    return mmbType;
            }
        }
        return null;
    }

    public static Dictionary<ActiveObject.ObjectTag, LogicInfo> GetBehaviourTypes()
    {
        var ret = new Dictionary<ActiveObject.ObjectTag, LogicInfo>();

        var asm = Assembly.GetExecutingAssembly();
        var suitableTypes = asm.GetTypes().Where(x => x.IsSubclassOf(typeof(ManagedMonoBehaviour)));
        foreach (var mmbType in suitableTypes)
        {
            foreach (var attr in (mmbType as MemberInfo).GetCustomAttributes(true))
            {
                if (attr is ObjectLogicAttribute)
                {
                    bool isConnectable = typeof(Connectable).IsAssignableFrom(mmbType);
                    ret.Add((attr as ObjectLogicAttribute).Tag, new LogicInfo() { BehaviourType = mmbType, IsConnactable = isConnectable});
                }
            }
        }
        return ret;
    }
}
