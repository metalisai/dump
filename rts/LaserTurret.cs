using System;
using UnityEngine;
using UnityEngine.Assertions;

[ObjectLogic(ActiveObject.ObjectTag.LaserTurret)]
public class LaserTurret : ElectricDevice
{
	GameObject target = null;

	GameObject turretHead;
	GameObject barrel;

	int idleConsumption = 10;
	int combatConsumption = 30;

	AudioSource audioSource;

	PowerConsumeModule _powerModule;
	TargetingModule _targeting;
	BeamRenderer _beam;

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
		_beam = BeamRenderer.CreateBeam (Vector3.zero, Vector3.one, 1.0f, Color.red);
		_beam.gameObject.SetActive (false);
	}

	float turnRate = 30.0f;
	float range = 100.0f;

	void Tick(float deltaTime)
	{
		_powerModule.Tick (deltaTime);
	}

	public override void ManagedUpdate()
	{
		bool laser = false;

		if (hovering)
			UI.SwitchInterface.UpdateData(transform.position);

		if (_powerModule.IsOnline) {

			if (target != null) { // check if current target still valid
				float distanceToTarget = Vector3.Distance (target.transform.position, transform.position);
				if (distanceToTarget > range || !target.activeInHierarchy)
					target = null;
			}
			if (target == null) { // try to find a new target
				var validTargets = Game.Instance.GetDestroyableEnemiesInRange (transform.position, range);
				var searchR = _targeting.FindBestTarget (validTargets, barrel.transform.position);
				if (searchR != null)
					target = searchR.gameObject;
			}

			if (target != null) {
                // targeting math for the turret head and barrel
                Vector3 dir = (target.transform.position - turretHead.transform.position).normalized;
                float dif;
                float yaw = RTSMath.GetLimitedYawToTarget(dir, turretHead.transform.rotation, turnRate * Time.deltaTime, out dif);
                turretHead.transform.Rotate(new Vector3(0.0f, yaw, 0.0f));
                dir = (target.transform.position - barrel.transform.position).normalized;
                Vector3 rot = Quaternion.LookRotation(dir, Vector3.up).eulerAngles;
                barrel.transform.localRotation = Quaternion.Euler(new Vector3(rot.x, 0.0f, 0.0f));

                if (Mathf.Repeat (Mathf.Abs (dif), 180.0f) <= 0.1f) {
					laser = true;
				}
			}
		} else {
			target = null;
		}

		if (laser && target != null) {
			_powerModule.SetConsumption (combatConsumption);
			_beam.gameObject.SetActive (true);
			_beam.UpdateBeam (barrel.transform.position, target.transform.position);
			// TODO: cache destroyable
			target.GetComponent<Destroyable> ().Hit(Time.deltaTime*500.0f);
		} else {
			_powerModule.SetConsumption (idleConsumption);
			_beam.gameObject.SetActive (false);
		}
	}

	new void OnDestroy()
	{
		GameTime.MediumTick -= Tick;
		if(_beam != null)
			Destroy (_beam.gameObject);
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

