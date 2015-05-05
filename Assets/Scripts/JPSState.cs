using UnityEngine;
using System.Collections;

public enum eJPSState
{
	ST_OBSTACLE_BUILDING      = 0,
	ST_PRIMARY_JPS_BUILDING   = 1,
	ST_STRAIGHT_JPS_BUILDING  = 2,
	ST_DIAGONAL_JPS_BUILDING  = 3,
	ST_WALL_DISTANCES_BUILT   = 4
}

public static class JPSState
{
	public static eJPSState state = eJPSState.ST_OBSTACLE_BUILDING;
}
