using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using UnityEngine.Assertions;

interface PowerProvider
{
    float GetElectricity(ElectricDevice requester, float amount, bool takeAnything);
}

interface Connectable
{
    ElectricConnector Connector { get; }
}

[ObjectLogic(ActiveObject.ObjectTag.ElectricBattery)]
public class ElectricBattery : ElectricDevice, PowerProvider, IProgressable
{
    private float charge = 0;
    public float MaxRechargePerSecond = 100;
    public float MaxCapacity = 10000;

    new void Awake()
    {
        deviceFlags |= DeviceFlags.Provider;
        deviceFlags |= DeviceFlags.Container;
        GameTime.MediumTick += Tick;
        base.Awake();
    }

    public float GetProgress()
    {
        Assert.IsTrue(charge >= 0);
        return (float)charge / (float)MaxCapacity;
    }

    public void Tick(float tickDelta)
    {
        float chargedThisTick = 0;
        foreach (var wire in connector.ConnectedWires)
        {
            float leftToCharge = MaxCapacity - charge - chargedThisTick;
            float allowedToCharge = (tickDelta * MaxRechargePerSecond) - chargedThisTick;
            float amount = Mathf.Min(allowedToCharge, leftToCharge);
            float power = wire.GetElectricity(this, amount, true);
            if (power != 0.0f)
            {
                chargedThisTick += power;
                charge += power;
            }
        }
    }

    public float GetElectricity(ElectricDevice requester, float amount, bool takeAnything)
    {
        if (!takeAnything)
        {
            if (charge >= amount)
            {
                charge -= amount;
                return amount;
            }
        }
        else
        {
            float ret = Mathf.Min(charge, amount);
            charge -= ret;
            return ret;
        }
        return 0;
    }

    // TODO: check if this gets triggered properly
    new void OnDestroy()
    {
        GameTime.MediumTick -= Tick;
        base.OnDestroy();
    }
}

[ObjectLogic(ActiveObject.ObjectTag.ElectricLamp)]
public class ElectricLamp : ElectricDevice
{
    bool lit = false;

    public int ConsumptionPerSecond = 10;
    GameObject bulb;
    Light tlight;
    MeshRenderer trenderer;

    new void Awake()
    {
        GameTime.MediumTick += Tick;
        base.Awake();
    }

    void Start()
    {
        bulb = gameObject.transform.FindChild("Bulb").gameObject;
        if (bulb == null)
            Debug.Log("There is an electric lamp but no bulb! (Add a child object called 'Bulb' to it with a light component)");

        tlight = bulb.GetComponent<Light>();
        trenderer = bulb.GetComponent<MeshRenderer>();

        trenderer.material.EnableKeyword("_EMISSION");
    }

    public void EnableLamp()
    {
        if (tlight != null)
            tlight.enabled = true;
        if(trenderer != null)
            trenderer.material.SetColor("_EmissionColor", Color.white);
        Debug.LogWarning("Lamp Lit");
        lit = true;
    }

    public void DisableLamp()
    {
        if (tlight != null)
            tlight.enabled = false;
        if (trenderer != null)
            trenderer.material.SetColor("_EmissionColor", Color.black);
        Debug.LogWarning("Lamp out of power");
        lit = false;
    }

    public void Tick(float tickDelta)
    {
        float ticks = 1.0f / tickDelta;
        float consume = ConsumptionPerSecond / ticks;
        float power = 0;

        foreach(var device in connector.ConnectedWires)
        {
            if (device is PowerProvider)
            {
                var provider = device as PowerProvider;
                power = provider.GetElectricity(this, consume, false);
                // TODO: with multiple providers this is wrong
                if(power > 0)
                    break;
            }
        }

        if (!lit && power != 0)
            EnableLamp();
        else if (lit && power == 0)
            DisableLamp();
    }

    // TODO: check if this gets triggered properly
    new void OnDestroy()
    {
        GameTime.MediumTick -= Tick;
        base.OnDestroy();
    }
}

[ObjectLogic(ActiveObject.ObjectTag.ElectricGenerator)]
public class ElectricGenerator : ElectricDevice, PowerProvider
{
    float internalCapacitor = 0;
    float powerPerSecond = 20;

    new void Awake()
    {
        GameTime.MediumTick += Tick;
        base.Awake();
    }

    void Start()
    {
        deviceFlags |= DeviceFlags.Provider;
    }

    public void Tick(float tickDelta)
    {
        internalCapacitor += tickDelta * powerPerSecond;
        internalCapacitor = Mathf.Min(internalCapacitor, 500.0f);
    }

    public float GetElectricity(ElectricDevice requester, float amount, bool takeAnything)
    {
        if (!takeAnything)
        {
            if (internalCapacitor >= amount)
            {
                internalCapacitor -= amount;
                return amount;
            }
        }
        else
        {
            float ret = Mathf.Min(internalCapacitor, amount);
            internalCapacitor -= ret;
            return ret;
        }
        return 0;
    }

    // TODO: check if this gets triggered properly
    new void OnDestroy()
    {
        GameTime.MediumTick -= Tick;
        base.OnDestroy();
    }
}

[ObjectLogic(ActiveObject.ObjectTag.ElectricPole)]
public class ElectricPole : ElectricDevice
{
    
}


public class ElectricDevice: ManagedMonoBehaviour, Connectable
{
    [Flags]
    public enum DeviceFlags
    {
        None = 0,
        Provider = 1,
        Consumer = 2,
        Container = 4
    }

    public DeviceFlags deviceFlags;
    protected ElectricConnector connector;

