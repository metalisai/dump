using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

public class FogOfWar
{
    public struct FowSightAgent
    {
        public Transform transform;
        public float lineOfSight;
    }

    public struct FowEnemyAgent
    {
        public Destroyable destroyable;
        public Transform transform;
        public bool wasVisible;
    }

    public List<FowSightAgent> SightAgents = new List<FowSightAgent>();
    public List<FowEnemyAgent> EnemyAgents = new List<FowEnemyAgent>();

    public void Enable()
    {
        GameTime.MediumTick += MediumTick;
    }

    public void Disable()
    {
        for (int i = 0; i < EnemyAgents.Count; i++) // make all hidden enemies visible
        {
            var enemy = EnemyAgents[i];
            if(!enemy.wasVisible)
            {
                enemy.destroyable.Show();
                enemy.wasVisible = true;
                EnemyAgents[i] = enemy;
            }
        }
        GameTime.MediumTick -= MediumTick;
    }

    List<Vector3> retArray = new List<Vector3>();
    public List<Vector3> GetSightAgents() // TODO: could stackalloc this with unsafe?
    {
        retArray.Clear();
        foreach(var sagent in SightAgents)
        {
            retArray.Add(sagent.transform.position);
        }
        return retArray;
    }

    void MediumTick(float dt)
    {
        // TODO: is this a performance issue at all?
        for(int i = 0; i < EnemyAgents.Count; i++) // TODO: could get the list of position of the internal loop and cache it ourside the loop
        {
            bool visible = false;
            var enemy = EnemyAgents[i];
            foreach(var agent in SightAgents)
            {
                if (Vector3.Distance(enemy.transform.position, agent.transform.position) < agent.lineOfSight)
                {
                    visible = true;
                    break;
                }
            }

            if(visible && !enemy.wasVisible)
            {
                enemy.destroyable.Show();
                enemy.wasVisible = true;
                EnemyAgents[i] = enemy;
            }
            else if (!visible && enemy.wasVisible)
            {
                enemy.destroyable.Hide();
                enemy.wasVisible = false;
                EnemyAgents[i] = enemy;
            }
        }
    }

    public void AddEnemyAgent(Destroyable e)
    {
        FowEnemyAgent eagent = new FowEnemyAgent();
        eagent.transform = e.transform;
        eagent.wasVisible = false;
        eagent.destroyable = e;
        EnemyAgents.Add(eagent);
    }

    public void RemoveEnemyAgent(Transform transform)
    {
        int removeIndex = -1;
        for (int i = 0; i < EnemyAgents.Count; i++)
        {
            if (EnemyAgents[i].transform == transform)
            {
                removeIndex = i;
                break;
            }
        }
        if (removeIndex != -1)
            EnemyAgents.RemoveAt(removeIndex);
        else
            Debug.LogWarning("Tried to remove enemy agent that wasn't added");
    }

    public void AddSightAgent(Transform transform, float lineOfSight)
    {
        FowSightAgent agent = new FowSightAgent();
        agent.transform = transform;
        agent.lineOfSight = lineOfSight;
        SightAgents.Add(agent);
    }

    public void RemoveSightAgent(Transform transform)
    {
        int removeIndex = -1;
        for(int i = 0; i < SightAgents.Count; i++)
        {
            if (SightAgents[i].transform == transform)
            {
                removeIndex = i;
                break;
            }
        }
        if (removeIndex != -1)
            SightAgents.RemoveAt(removeIndex);
        else
            Debug.LogWarning("Tried to remove sight agent that isn't added");
    }
}
