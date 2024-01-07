using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

[ObjectLogic(ActiveObject.ObjectTag.ArtilleryTurret)]
public class ArtilleryTurret : ElectricDevice
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
        _powerModule = new PowerConsumeModule(connector, idleConsumption);
        _targeting = new TargetingModule();
    }

    float turnRate = 30.0f;
    float maxRange = 200.0f;
    float minRange = 50.0f;
    float reloadTime = 5.0f;
    float shootAngle = 50.0f;
    float damageRange = 25.0f;
    float maxDamage = 500.0f;

    void Tick(float deltaTime)
    {
        _powerModule.Tick(deltaTime);
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
            if (distanceToTarget > maxRange || distanceToTarget < minRange || !target.activeInHierarchy)
                target = null;
        }
        if (target == null) // try to find a new target
        {
            //var validTargets = Game.Instance.GetDestroyableEnemiesInRange(transform.position, minRange, maxRange);
            var validTargets = Game.Instance.GetDestroyableEnemiesInRange(transform.position, maxRange);
            var searchR = _targeting.FindBestTarget(validTargets, barrel.transform.position);
            if (searchR != null)
                target = searchR.gameObject;
        }
        reloadTimer -= Time.deltaTime;

        if (target != null)
        {
            _powerModule.SetConsumption(combatConsumption);

            // targeting math for the turret head and barrel
            Vector3 dir = (target.transform.position - turretHead.transform.position).normalized;
            float dif;
            float yaw = RTSMath.GetLimitedYawToTarget(dir, turretHead.transform.rotation, turnRate * Time.deltaTime, out dif);
            turretHead.transform.Rotate(new Vector3(0.0f, yaw, 0.0f));
            dir = (target.transform.position - barrel.transform.position).normalized;
            Vector3 rot = Quaternion.LookRotation(dir, Vector3.up).eulerAngles;
            barrel.transform.localRotation = Quaternion.Euler(new Vector3(-shootAngle, 0.0f, 0.0f));

            if (reloadTimer <= 0.0f && Mathf.Repeat(Mathf.Abs(dif), 180.0f) <= 0.1f)
            {
                Shoot(target.GetComponent<Destroyable>());
            }
        }
        else
        {
            _powerModule.SetConsumption(idleConsumption);
        }
    }

    void Shoot(Destroyable dest)
    {
        reloadTimer = reloadTime;
        audioSource.PlayOneShot(audioSource.clip);
        Vector3 targetPos = target.transform.position;
        Action onHit = () =>
        {
            var hitTargets = Game.Instance.GetDestroyableEnemiesInRange(targetPos, damageRange);
            foreach(var hittarget in hitTargets)
            {
                float dmgScale = 1.0f - ((targetPos - hittarget.transform.position).magnitude / damageRange);
                hittarget.Hit(dmgScale * maxDamage);
            }
            AudioSource.PlayClipAtPoint(Game.Instance.ExplosionClip, targetPos);
            ExplosionManager.ExplosionAt(targetPos);
        };
        Game.BulletManager.FireArtilleryProjectile(barrel.transform.position, targetPos, shootAngle, onHit);
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
