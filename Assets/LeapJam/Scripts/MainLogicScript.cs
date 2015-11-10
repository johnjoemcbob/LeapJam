using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

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
	public HandController InstructionLeapController;
	// The colours to use on the Image_Time UI element to display time running out
	public Color[] TimeColours;
	// The value (from 0->1) when this colour will first appear (starting at 1 with element 0)
	public float[] TimeColourRatios;
	// The left and right graphical hands of each character
	public HandModel[] Hands;
	// The name of each character
	public string[] CharacterName;
	// State References
	[Header( "State" )]
	public GameObject Menu;
	public GameObject Play;
	public GameObject Win;
	public GameObject WinPrefab;
	// UI References
	[Header( "UI" )]
	public Image Image_Background_Fade;
	public Image Image_Time;
	public Image Image_Timeout;
	public RawImage Image_Instruction;
	public Text Text_Instruction;
	public Text Text_Character;
	public Text[] Text_Score;
	// Audio Clip References
	[Header( "Audio" )]
	public AudioClip[] Audio_Win;
	public AudioClip[] Audio_Lose;
	// Leap Motion Recording References
	[Header( "Recording" )]
	public TextAsset Recording_Win;
	public TextAsset Recording_Lose;
	public TextAsset Recording_Timeout;

	// The id in the MiniGames array of the currently playing minigame
	private int CurrentGameID = -1;
	// The id of the last minigame
	private int LastGameID = 0;
	// The max id which has been reached in the current session (start at -1 so 0 game is run first)
	private int MaxGameID = 0;
	// The total score for this play session
	private int[] Score = new int[] { 0, 0, 0, 0 };
	// The score shake interpolation value for each player's score
	private float[] ScoreShake = new float[] { 0, 0, 0, 0 };
	// The script in the scene which inherits from BaseMiniGameScript, will be a child of CurrentGame
	private BaseMiniGameScript CurrentGameLogic;
	// The current player (for hotseat multiplay, in single it will always be 0)
	private int CurrentPlayer = -1;
	// The max players (for hotseat multiplay, in single it will always be 1)
	private int MaxPlayers = 1;
	// The current countdown to main menu switch (Timeout gesture functionality)
	private float CurrentTimeout = 1;
	// The completion flag of each player for the current minigame
	private bool[] CompletedGame = new bool[] { false, false, false, false };
	// The fade in/out value of the character text (0->1)
	private float CharacterTextFade = 0;
	// The fade in/out direction of the character text (-1/0/1)
	private float CharacterTextFadeDirection = 0;
	// Flag for whether or not this is the final attempt on this minigame
	private bool FinalAttempt = false;
	// The current fade alpha value for the instruction image
	private float InstructionAlpha = 0;
	// The current fade alpha direction for the instruction image
	private float InstructionAlphaDirection = 0;

	void Update()
	{
		if ( Input.GetKeyDown( KeyCode.Escape ) )
		{
			RunWinLose( true );
			EndCurrentGame();
		}

		Update_CheckTimeout();
		Update_ScoreShake();
		Update_CharacterText();

		// Start instruction fade when the recording ends
		if ( ( InstructionAlphaDirection == 1 ) && ( InstructionLeapController.GetRecordingProgress() > 0.8f ) )
		{
			// Reset the instruction fade out time
			InstructionAlpha = 1;
			InstructionAlphaDirection = -1;
		}
		// Instruction fading using Color.Lerp
		InstructionAlpha += Time.deltaTime * InstructionAlphaDirection;
		Image_Instruction.color = Color.Lerp( new Color( 255, 255, 255, 0 ), Color.white, InstructionAlpha );
	}

	// Check for the 'timeout' return to main menu gesture
	private void Update_CheckTimeout()
	{
		// Cannot return to the menu while on the menu
		if ( Menu.activeSelf )
		{
			CurrentTimeout = 1;
			Image_Timeout.fillAmount = 0;
			Image_Timeout.transform.GetChild( 0 ).gameObject.SetActive( false );
			return;
		}

		// Check gestures
		bool timeout = false;
			// There must be two hands present
			HandModel[] hands = LeapController.GetAllPhysicsHands();
			if ( hands.Length > 1 )
			{
				// Check for proximity of hands
				float distance = Vector3.Distance( hands[0].GetPalmPosition(), hands[1].GetPalmPosition() );
				if ( distance < 1.5f )
				{
					Vector3 normal, direction;
					// Check the direction of hand 0 against the normal of hand 1
					direction = hands[0].GetPalmDirection();
					normal = -hands[1].GetPalmNormal();
					distance = Vector3.Distance( direction, normal );
					if ( distance < 0.4f )
					{
						timeout = true;
					}
					// Check the direction of hand 1 against the normal of hand 0
					direction = hands[1].GetPalmDirection();
					normal = -hands[0].GetPalmNormal();
					distance = Vector3.Distance( direction, normal );
					if ( distance < 0.4f )
					{
						timeout = true;
					}
				}
			}
		if ( timeout )
		{
			// Decrease time until main menu switch
			CurrentTimeout -= Time.deltaTime;
			CurrentTimeout = Mathf.Max( CurrentTimeout, 0 );
			if ( CurrentTimeout == 0 )
			{
				StartMenu();
				EndPlay();
			}
		}
		else
		{
			// Increase time until main menu switch
			CurrentTimeout += Time.deltaTime;
			CurrentTimeout = Mathf.Min( CurrentTimeout, 1 );
		}
		// Fill circular progress for returning to menu
		float fill = 1 - CurrentTimeout;
		Image_Timeout.fillAmount = fill;
		if ( fill > 0 )
		{
			Image_Timeout.transform.GetChild( 0 ).gameObject.SetActive( true );
		}
		else
		{
			Image_Timeout.transform.GetChild( 0 ).gameObject.SetActive( false );
		}
	}

	public void Update_ScoreShake()
	{
		for ( int player = 0; player < 4; player++ )
		{
			Text_Score[player].rectTransform.localRotation = Quaternion.Euler( 0, 0, Mathf.Cos( ( ScoreShake[player] * 5 ) - 2.5f ) * 20 );
			ScoreShake[player] = Mathf.Max( 0, ScoreShake[player] / 1.1f );
		}
	}

	public void Update_CharacterText()
	{
		if ( !CurrentGameLogic ) return;

		// Character text
		// Don't fade if waiting for hands to appear in hotseat multiplayer OR
		// If the instructions are still displaying
		if (
			CurrentGameLogic.GetInstructionFinished() &&
			(
				( Menu.activeSelf ) ||
				CurrentGameLogic.GetGameStarted() ||
				( GetMaxPlayers() == 1 ) ||
				( !CurrentGameLogic.GetWaitingHands() ) ||
				( CharacterTextFadeDirection == 1 )
			)
		)
		{
			CharacterTextFade += Time.deltaTime * CharacterTextFadeDirection;
			if ( CharacterTextFade < 0 )
			{
				CharacterTextFadeDirection = 0;
				CharacterTextFade = 0;
			}
			if ( CharacterTextFade > 1 )
			{
				CharacterTextFadeDirection = -1;
				CharacterTextFade = 1;
			}
			Text_Character.color = new Color( 255, 255, 255, CharacterTextFade );
		}
	}

	// Called from the PlayButtonScript on the main menu
	public void StartPlay()
	{
		Menu.SetActive( false );
		SetPlayers( MaxPlayers );
		//SetPlayers( 1 );
		ChooseGame();
	}

	// Called from the ResetButtonScript on the main menu
	public void Reset()
	{
		CurrentPlayer = -1;
		CurrentGameID = 0;
		LastGameID = 0;
		MaxGameID = 0;
		for ( int player = 0; player < 4; player++ )
		{
			Score[player] = 0;
			Text_Score[player].text = "0";
		}
		CurrentGameLogic = null;
	}

	// Called from the Update_CheckTimeout function
	public void EndPlay()
	{
		//SetPlayers( 0 );
		SetBackgroundAlpha( 0 );
		SetTime( 0 );
		ExitGame( CurrentGameID );
	}

	// Called from any of the minigames in order to affect the progress of the time image (0->1)
	public void SetTime( float time )
	{
		Image_Time.fillAmount = time;
		if ( time != 0 )
		{
			// Lookup whether or not we need to change the colour of the timer
			for ( int colour = TimeColourRatios.Length - 1; colour >= 0; colour-- )
			{
				float colourtime = TimeColourRatios[colour];
				if ( time <= colourtime )
				{
					Image_Time.color = TimeColours[colour];
					break;
				}
			}
		}
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
		// Extra logic to stop bug (when all had completed game except last player, first player could play again)
		if ( FinalAttempt )
		{
			CompletedGame[CurrentPlayer] = true;
		}

		// Run win logic
		if ( win )
		{
			// Add to total score
			Score[CurrentPlayer] += CurrentGameLogic.Score;
			ScoreShake[CurrentPlayer] = 5;
			Text_Score[CurrentPlayer].text = Score[CurrentPlayer].ToString();

			// Play cheer/clap effect
			GetComponent<AudioSource>().clip = Audio_Win[UnityEngine.Random.Range( 0, Audio_Win.Length )];
			GetComponent<AudioSource>().Play();

			// All players before this one have run out of attempts
			for ( int player = 0; player < CurrentPlayer + 1; player++ )
			{
				CompletedGame[player] = true;
			}

			// If the player did win and this is a more difficult game, then store as the hardest completed
			MaxGameID = Mathf.Max( CurrentGameID, MaxGameID );
			FinalAttempt = true;
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
		string character_linetwo = "YOU'RE UP!";

		// Only advance if all players have had their final attempt
		if ( LastGameID == MaxGameID )
		{
			// Ensure that each player has a last attempt
			bool allattempted = true;
				for ( int player = 0; player < MaxPlayers; player++ )
				{
					if ( !CompletedGame[player] )
					{
						allattempted = false;
					}
				}
			if ( allattempted )
			{
				MaxGameID++;

				// Reset player attempts (hotseat multiplayer)
				FinalAttempt = false;
				for ( int player = 0; player < MaxPlayers; player++ )
				{
					CompletedGame[player] = false;
				}

				// Show win screen
				if ( MaxGameID >= MiniGames.Length )
				{
					WinGame();
					return;
				}
			}
		}

		// Completed the current minigame
		if ( FinalAttempt )
		{
			CompletedGame[CurrentPlayer] = true;
			character_linetwo = "LAST CHANCE!";
		}

		// Next player
		LeapController.DestroyAllHands();
		CurrentPlayer++;
		if ( CurrentPlayer >= MaxPlayers ) CurrentPlayer = 0;
		int hand = CurrentPlayer * 2;
		LeapController.GetComponent<HandController>().leftGraphicsModel = Hands[hand];
		LeapController.GetComponent<HandController>().rightGraphicsModel = Hands[hand + 1];
		// In multiplayer, display the next player's name
		if ( MaxPlayers > 1 )
		{
			CharacterTextFade = 0;
			CharacterTextFadeDirection = 1;
			Text_Character.text = CharacterName[CurrentPlayer] + "\n(PLACE BOTH HANDS IN THE SCENE)\n" + character_linetwo;
		}

		// Reset the instruction fade in time
		InstructionAlpha = 0;
		InstructionAlphaDirection = 1;

		// Choose from a selection of minigames near the current level
		//int mingame = Mathf.Max( 0, MaxGameID - 5 );
		//int maxgame = Mathf.Min( MaxGameID + 2, MiniGames.Length );
		//int rndgame = LastGameID;
		//    while ( rndgame == LastGameID )
		//    {
		//        rndgame = UnityEngine.Random.Range( mingame, maxgame );
		//    }
		//EnterGame( rndgame );
		EnterGame( Mathf.Min( MaxGameID, MiniGames.Length - 1 ) );
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
		CurrentGameLogic.InstructionLeapController = InstructionLeapController;

		// Flag the minigame as loaded
		CurrentGameID = gameid;
	}

	// Call cleanup and destroy of the specified minigame (looked up from MiniGames array)
	// Expects EnterGame to have been called to load in a game already
	private void ExitGame( int gameid )
	{
		// The specified minigame must be currently loaded
		if ( !CurrentGame ) return;
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

	// Called from ChooseGame if all players have completed the final level
	// Used to stop the play state and show the final scores
	private void WinGame()
	{
		// Do something else if it was singleplayer
		//if ( MaxPlayers == 1 ) return;

		// Sort the scores to find the winner
		Dictionary<int, int> scores = new Dictionary<int, int>();
		for ( int player = 0; player < 4; player++ )
		{
			scores.Add( player, Score[player] );
		}
		var scores_sorted = from entry in scores orderby entry.Value descending select entry;

		// Display the win state
		//Win.SetActive( true );
		Win = (GameObject) Instantiate( WinPrefab );

		// Change the number of hands depending on the number of players (2-3)
		for ( int player = 0; player < 4; player++ )
		{
			// Enable if the player was active during the game
			Win.transform.GetChild( player ).gameObject.SetActive( player < MaxPlayers );
		}

		// Change the hand model of each pedestal
		for ( int position = 0; position < 4; position++ )
		{
			// Win -> PedestalNum -> AnimationParent -> HandController
			HandController controller = GetWinHandController( position );

			int hand = scores_sorted.ElementAt( position ).Key * 2;
			controller.leftGraphicsModel = Hands[hand];
			controller.rightGraphicsModel = Hands[hand + 1];
		}

		// Change the score displayed on each pedestal
		for ( int position = 0; position < 4; position++ )
		{
			// Win -> PedestalNum -> Child1(Canvas) -> Child0 (Score)
			Text text = (Text) Win.transform.GetChild( position ).GetChild( 1 ).GetChild( 0 ).GetComponent<Text>();
			text.text = scores_sorted.ElementAt( position ).Value.ToString();
		}

		// Change instructions to display the winner's name
		Text_Instruction.text = CharacterName[scores_sorted.ElementAt( 0 ).Key] + " WON!";
		Text_Character.color = new Color( 0, 0, 0, 0 );

		// Reset the instruction fade in time
		InstructionAlpha = 0;
		InstructionAlphaDirection = 1;

		// Set the instruction hands to display the tutorial for timeout (Return to Menu) gesture
		InstructionLeapController.GetLeapRecorder().Load( Recording_Timeout );

		// Stop the game state (last because it resets values)
		Reset();
		EndPlay();
	}

	// Called using the timeout gesture to return to the main menu
	private void StartMenu()
	{
		// Change states
		Menu.SetActive( true );
		if ( Win )
		{
			Destroy( Win );
			Win = null;
		}

		// Set the title back to the game name
		Text_Instruction.text = "-- LEAP --\n-- CARNIVAL --";
	}

	private HandController GetWinHandController( int position )
	{
		return (HandController) Win.transform.GetChild( position ).GetChild( 0 ).GetChild( 0 ).GetComponent<HandController>();
	}

	// Set the number of players in the current game
	public void SetPlayers( int players )
	{
		MaxPlayers = players;

		// Hide all player HUDs
		foreach ( Text score in Text_Score )
		{
			score.gameObject.SetActive( false );
		}

		// Enable those which are in play again
		int player = 0;
		foreach ( Text score in Text_Score )
		{
			if ( player >= players )
			{
				break;
			}
			score.gameObject.SetActive( true );
			player++;
		}
	}

	public int GetMaxPlayers()
	{
		return MaxPlayers;
	}
}