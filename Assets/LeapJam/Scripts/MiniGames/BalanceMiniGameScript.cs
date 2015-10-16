using UnityEngine;
using System.Collections;

public class BalanceMiniGameScript : BaseMiniGameScript
{
	// The balance gameobjects, to be balanced
	public GameObject[] BalanceObjects;

	void Start()
	{
		SetBasicInstructions( Instructions + "\nBeginning in: {0}", MaxPreGameTime );
	}

	protected override void Update_PreGame()
	{
		// Lerp the colour of the object to tell the user the game is starting
		foreach ( GameObject balanceobject in BalanceObjects )
		{
			balanceobject.GetComponent<Renderer>().material.color = new Color( 1, 1, 1, GameTime / MaxPreGameTime );
		}
		int time = (int) Mathf.Ceil( ( MaxPreGameTime - GameTime ) * 5 );
		SetBasicInstructions( Instructions + "\nBeginning in: {0}", time );

		// Start the game
		if ( GameTime / MaxPreGameTime >= 1 )
		{
			GameStarted = true;
			ResetGameTime();
			foreach ( GameObject balanceobject in BalanceObjects )
			{
				balanceobject.GetComponent<Rigidbody>().isKinematic = false;
			}
		}
	}

	protected override void Update_Game()
	{
		int time = (int) Mathf.Ceil( ( MaxGameTime - GameTime ) * 5 );
		SetBasicInstructions( Instructions + "\n" + Instructions2 + "{0}", time );

		// Check the normal game end conditions (timer)
		if ( CheckGameEnd() )
		{
			WinMessage = "Yes!";
			GameWon = true;
			MainLogic.RunWinLose( GameWon );
			return;
		}

		// Check the unique game end conditions
		foreach ( GameObject balanceobject in BalanceObjects )
		{
			bool notupright = ( !CheckUpright( balanceobject, 1 ) );
			bool notonscreen = ( balanceobject.transform.position.y < -2 );
			if (
				notupright || // (object fell over)
				notonscreen // (object left screen)
			)
			{
				// Format lose message
				WinMessage = "No!\n";
				if ( notupright )
				{
					WinMessage += "The object fell over!";
				}
				else
				{
					WinMessage += "You dropped the object!";
				}
				// Flag the game as over
				GameEnded = true;
				GameWon = false;
				ResetGameTime();
				MainLogic.RunWinLose( GameWon );
			}
		}
	}

	protected override void Update_PostGame()
	{
		SetBasicInstructions( WinMessage, 0 );

		if ( GameTime >= MaxPostGameTime )
		{
			MainLogic.EndCurrentGame();
		}
	}
}