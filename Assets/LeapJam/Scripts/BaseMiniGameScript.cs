using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BaseMiniGameScript : MonoBehaviour
{
	[Header( "Base" )]
	// Reference to the main game logic
	public MainLogicScript MainLogic;
	// Reference to the Leap Motion Controller object
	public HandController LeapController;
	public HandController InstructionLeapController;

	// The time for which the game lasts
	public float MaxPreGameTime = 1;
	public float MaxGameTime = 5;
	public float MaxPostGameTime = 1;
	// The instructions to display
	public string Instructions;
	public string Instructions2;
	// The instruction recording file to play (if any)
	public TextAsset InstructionRecording;
	// The score to award on completion
	public int Score;

	// The current time through the minigame
	protected float GameTime = 0;
	// Flag after the game has started
	protected bool GameStarted = false;
	// Flag after the game has ended
	protected bool GameEnded = false;
	// Flag for whether or not the minigame was won
	protected bool GameWon = false;
	// Flag while waiting for old player hands to be removed in multiplayer
	protected bool NewGameHandsRemoved = false;
	// Flag while waiting for new player hands to enter in multiplayer
	protected bool NewGameWaitingHands = true;
	// String to display when the minigame is won
	protected string WinMessage = "";

	public void Start()
	{
		PlayInstruction();
	}

	void Update()
	{
		// Must have a reference, or functionality will be unavailable
		if ( !MainLogic )
		{
			Destroy( this );
			return;
		}

		// Increment the amount of time this game has been active for
		// Actual game end logic is checked within individual minigames,
		// to allow for some to work around the time limit if need be
		GameTime += Time.deltaTime;
		// In multiplayer the start timer needs to pause to allow for player repositioning OR
		// While instructions are being displayed
		if (
			( ( !GameStarted ) && ( MainLogic.GetMaxPlayers() > 1 ) && NewGameWaitingHands ) ||
			( ( !GameStarted ) && ( !GetInstructionFinished() ) )
		)
		{
			GameTime -= Time.deltaTime;

			// Check if the hands are present now, after being removed from the scene
			int hands = LeapController.GetAllGraphicsHands().Length;
			if ( hands == 0 )
			{
				NewGameHandsRemoved = true;
			}
			else if ( hands == 2 )
			{
				if ( NewGameHandsRemoved )
				{
					NewGameWaitingHands = false;
				}
			}
		}

		if ( GameEnded )
		{
			Update_PostGame();
			// Flag hands as needing to be changed in the next game
			NewGameHandsRemoved = false;
			NewGameWaitingHands = true;
		}
		else if ( GameStarted )
		{
			Update_Game();
		}
		else
		{
			Update_PreGame();
		}
	}

	protected virtual void Update_PreGame() { }
	protected virtual void Update_Game() { }
	protected virtual void Update_PostGame() { }

	protected void ResetGameTime()
	{
		GameTime = 0;
	}

	protected void SetBasicInstructions( string text, float time )
	{
		if ( !MainLogic ) return;

		MainLogic.Text_Instruction.text = text.ToUpper(); //string.Format( text, time ).ToUpper();
		MainLogic.SetTime( time );
	}

	protected bool CheckGameEnd()
	{
		if ( GameTime >= MaxGameTime )
		{
			GameEnded = true;
			ResetGameTime();
		}
		return GameEnded;
	}

	protected bool CheckUpright( GameObject gameobject, float maxdistance = 1 )
	{
		if ( Vector3.Distance( gameobject.transform.up, transform.up ) < maxdistance )
		{
			return true;
		}
		return false;
	}

	public bool GetGameStarted()
	{
		return GameStarted;
	}

	public bool GetWaitingHands()
	{
		return NewGameWaitingHands;
	}

	public void PlayInstruction()
	{
		if ( !InstructionRecording ) return;

		InstructionLeapController.StopRecording();
		InstructionLeapController.GetLeapRecorder().Load( InstructionRecording );
		InstructionLeapController.PlayRecording();
	}

	// Return true when there are no instructions or they have played completely
	public bool GetInstructionFinished()
	{
		if (
			//( !GameStarted ) &&
			(
				( !InstructionRecording ) ||
				( InstructionLeapController.GetRecordingProgress() > 0.8f )
			)
		)
		{
			return true;
		}
		return true;//false;
	}
}