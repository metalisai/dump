using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Interactable : ManagedMonoBehaviour
{
    public enum InteractionType
    {
        MoveUnit,
        ConnectWire,
        GatherResources,
        RotateKnob
    }

    public InteractionType InteractableType;
    public GameObject rootObject;

    void Awake()
    {
        gameObject.layer = 8;
        if (rootObject == null)
            rootObject = transform.root.gameObject;
    }
}
