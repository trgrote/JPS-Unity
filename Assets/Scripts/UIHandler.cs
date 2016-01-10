using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIHandler : MonoBehaviour 
{
	[SerializeField] private Button calcJPButton;
	[SerializeField] private Button calcStraightJPDistButton;
	[SerializeField] private Button calcDiagonalJPDistButton;
	[SerializeField] private Button calcWallDistButton;
	[SerializeField] private Button placeSearchMarkersButton;
	[SerializeField] private Button findPathButton;

	[SerializeField] private Text _descriptionTextBox;

	[Header("Descriptions")]
	[SerializeField, TextArea(3, 10)] private string initalDescription;
	[SerializeField, TextArea(3, 10)] private string calcJPDescription;
	[SerializeField, TextArea(3, 10)] private string calcJPStraightDescription;
	[SerializeField, TextArea(3, 10)] private string calcDiagonalJPDescription;
	[SerializeField, TextArea(3, 10)] private string calcWallDistDescription;
	[SerializeField, TextArea(3, 10)] private string placeSearchMarkersDescription;
	[SerializeField, TextArea(3, 10)] private string stepThroughDescription;

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
		placeSearchMarkersButton.interactable = false;
		findPathButton.interactable = false;

		switch ( JPSState.state )
		{
			case eJPSState.ST_OBSTACLE_BUILDING:
				calcJPButton.interactable = true;
				_descriptionTextBox.text = initalDescription;
				break;
			case eJPSState.ST_PRIMARY_JPS_BUILDING:
				calcStraightJPDistButton.interactable = true;
				_descriptionTextBox.text = calcJPDescription;
				break;
			case eJPSState.ST_STRAIGHT_JPS_BUILDING:
				calcDiagonalJPDistButton.interactable = true;
				_descriptionTextBox.text = calcJPStraightDescription;
				break;
			case eJPSState.ST_DIAGONAL_JPS_BUILDING:
				calcWallDistButton.interactable = true;
				_descriptionTextBox.text = calcDiagonalJPDescription;
				break;
			case eJPSState.ST_WALL_DISTANCES_BUILT:
				placeSearchMarkersButton.interactable = true;
				_descriptionTextBox.text = calcWallDistDescription;
				break;
			case eJPSState.ST_PLACE_SEARCH_ENDPOINTS:
				findPathButton.interactable = true;
				_descriptionTextBox.text = placeSearchMarkersDescription;
				break;
			case eJPSState.ST_FIND_PATH:
				// findPathButton.interactable = true;
				_descriptionTextBox.text = stepThroughDescription;
				break;
		}
	}
}
