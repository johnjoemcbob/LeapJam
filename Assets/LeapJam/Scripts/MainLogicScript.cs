using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class MainLogicScript : MonoBehaviour
{
	[Header( "Main" )]
	// Array of possible minigames
	public GameObject[] MiniGames;
	// Parents for different types of objects in the scene
	public GameObject DefaultScene;
	public GameObject CurrentGame;
	public GameObject MainCamera;
	// Reference to the Leap Motion Controller object
	public HandController LeapController;
	// UI References
	[Header( "UI" )]
	public GameObject Menu;
	public Image Image_Background_Fade;
	public Text Text_Instruction;
	public Text Text_Score;
	// Audio Clip References
	[Header( "Audio" )]
	public AudioClip[] Audio_Win;
	public AudioClip[] Audio_Lose;

	// The id in the MiniGames array of the currently playing minigame
	private int CurrentGameID = -1;
	// The id of the last minigame
	private int LastGameID = -1;
	// The max id which has been reached in the current session (start at -1 so 0 game is run first)
	private int MaxGameID = -1;
	// The total score for this play session
	private int Score = 0;
	// The script in the scene which inherits from BaseMiniGameScript, will be a child of CurrentGame
	private BaseMiniGameScript CurrentGameLogic;

	void Update()
	{
		if ( Input.GetKeyDown( KeyCode.Escape ) )
		{
			RunWinLose( true );
			EndCurrentGame();
		}
	}

	// Called from the PlayButtonScript on the main menu
	public void StartPlay()
	{
		Menu.SetActive( false );
		ChooseGame();
	}

	// Called from any of the states in order to affect the alpha transparency of the background
	public void SetBackgroundAlpha( float alpha )
	{
		Image_Background_Fade.color = new Color( 0, 0, 0, alpha );
		MainCamera.transform.localPosition = new Vector3( 0, 0, 1 - alpha - 1 );
	}

	// Callback from the minigame script to run win/lose logic
	public void RunWinLose( bool win )
	{
		// Run win logic
		if ( win )
		{
			// Add to total score
			Score += CurrentGameLogic.Score;
			Text_Score.text = Score.ToString();
			// Play cheer/clap effect
			GetComponent<AudioSource>().clip = Audio_Win[UnityEngine.Random.Range( 0, Audio_Win.Length )];
			GetComponent<AudioSource>().Play();

			// If the player did win and this is a more difficult game, then store as the hardest completed
			MaxGameID = Mathf.Max( CurrentGameID, MaxGameID );
		}
		// Run lose logic
		else
		{
			// Play boo/trombone effect
			GetComponent<AudioSource>().clip = Audio_Lose[UnityEngine.Random.Range( 0, Audio_Lose.Length )];
			GetComponent<AudioSource>().Play();
		}
	}

	// Callback from the minigame script to end it
	public void EndCurrentGame()
	{
		// Exit the minigame
		ExitGame( CurrentGameID );

		// Choose the next minigame
		ChooseGame();
	}

	// Called to choose the next appropriately difficult minigame
	private void ChooseGame()
	{
		// Choose from a selection of minigames near the current level
		//int mingame = Mathf.Max( 0, MaxGameID - 5 );
		//int maxgame = Mathf.Min( MaxGameID + 2, MiniGames.Length );
		//int rndgame = LastGameID;
		//    while ( rndgame == LastGameID )
		//    {
		//        rndgame = UnityEngine.Random.Range( mingame, maxgame );
		//    }
		//EnterGame( rndgame );
		EnterGame( Mathf.Min( MaxGameID + 1, MiniGames.Length - 1 ) );
	}

	// Instantiate and call initialization of the specified minigame (looked up from MiniGames array)
	// Expects ExitGame to have been called on the previous game already
	private void EnterGame( int gameid )
	{
		// There must be no game currently loaded
		if ( CurrentGameID != -1 ) return;
		// The game must exist in the array
		if ( ( gameid < 0 ) || ( gameid >= MiniGames.Length ) ) return;

		// Load the prefab and parent it to the CurrentGame parent
		GameObject minigame = Instantiate( MiniGames[gameid] );
		minigame.transform.parent = CurrentGame.transform;

		// Find the minigame logic script and add a reference to this on it
		CurrentGameLogic = (BaseMiniGameScript) minigame.GetComponentInChildren<BaseMiniGameScript>();
		CurrentGameLogic.MainLogic = this;
		CurrentGameLogic.LeapController = LeapController;

		// Flag the minigame as loaded
		CurrentGameID = gameid;
	}

	// Call cleanup and destroy of the specified minigame (looked up from MiniGames array)
	// Expects EnterGame to have been called to load in a game already
	private void ExitGame( int gameid )
	{
		// The specified minigame must be currently loaded
		if ( CurrentGameID != gameid ) return;

		// Cleanup any children of the CurrentGame parent
		int maxchild = CurrentGame.transform.childCount;
		for ( int childid = 0; childid < maxchild; childid++ )
		{
			GameObject child = CurrentGame.transform.GetChild( childid ).gameObject;
			Destroy( child );
			child = null;
		}

		// Cleanup the current minigame's logic script
		Destroy( CurrentGameLogic );
		CurrentGameLogic = null;

		// Flag currently no minigame
		LastGameID = CurrentGameID;
		CurrentGameID = -1;
	}
}