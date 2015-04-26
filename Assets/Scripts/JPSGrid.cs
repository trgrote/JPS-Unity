using UnityEngine;
using System.Collections;

public enum eDirections
{
	DIR_NORTH      = 0,
	DIR_NORTH_EAST = 1,
	DIR_EAST       = 2,
	DIR_SOUTH_EAST = 3,
	DIR_SOUTH      = 4,
	DIR_SOUTH_WEST = 5,
	DIR_WEST       = 6,
	DIR_NORTH_WEST = 7,
}

public class Node
{
	public int[] jpDistances = new int[8];
	public bool isObstacle     = false;
	public bool isJumpPoint    = false;
}

public class Grid
{
	public Node[] gridNodes = new Node[0];
}
