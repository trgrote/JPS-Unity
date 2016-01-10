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

	[SerializeField] private GameObject _gridSliders;
	[SerializeField] private GridView _gridView;

	[SerializeField] private Slider _widthSlider;
	[SerializeField] private InputField _widthText;
	[SerializeField] private Slider _heightSlider;
	[SerializeField] private InputField _heightText;

	[Header("Descriptions")]
	[SerializeField, TextArea(3, 10)] private string initalDescription;
	[SerializeField, TextArea(3, 10)] private string calcJPDescription;
	[SerializeField, TextArea(3, 10)] private string calcJPStraightDescription;
	[SerializeField, TextArea(3, 10)] private string calcDiagonalJPDescription;
	[SerializeField, TextArea(3, 10)] private string calcWallDistDescription;
	[SerializeField, TextArea(3, 10)] private string placeSearchMarkersDescription;
	[SerializeField, TextArea(3, 10)] private string stepThroughDescription;
	[SerializeField, TextArea(3, 10)] private string pathFoundDescription;

	void Start()
	{
		SetInteractiveButtons();    // set initial state of buttons

		// Set Values of Sliders
		_widthSlider.value = _gridView.rowSize;
		_widthText.text = _widthSlider.value.ToString();

		_heightSlider.value = _gridView.numBlocks / _gridView.rowSize;
		_heightText.text = _heightSlider.value.ToString();
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
		_gridSliders.SetActive(false);

		switch ( JPSState.state )
		{
			case eJPSState.ST_OBSTACLE_BUILDING:
				calcJPButton.interactable = true;
				_descriptionTextBox.text = initalDescription;
				_gridSliders.SetActive(true);
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
				findPathButton.interactable = true;
				_descriptionTextBox.text = stepThroughDescription;
				break;
			case eJPSState.ST_PATH_FIND_COMPLETE:
				// Change Text depending on the result
				placeSearchMarkersButton.interactable = true;    // re-enable the place search path stuff
				_descriptionTextBox.text = pathFoundDescription + "\n" + 
					( JPSState.LastPathFound ? "PATH FOUND!" : "FAILED TO FIND A PATH" );
				break;
		}
	}

	public void OnWidthSliderChange()
	{
		// Update Text
		_widthText.text = _widthSlider.value.ToString();
		// Set new size of grid and resize it
		_gridView.rowSize = (int) _widthSlider.value;
		_gridView.numBlocks = _gridView.rowSize * (int) _heightSlider.value;
	}

	public void OnHeightSliderChange()
	{
		// Update Text
		_heightText.text = _heightSlider.value.ToString();
		// Set new size of grid and resize it	
		_gridView.numBlocks = _gridView.rowSize * (int) _heightSlider.value;
	}
}
