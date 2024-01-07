using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;

enum GridEditorMode
{
    None,
    Ceilings,
    Floors
}

[CustomEditor(typeof(Grid))]
public class GridEditor : Editor {

	bool _editing = false;
    GridEditorMode _mode = GridEditorMode.None;

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		Grid myScript = (Grid)target;
		/*if(GUILayout.Button("Build Object"))
		{
			_editing = !_editing;
		}*/

        if (GUILayout.Button("Remove inactive"))
        {
            myScript.InactiveCeilingCells.Clear();
            myScript.InactiveFloorCells.Clear();
        }

        _mode = (GridEditorMode)EditorGUILayout.EnumPopup(_mode);
    }

	public void OnSceneGUI()
	{
		//Grid gird = Selection.activeGameObject.GetComponentInChildren<Grid> ();

		if (!_editing) {

			//int controlId = GUIUtility.GetControlID(FocusType.Passive);

			if (Event.current.type == EventType.MouseUp) {
				//Debug.Log ("mouiseyup");

				if (Event.current.button == 0) {
					Ray worldRay = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);

					RaycastHit hit; // if the ray hits, info will be stored here

                    GridCellType ctype = GridCellType.All;

                    if(_mode == GridEditorMode.Ceilings)
                    {
                        ctype = GridCellType.CeilingCell;
                    }
                    else if(_mode == GridEditorMode.Floors)
                    {
                        ctype = GridCellType.FloorCell;
                    }

                    int mask = GridRayCaster.TypeToMask(ctype);

                    //Debug.Log (mask.value);
                    // TODO: check if the apartment matches to the active one
                    if (Physics.Raycast(worldRay, out hit, 35f, mask)) {
                        Grid grid = hit.collider.gameObject.GetComponentInParent<Grid>();
                        //Debug.Log("Hit");
                        //Debug.DrawLine(hit.point, hit.point + Vector3.up);
                        var cell = grid.GetClosestCell(hit.point, ctype==GridCellType.All?GridCellType.FloorCell:ctype);

                        var aarray = (hit.collider.gameObject.layer == 9 || hit.collider.gameObject.layer == 14) ? grid.InactiveFloorCells : grid.InactiveCeilingCells;

                        if (!aarray.Any(x => x.Equals(cell.Coord)))
                        {
                            aarray.Add(cell.Coord);
                        }
                        else
                        {
                            aarray.Remove(cell.Coord);
                        }

					}
					//GUIUtility.hotControl = controlId;
					Event.current.Use();
				}
			} else if (Event.current.type == EventType.MouseDown && Event.current.button == 0) {
				GUIUtility.hotControl = 0;
				Event.current.Use();
			}
		}
	}
}
