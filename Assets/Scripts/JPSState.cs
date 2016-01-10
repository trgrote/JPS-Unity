using UnityEngine;
using System.Collections;

public enum eJPSState
{
	ST_OBSTACLE_BUILDING      = 0,
	ST_PRIMARY_JPS_BUILDING,
	ST_STRAIGHT_JPS_BUILDING,
	ST_DIAGONAL_JPS_BUILDING,
	ST_WALL_DISTANCES_BUILT,
	ST_PLACE_SEARCH_ENDPOINTS,
	ST_FIND_PATH,
	ST_PATH_FIND_COMPLETE,
}

public static class JPSState
{
	public static eJPSState state = eJPSState.ST_OBSTACLE_BUILDING;
	public static bool LastPathFound = false;
}
