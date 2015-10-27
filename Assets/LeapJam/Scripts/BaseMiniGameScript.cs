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

	// The time for which the game lasts
	public float MaxPreGameTime = 1;
	public float MaxGameTime = 5;
	public float MaxPostGameTime = 1;
	// The instructions to display
	public string Instructions;
	public string Instructions2;
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

	void Update()
	{
		// Increment the amount of time this game has been active for
		// Actual game end logic is checked within individual minigames,
		// to allow for some to work around the time limit if need be
		GameTime += Time.deltaTime;
		// In multiplayer the start timer needs to pause to allow for player repositioning
		if ( ( !GameStarted ) && ( MainLogic.GetMaxPlayers() > 1 ) && NewGameWaitingHands )
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
}