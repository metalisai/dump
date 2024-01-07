// Author: Talis Tont
// Copyright (c) 2015 All Rights Reserved

using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(StaffMember))]
[RequireComponent(typeof(Dealer))]
public class DealerAI : BaseAI/*, IItemSelectHandler*/ {

    bool _selected = false;
    Arrow _arrow;

    void Start()
    {
        GetComponent<StaffMember>().Apartment = GetComponent<Dealer>().Apartment;
        Debug.Assert(GetComponent<StaffMember>().Apartment != null);
    }

	public void Move(Vector3 position)
    {
        var job = new MoveJob(position);
        JobQueue.EnQueue(job);
    }

    new void Update()
    {
        if(_selected)
        {
            _arrow.SetDestination(Camera.main.ViewportToWorldPoint(Input.mousePosition));
        }
        base.Update();
    }

    public bool OnSelected()
    {
        _arrow = Arrow.CreateArrow(transform.position, Camera.main.ViewportToWorldPoint(Input.mousePosition));

        _selected = true;
        return true;
    }

    public void OnDeSelecded()
    {
        _selected = false;
    }
}
