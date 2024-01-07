using UnityEngine;
using System.Collections;

public static class ObjectConverter
{
	public static GameItem AttemptCraft(GameItem[] input, ObjectId objectToCraft)
	{
		switch (objectToCraft) {
		case ObjectId.Fireplace:
			if(input[0].ObjectId == ObjectId.Stick)
			{
				var fp = ObjectDatabase.CreateObject(objectToCraft).GetComponent<Fireplace>();
				fp.TryAddFuel(input[0]);
				return fp;
			}
			break;
		}
		return null;
	}
}
