using System;

// the sole purpose of this is to have a way
// an "owner" can cleanup its references to the owned item
// so for example when something is in player inventory
// and another object takes the ownership
// the Player knows to remove it from inventory

public interface IGameItemOwner
{
	void OnLoseOwnership(GameItem item);
	void OnGainOwnership(GameItem item);
    void OnOwnedItemTransform(GameItem oldItem, GameItem newItem);
}

