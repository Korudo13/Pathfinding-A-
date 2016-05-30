using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class _Grid : MonoBehaviour {

	public bool onlyDisplayPathGizmos; 
	public Transform player;
	public LayerMask unwalkableMask;
	public Vector2 gridWorldSize;
	public float nodeRadius;
	_Node[,]grid;

	float nodeDiameter;
	int gridSizeX, gridSizeY;

	void Start(){
		//figure out how many nodes can we fit into the grid, based on our nodeRadius

		nodeDiameter = nodeRadius * 2; //standard diameter equals radius times two 
		gridSizeX = Mathf.RoundToInt (gridWorldSize.x / nodeDiameter); // take gridWorldSize.x / nodeDiameter, then round (because no half nodes can exist)
		gridSizeY = Mathf.RoundToInt (gridWorldSize.y / nodeDiameter); // same for gridWorldSize.y
		CreateGrid ();
	}

	public int MaxSize{
		get{
			return gridSizeX * gridSizeY;
		}
	}
		
	void CreateGrid(){
		grid = new _Node[gridSizeX, gridSizeY]; //new 2D Array of Nodes

		//get bottom left corner of the world (far left = x, bottom = y)
		Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;  

		//loop through all positions that the nodes will be in to do collision checks - to determine if nodes are walkable or not, X then Y
		for (int x = 0; x < gridSizeX; x++) {
			for (int y = 0; y < gridSizeY; y++) {
				//as x increases, go in increments of nodeDiameter along the world until reaching the edge
				Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius)
					//do the same for y axis (z axis in world space)
				                     + Vector3.forward * (y * nodeDiameter * nodeRadius); 

				//(above) loop get each point the nodes will occupy in the world first, 

				//(below, next) collision check for each of those points. True if don't collide with anything in Unwalkable Layer Mask
				bool walkable = !(Physics.CheckSphere (worldPoint, nodeRadius, unwalkableMask)); 

				//new node, using the _Node constructor
				grid [x, y] = new _Node (walkable, worldPoint, x, y);
			} 
		}
	}

	public List<_Node> GetNeighbors(_Node node){
		//list of nodes initialized as a new empty list of nodes
		List<_Node> neighbors = new List<_Node> ();

		for (int x = -1; x <= 1; x++) {
			for (int y = -1; y <= 1; y++) {
				if (x == 0 && y == 0)
					continue;

				int checkX = node.gridX + x;
				int checkY = node.gridY + y;

				//check to see if node is inside the grid
				if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) {
					neighbors.Add (grid [checkX, checkY]);
				}
			}
		}

		return neighbors; 
	}


	public _Node NodeFromWorldPoint(Vector3 worldPosition){
		//Converts worldPosition into percentage (x,y). How far along an object is on the grid
		//X:   far left = 0%, middle = 0.5%, far right = 1%
		float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x; 
		float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y; 

		//clamps between 0 - 1, pragmatic programming. Doesn't trust myself, ensures that it works.
		percentX = Mathf.Clamp01 (percentX);
		percentY = Mathf.Clamp01 (percentY);

		//arrays are 0-based. (1-1 = 0) * 1 = 0; Meant to ensure not getting outside the Array.
		int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
		int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
		return grid [x, y];
	}

	public List<_Node> path;

	void OnDrawGizmos(){
		//gridWorldSize.y is representing the Z-Axis in 3D space from a top-down perspective
		Gizmos.DrawWireCube (transform.position, new Vector3 (gridWorldSize.x, 1, gridWorldSize.y));

		if (onlyDisplayPathGizmos) {
			if (path != null) {
				foreach (_Node n in path) {
					Gizmos.color = Color.white; 
					Gizmos.DrawCube (n.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
				}
			}
		}

			else{

				if (grid != null) {
					foreach (_Node n in grid) {
						Gizmos.color = (n.walkable) ? Color.black : Color.red;  

						if (path != null)
						if (path.Contains (n)) { 
							Gizmos.color = Color.white; 
							Gizmos.DrawCube (n.worldPosition, Vector3.one * (nodeDiameter - 0.1f));
						}
					}
				}
			}
		}
	}


