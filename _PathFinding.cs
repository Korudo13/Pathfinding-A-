using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class _PathFinding : MonoBehaviour {

	//public Transform seeker, target;
	_PathRequestManager requestManager;

	_Grid grid; 

	void Awake(){
		requestManager = GetComponent<_PathRequestManager> ();
		grid = GetComponent<_Grid> (); 
	}
		

	public void StartFindPath(Vector3 startPos, Vector3 targetPos){
		StartCoroutine (FindPath (startPos, targetPos));
	}

	IEnumerator FindPath(Vector3 startPos, Vector3 targetPos){ 

		Stopwatch sw = new Stopwatch ();
		sw.Start (); 

		Vector3[] waypoints = new Vector3[0];
		bool pathSuccess = false;

		_Node startNode = grid.NodeFromWorldPoint (startPos); 
		_Node targetNode= grid.NodeFromWorldPoint (targetPos); 

		if (startNode.walkable && targetNode.walkable) {

			_Heap<_Node> openSet = new _Heap<_Node> (grid.MaxSize);   
			HashSet<_Node> closedSet = new HashSet<_Node> ();
			openSet.Add (startNode); 

			while (openSet.Count > 0) {
				_Node currentNode = openSet.RemoveFirst ();
				closedSet.Add (currentNode); 

				if (currentNode == targetNode) {
					sw.Stop ();  
					print ("Path found: " + sw.ElapsedMilliseconds + " ms"); 
					pathSuccess = true;
					break;
				}

				// foreach neighbor of the current node
				//   if neighbor is not traversable or neighbor is in CLOSED
				//       skip to the next neighbor
				foreach (_Node neighbor in grid.GetNeighbors(currentNode)) {
					if (!neighbor.walkable || closedSet.Contains (neighbor)) {
						continue;
					}
		

					//check if new path is shorter than the old path OR if neighbor is not in OPEN list
					int newMovementCostToNeighbor = currentNode.gCost + GetDistance (currentNode, neighbor);
					if (newMovementCostToNeighbor < neighbor.gCost || !openSet.Contains (neighbor)) {
						neighbor.gCost = newMovementCostToNeighbor;
						neighbor.hCost = GetDistance (neighbor, targetNode);
						neighbor.parent = currentNode;

						if (!openSet.Contains (neighbor)) {
							openSet.Add (neighbor); 
						} else
							openSet.UpdateItem (neighbor);
					}
				}
			}
		}

		yield return null;

		if (pathSuccess) {
			waypoints = RetracePath (startNode, targetNode);   
		}

		requestManager.FinishedProcessingPath (waypoints, pathSuccess);
	}


	Vector3[] RetracePath(_Node startNode, _Node endNode){
		List<_Node> path = new List<_Node> ();
			_Node currentNode = endNode; 

		while (currentNode != startNode) {
			path.Add (currentNode);
			currentNode = currentNode.parent;
		}

		path.Reverse ();

		//grid.path = path;

		/*[Unity Pathfinding Tutorial] Units - Sebastian League (Pathfinding 5/6)
		 * 
		 * 12:40
		 * 
		 */
	}
		
	int GetDistance(_Node nodeA, _Node nodeB){
		int dstX = Mathf.Abs (nodeA.gridX - nodeB.gridX);
		int dstY = Mathf.Abs (nodeA.gridY - nodeB.gridY);

		if (dstX > dstY) 
			return 14 * dstY + 10 * (dstX - dstY);
		
	return 14 * dstX + 10 * (dstY - dstX);
		}
	}

/*Pseudo-code 
	 * 
	 * 
	 * OPEN // the set of nodes to be evaluated
	 * CLOSED // the set of nodes already evaluated
	 * 
	 * loop
	 *   current = node in OPEN with the lowest f_cost
	 *   remove current from OPEN
	 *   add current to CLOSED
	 * 
	 *   if current is the target node // path has been found
	 *     return
	 * 
	 *   foreach neighbor of the current node
	 *     if neighbor is not traversable or neighbor is in CLOSED
	 *       skip to the next neighbor
	 * 
	 *   if new path to neighbor is shorter OR neighbor is not in OPEN
	 *     set f_cost of neighbor
	 *     set parent of neighbor to current
	 *     if neighbor is not in OPEN
	 *       add neighbor to OPEN
	 */