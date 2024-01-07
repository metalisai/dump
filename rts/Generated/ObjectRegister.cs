// Automatically generated

using UnityEngine;

public enum GameObjectID {
	Artillery = 16,
	LaserTurret = 12,
	Turret = 1,
	Battery = 2,
	Generator = 3,
	WirePole = 4,
	WoodGenerator = 17,
	BasicEnemy = 10,
	Unit = 5,
	Lamp = 6,
	Tree = 11,
	Warehouse = 7,
	DiagonalWall = 13,
	EdgeWall = 15,
	GoldMine = 0,
	Nexus = 8,
	StoneMine = 9,
	StraightWall = 14,
	PathfindingDummy = 18,
};
public static class ObjectRegister { public static string GetResourcePath(GameObjectID id) { switch(id) {
case GameObjectID.Artillery: return @"PlaceableObjects\Defense\Artillery";
case GameObjectID.LaserTurret: return @"PlaceableObjects\Defense\LaserTurret";
case GameObjectID.Turret: return @"PlaceableObjects\Defense\Turret";
case GameObjectID.Battery: return @"PlaceableObjects\Electricity\Battery";
case GameObjectID.Generator: return @"PlaceableObjects\Electricity\Generator";
case GameObjectID.WirePole: return @"PlaceableObjects\Electricity\WirePole";
case GameObjectID.WoodGenerator: return @"PlaceableObjects\Electricity\WoodGenerator";
case GameObjectID.BasicEnemy: return @"PlaceableObjects\Unit\BasicEnemy";
case GameObjectID.PathfindingDummy: return @"PlaceableObjects\Unit\PathfindingDummy";
case GameObjectID.Unit: return @"PlaceableObjects\Unit\Unit";
case GameObjectID.Lamp: return @"PlaceableObjects\Utility\Lamp";
case GameObjectID.Tree: return @"PlaceableObjects\Utility\Tree";
case GameObjectID.Warehouse: return @"PlaceableObjects\Utility\Warehouse";
case GameObjectID.DiagonalWall: return @"StaticObjects\DiagonalWall";
case GameObjectID.EdgeWall: return @"StaticObjects\EdgeWall";
case GameObjectID.GoldMine: return @"StaticObjects\GoldMine";
case GameObjectID.Nexus: return @"StaticObjects\Nexus";
case GameObjectID.StoneMine: return @"StaticObjects\StoneMine";
case GameObjectID.StraightWall: return @"StaticObjects\StraightWall";
default: return null;
}
}public static string GetResourceName(GameObjectID id) { switch(id) {
case GameObjectID.Artillery: return @"Artillery";
case GameObjectID.LaserTurret: return @"LaserTurret";
case GameObjectID.Turret: return @"Turret";
case GameObjectID.Battery: return @"Battery";
case GameObjectID.Generator: return @"Generator";
case GameObjectID.WirePole: return @"WirePole";
case GameObjectID.WoodGenerator: return @"WoodGenerator";
case GameObjectID.BasicEnemy: return @"BasicEnemy";
case GameObjectID.PathfindingDummy: return @"PathfindingDummy";
case GameObjectID.Unit: return @"Unit";
case GameObjectID.Lamp: return @"Lamp";
case GameObjectID.Tree: return @"Tree";
case GameObjectID.Warehouse: return @"Warehouse";
case GameObjectID.DiagonalWall: return @"DiagonalWall";
case GameObjectID.EdgeWall: return @"EdgeWall";
case GameObjectID.GoldMine: return @"GoldMine";
case GameObjectID.Nexus: return @"Nexus";
case GameObjectID.StoneMine: return @"StoneMine";
case GameObjectID.StraightWall: return @"StraightWall";
default: return null;
}
}
}