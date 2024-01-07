using UnityEngine;
using System.Collections;

public enum InteractableObjectType
{
    GameItem,
    ItemBlueprint
}

public interface IInteractableObject
{
    void OnInteractionWithOtherObject(IInteractableObject otherObject);
    void OnPlayerPointingAt();
    void OnPointingAtEnd();
    InteractableObjectType GetType();
}