    protected void Awake()
    {
        connector = new ElectricConnector(this);
        connector.Position = transform.position;
    }

    protected float TakeAllAvailablePower(float maxAmount)
    {
        float got = 0.0f;
        foreach(var wire in connector.ConnectedWires)
        {
            float fromWire = wire.GetElectricity(this, maxAmount - got, true);
            got += fromWire;
            if (Mathf.Approximately(got, maxAmount))
                break;
        }
        return got;
    }

    protected void OnDestroy()
    {
        connector.DisconnectWires();
    }

    public ElectricConnector Connector
    {
        get
        {
            return connector;
        }
    }
}

public class ElectricConnector
{
    public readonly ElectricDevice Device;
    public readonly List<ElectricWire> ConnectedWires = new List<ElectricWire>(3);
    public Vector3 Position;

    public ElectricConnector(ElectricDevice device)
    {
        Device = device;
    }

    public void ConnectWire(ElectricWire wire)
    {
        ConnectedWires.Add(wire);
    }

    public void RemoveWire(ElectricWire wire)
    {
        ConnectedWires.Remove(wire);
    }

    public void DisconnectWires()
    {
        var removeList = new List<ElectricWire>();
        foreach (var co in ConnectedWires)
        {
            removeList.Add(co as ElectricWire);
        }
        foreach(var wire in removeList)
        {
            wire.Destroy();
        }
        ConnectedWires.Clear();
    }

    public float GetElectricity(float amount, bool takeAnything)
    {
        float got = 0.0f;
        foreach(var wire in ConnectedWires)
        {
            float power = wire.GetElectricity(Device, amount - got, takeAnything);
            got += power;
            if (Mathf.Approximately(amount, got))
                break;
        }
        return got;
    }
}

public class ElectricWire : PowerProvider
{
    public readonly ElectricConnector[] connectors = new ElectricConnector[2];
    public WireSystem system;
    int _physicalWireHandle = -1;
    public int LastGUID = -1;

    public ElectricWire(ElectricConnector connector1, ElectricConnector connector2, Vector3 startPosition, Vector3 endPosition)
    {
        connectors[0] = connector1;
        connectors[1] = connector2;
        connector1.ConnectWire(this);
        connector2.ConnectWire(this);
        _physicalWireHandle = PhysicalWireManager.CreateWire(startPosition, endPosition);
    }

    public void DebugRender()
    {
        if (connectors == null || connectors[0] == null || connectors[1] == null)
            return;
        Debug.DrawLine(connectors[0].Position, connectors[1].Position, Color.blue);
    }

    public ElectricConnector GetOther(ElectricConnector notother)
    {
        if (connectors[0] == notother)
            return connectors[1];
        if (connectors[1] == notother)
            return connectors[0];
        Assert.IsFalse(false); // should not try to get other connector, if the connector itself is not part of this wire
        return null;
    }

    public void Destroy()
    {
        PhysicalWireManager.DestroyWire(_physicalWireHandle);
        connectors[0].RemoveWire(this);
        connectors[1].RemoveWire(this);
        system.RemoveWire(this);
    }

    public float GetElectricity(ElectricDevice requester, float amount, bool takeAnything)
    {
        var devices = system.GetConnectedDevices();

        float got = 0.0f;

        foreach (var device in devices)
        {
            if (device == null || (object)device == requester)
                continue;
            if ((device.deviceFlags & ElectricDevice.DeviceFlags.Container) != 0  // container can't charge from container (otherwise they can keep circling power in case there is even number connected)
                && (requester.deviceFlags & ElectricDevice.DeviceFlags.Container) != 0)
                continue;
            if ((device.deviceFlags & ElectricDevice.DeviceFlags.Provider) != 0)
            {
#if DEBUG
                if (!(device is PowerProvider))
                    Debug.LogError("Electricity producer doesn't implement PowerProvider interface!");
#endif
                got += (device as PowerProvider).GetElectricity(requester, amount-got, takeAnything);
                
                if (Mathf.Approximately(got, amount))
                    return got;
            }
        }
        return got;
    }
}

public class WireSystem
{
    static int GUID;
    ElectricWire root;
    List<ElectricDevice> devices = new List<ElectricDevice>();
    WireLogicManager WManager;

    int localGUID;

    static int GenGUID()
    {
        return GUID++;
    }

    public void RemoveWire(ElectricWire wire)
    {
        // TODO: instead of deleting, seek for new root first
        if(wire == root)
        {
            WManager.RemoveWireSystem(this);
        }
        WManager.RemoveWire(wire);
    }

    void MapGraph(ElectricWire wire, ElectricConnector previousConnector)
    {
        if (wire.LastGUID == localGUID) // graph loop protection
            return;
        else
            wire.LastGUID = localGUID;

        wire.system = this;

        foreach(var connector in wire.connectors)
        {
            devices.Add(connector.Device);
            if (previousConnector != null && connector == previousConnector)
                continue;
            foreach(var connection in connector.ConnectedWires)
            {
                if (connection == wire)
                    continue;
                MapGraph(connection, connector);
            }
        }
        return;
    }

    public WireSystem(ElectricWire rootNode, WireLogicManager manager)
    {
        Assert.IsTrue(rootNode.system == null);
        localGUID = GenGUID();
        root = rootNode;
        MapGraph(rootNode, null);
        WManager = manager;
    }

    public bool UpdateGraph()
    {
        if (root.system != null)
            return false;
        localGUID = GenGUID();
        devices.Clear();
        MapGraph(root, null);
        return true;
    }

    public ElectricDevice[] GetConnectedDevices()
    {
        return devices.ToArray();
    }
}
