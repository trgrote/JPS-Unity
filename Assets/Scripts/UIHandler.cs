using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIHandler : MonoBehaviour 
{
	[SerializeField] private Button calcJPButton;
	[SerializeField] private Button calcStraightJPDistButton;
	[SerializeField] private Button calcDiagonalJPDistButton;
	[SerializeField] private Button calcWallDistButton;

	void Start()
	{
		SetInteractiveButtons();    // set initial state of buttons
	}

	public void SetInteractiveButtons()
	{
		// Turn them all off by default
		calcJPButton.interactable = false;
		calcStraightJPDistButton.interactable = false;
		calcDiagonalJPDistButton.interactable = false;
		calcWallDistButton.interactable = false;

		switch ( JPSState.state )
		{
			case eJPSState.ST_OBSTACLE_BUILDING:
				calcJPButton.interactable = true;
				break;
			case eJPSState.ST_PRIMARY_JPS_BUILDING:
				calcStraightJPDistButton.interactable = true;
				break;
			case eJPSState.ST_STRAIGHT_JPS_BUILDING:
				calcDiagonalJPDistButton.interactable = true;
				break;
			case eJPSState.ST_DIAGONAL_JPS_BUILDING:
				calcWallDistButton.interactable = true;
				break;
			case eJPSState.ST_WALL_DISTANCES_BUILT:
				break;
		}
	}
}
