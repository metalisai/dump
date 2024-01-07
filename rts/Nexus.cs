using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

[ObjectLogic(ActiveObject.ObjectTag.Nexus)]
public class Nexus : ElectricDevice
{
    public UnitStorage UnitStorage;
    float _unitProgress = 0.0f;

    public static float UnitProgressSeconds = 60.0f;
    public float ConsumptionPerSecond = 0.0f;
    float _unitProgressPerPowerUnit = 0.01f;
    Transform _workerSpawnTransform;

    public Text _powerText;
    PlaceableObject _currentBuildingWorker = null;

    public void ModifyKnob(float value)
    {
        ConsumptionPerSecond += value;
        ConsumptionPerSecond = Mathf.Max(0.0f, ConsumptionPerSecond);
        _powerText.text = "Power: " + ConsumptionPerSecond.ToString("0") + " kW/h";
    }

    new void Awake()
    {
        _powerText = GetComponentInChildren<Text>();
        Assert.IsNotNull(_powerText, "Power text was null! Nexus needs a panel with Text named <PowerText>");
        UnitStorage = gameObject.AddComponent<UnitStorage>();
        GameTime.SlowTick += SlowTick;
        GameTime.MediumTick += Tick;
        _workerSpawnTransform = transform.FindChild("WorkerSpawner");
        Assert.IsNotNull(_workerSpawnTransform, "Nexus needs a child named <WorkerSpawner>, indicating where workers spawn");
        CreateNewWorker();
        _powerText.text = "Power: " + ConsumptionPerSecond.ToString("0") + " kW/h";
        base.Awake();
    }

    new void OnDestroy()
    {
        GameTime.SlowTick -= SlowTick;
        GameTime.MediumTick -= Tick;
        base.OnDestroy();
    }

    void CreateNewWorker()
    {
        // TODO: awful, preload or something
        var spawnedWorker = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>(ObjectRegister.GetResourcePath(GameObjectID.Unit)));
        spawnedWorker.transform.position = _workerSpawnTransform.position;
        spawnedWorker.SetActive(true);
        _currentBuildingWorker = spawnedWorker.GetComponent<PlaceableObject>();
        _currentBuildingWorker.autoBuild = false;
        _currentBuildingWorker.StartBuilding();
        Game.Instance.RegisterDynamicObject(spawnedWorker, false);
    }

    void SlowTick(float dt)
    {
        if(GameTime.Hour >= 6 && GameTime.Hour < 21)
        {
            while (UnitStorage.EjectUnit() != null) ;
        }
        _unitProgress += dt;
        if(_unitProgress > UnitProgressSeconds)
        {
            _currentBuildingWorker.Built();
            _unitProgress = 0.0f;
            CreateNewWorker();
            UnitProgressSeconds += UnitProgressSeconds * 0.1f; // units will start taking longer to produce
        }
    }

    public void Tick(float tickDelta)
    {
        float ticks = 1.0f / tickDelta;
        float consume = ConsumptionPerSecond / ticks;
        float power = TakeAllAvailablePower(consume);
        _unitProgress += power * _unitProgressPerPowerUnit;
        if (_currentBuildingWorker != null)
            _currentBuildingWorker.buildProgress = Mathf.Clamp01(_unitProgress / UnitProgressSeconds);
    }
}
