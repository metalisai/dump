using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;


public class WireLogicManager
{
    //List<Wire> wires = new List<Wire>();
    List<WireSystem> wireSystems = new List<WireSystem>();
    List<ElectricWire> electricWires = new List<ElectricWire>();

    Dictionary<GameObject, ElectricConnector> connectorDictionary = new Dictionary<GameObject, ElectricConnector>();

    public WireLogicManager()
    {
        Func<string> lambda = () => {
            return wireSystems.Count.ToString();
        };
        Func<string> lambda2 = () => {
            return electricWires.Count.ToString();
        };
        DebugUI.AddParameter("WireSystems", lambda);
        DebugUI.AddParameter("Wires", lambda2);
    }

    public void RemoveWire(ElectricWire wire)
    {
        electricWires.Remove(wire);
        UpdateAllGraphs();
    }

    public void RemoveWireSystem(WireSystem sys)
    {
        wireSystems.Remove(sys);
    }

    void UpdateAllGraphs()
    {
        List<WireSystem> remove = new List<WireSystem>();
        // TODO: find a smarter way to do this
        foreach (var wire in electricWires)
        {
            wire.system = null;
        }
        foreach (var ws in wireSystems)
        {
            bool ret = ws.UpdateGraph();
            if (!ret)
                remove.Add(ws);
        }
        foreach (var thing in remove)
        {
            // TODO: if this hasn't triggered, you can remove this assertion stuff
            int count = wireSystems.Count;
            wireSystems.Remove(thing);
            Assert.IsTrue(count-1 == wireSystems.Count);
        }
        foreach (var wire in electricWires)
        {
            if (wire.system == null)
            {
                wireSystems.Add(new WireSystem(wire, this));
            }
        }
        foreach (var wire in electricWires)
        {
            Assert.IsTrue(wire.system != null);
        }
    }

    public void AddWire(Interactable from, Interactable to)
    {
        var connector1 = connectorDictionary[from.rootObject];
        Assert.IsTrue(connector1 != null);

        var connector2 = connectorDictionary[to.rootObject];
        Assert.IsTrue(connector2 != null);

        ElectricWire newWire = new ElectricWire(connector1, connector2, from.transform.position, to.transform.position);
        Assert.IsTrue(newWire != null);
        Debug.Log(string.Format("Wire connected {0} {1}", connector1, connector2));
        electricWires.Add(newWire);
        //wires.Add(new Wire() { start = from.transform.position, end = to.transform.position });
        UpdateAllGraphs();
    }

    public void AddConnector(GameObject go, ElectricConnector connector)
    {
        connectorDictionary.Add(go, connector);
    }

    public void Update()
    {
        /*foreach (var wire in wires)
        {
            Debug.DrawLine(wire.start, wire.end);
        }*/

#if DEBUG
        foreach(var wire in electricWires)
        {
            wire.DebugRender();
        }
#endif
    }
}
