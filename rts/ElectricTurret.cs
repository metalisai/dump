using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

[ObjectLogic(ActiveObject.ObjectTag.ElectricTurret)]
public class ElectricTurret : ElectricDevice
{
    GameObject target = null;

    GameObject turretHead;
    GameObject barrel;


    int idleConsumption = 10;
    int combatConsumption = 30;

    float reloadTimer = 0.0f;

    AudioSource audioSource;

	PowerConsumeModule _powerModule;
	TargetingModule _targeting;

    new void Awake()
    {
        GameTime.MediumTick += Tick;

        var turretHeadT = transform.FindChild("TurretHead");
        Assert.IsNotNull(turretHeadT);
        var barrelT = turretHeadT.FindChild("Barrel");
        Assert.IsNotNull(barrelT);

        turretHead = turretHeadT.gameObject;
        barrel = barrelT.gameObject;

        base.Awake();

        audioSource = GetComponent<AudioSource>();
		_powerModule = new PowerConsumeModule (connector, idleConsumption);
		_targeting = new TargetingModule ();
    }

    float turnRate = 30.0f;
    float range = 100.0f;

    void Tick(float deltaTime)
    {
		_powerModule.Tick (deltaTime);
    }

    public override void ManagedUpdate()
    {
        if (hovering)
            UI.SwitchInterface.UpdateData(transform.position);

		if (!_powerModule.IsOnline)
            return;

        if (target != null) // check if current target still valid
        {
            float distanceToTarget = Vector3.Distance(target.transform.position, transform.position);
            if (distanceToTarget > range || !target.activeInHierarchy)
                target = null;
        }
		if(target == null) // try to find a new target
        {
			var validTargets = Game.Instance.GetDestroyableEnemiesInRange(transform.position, range);
			var searchR = _targeting.FindBestTarget (validTargets, barrel.transform.position);
            if(searchR != null)
                target = searchR.gameObject;
        }
        reloadTimer -= Time.deltaTime;

        if (target != null)
        {
			_powerModule.SetConsumption (combatConsumption);

            // targeting math for the turret head and barrel
            Vector3 dir = (target.transform.position - turretHead.transform.position).normalized;
            float dif;
            float yaw = RTSMath.GetLimitedYawToTarget(dir, turretHead.transform.rotation, turnRate*Time.deltaTime, out dif);
            turretHead.transform.Rotate(new Vector3(0.0f, yaw, 0.0f));
            dir = (target.transform.position - barrel.transform.position).normalized;
            Vector3 rot = Quaternion.LookRotation(dir, Vector3.up).eulerAngles;
            barrel.transform.localRotation = Quaternion.Euler(new Vector3(rot.x, 0.0f, 0.0f));

            if(reloadTimer <= 0.0f && Mathf.Repeat(Mathf.Abs(dif), 180.0f) <= 0.1f)
            {
                Shoot(target.GetComponent<Destroyable>());
            }
        }
        else
        {
			_powerModule.SetConsumption (idleConsumption);
        }
    }

    void Shoot(Destroyable dest)
    {
        reloadTimer = 0.9f;
        audioSource.PlayOneShot(audioSource.clip);
        Action onHit = () =>
        {
            // the null check only works if it's a monobehaviour
            if (dest != null)
                dest.Hit(100);
        };
        Game.BulletManager.FireBullet(barrel.transform.position, target.transform.position, 100.0f, onHit);
    }

    new void OnDestroy()
    {
        GameTime.MediumTick -= Tick;
        base.OnDestroy();
    }

    void Switch(bool active)
    {
		_powerModule.OnlineSwitch = active;
    }

    bool hovering = false;
    void OnMouseEnter()
    {
        hovering = true;
        Action show = () =>
        {
			UI.SwitchInterface.Show(transform.position, _powerModule.OnlineSwitch, Switch);
        };
        UI.RequestPopup(this, show);
    }

    void OnMouseExit()
    {
        hovering = false;
        Action hide = () =>
        {
            UI.SwitchInterface.Hide();
        };
        UI.HidePopup(this, hide);
    }
}

public class PowerConsumeModule
{
	ElectricConnector _connector;
	public bool IsOnline = false;
	float _powerConsumption;
	public bool OnlineSwitch = true;

	public PowerConsumeModule (ElectricConnector connector, int defaultConsumption)
	{
		_connector = connector;
		_powerConsumption = defaultConsumption;
	}

	public void SetConsumption(float consumption)
	{
		_powerConsumption = consumption;
	}

	public void Tick(float dt)
	{
		if (!OnlineSwitch) {
			IsOnline = false;
			return;
		}
		int neededAmount = (int)(_powerConsumption * dt);
		float amount = _connector.GetElectricity(neededAmount, false);
		if (amount > 0) {
			IsOnline = true;
		} else {
			IsOnline = false;
		}
	}
}


public class TargetingModule
{
	public enum TargetingModuleType
	{
		Random,
		Closest,
		Furthest,
		LowestHealth,
		HighestHealth,
		SplashMost,
		PenetrateMost
	}

	public TargetingModuleType ModuleType = TargetingModuleType.Random;

	public Destroyable FindBestTarget(List<Destroyable> possibleTargets, Vector3 gunLocation)
	{
		if (possibleTargets == null || possibleTargets.Count == 0)
			return null;
		
		switch (ModuleType) {
		case TargetingModuleType.Random:
			{
				int count = possibleTargets.Count;
				int choose = UnityEngine.Random.Range (0, count);
				return possibleTargets [choose];
			}
		case TargetingModuleType.Closest:
			{
				float minSqDist = float.MaxValue;
				Destroyable best = null;
				int count = possibleTargets.Count;
				for(int i = 0; i < count; i++)
				{
					float sqDist = Vector3.SqrMagnitude (gunLocation - possibleTargets [i].transform.position);
					if (sqDist < minSqDist) {
						minSqDist = sqDist;
						best = possibleTargets [i];
					}
				}
				return best;
			}
		case TargetingModuleType.Furthest:
			{
				float maxSqDist = float.MinValue;
				Destroyable best = null;
				int count = possibleTargets.Count;
				for(int i = 0; i < count; i++)
				{
					float sqDist = Vector3.SqrMagnitude (gunLocation - possibleTargets [i].transform.position);
					if (sqDist > maxSqDist) {
						maxSqDist = sqDist;
						best = possibleTargets [i];
					}
				}
				return best;
			}
		case TargetingModuleType.LowestHealth:
			{
				float minHealth = float.MaxValue;
				Destroyable best = null;
				int count = possibleTargets.Count;
				for (int i = 0; i < count; i++) {
					float health = possibleTargets [i].Health01();
					if (health < minHealth) {
						minHealth = health;
						best = possibleTargets [i];
					}
				}
				return best;
			}
		case TargetingModuleType.HighestHealth:
			{
				float maxHealth = float.MaxValue;
				Destroyable best = null;
				int count = possibleTargets.Count;
				for (int i = 0; i < count; i++) {
					float health = possibleTargets [i].Health01();
					if (health > maxHealth) {
						maxHealth = health;
						best = possibleTargets [i];
					}
				}
				return best;
			}
		case TargetingModuleType.SplashMost:
			throw new NotImplementedException ();
		case TargetingModuleType.PenetrateMost:
			throw new NotImplementedException ();
		default:
			throw new InvalidOperationException ();
		}
	}
}
