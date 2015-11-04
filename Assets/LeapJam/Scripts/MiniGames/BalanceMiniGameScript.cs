using UnityEngine;
using System.Collections;

public class BalanceMiniGameScript : BaseMiniGameScript
{
	[Header( "Balance" )]
	// The balance gameobjects, to be balanced
	public GameObject[] BalanceObjects;
	// The looping ascending tone audiosource
	public AudioSource Audio_AscendingTone;
	// The orchestra hit audiosource
	public AudioSource Audio_OrchestraHit;

	void Start()
	{
		base.Start();

		SetBasicInstructions( Instructions, 0 );
		//SetBasicInstructions( Instructions + "\nBeginning in: {0}", MaxPreGameTime );
	}

	protected override void Update_PreGame()
	{
		// Lerp the colour of the object to tell the user the game is starting
		//foreach ( GameObject balanceobject in BalanceObjects )
		//{
		//	balanceobject.GetComponent<Renderer>().material.color = new Color( 1, 1, 1, GameTime / MaxPreGameTime );
		//}
		MainLogic.SetBackgroundAlpha( 1 - ( GameTime / MaxPreGameTime ) );
		int time = (int) Mathf.Ceil( ( MaxPreGameTime - GameTime ) * 5 );
		SetBasicInstructions( Instructions, 0 );
		//SetBasicInstructions( Instructions + "\nBeginning in: {0}", time );

		// Start the game
		if ( GameTime / MaxPreGameTime >= 1 )
		{
			GameStarted = true;
			ResetGameTime();
			StartBalance();
		}
	}

	protected void StartBalance()
	{
		// Begin physics
		foreach ( GameObject balanceobject in BalanceObjects )
		{
			balanceobject.GetComponent<Rigidbody>().isKinematic = false;
		}
		// Start looping tone audio
		Audio_AscendingTone.Play();
	}

	protected override void Update_Game()
	{
		MainLogic.SetBackgroundAlpha( 0 );
		int time = (int) Mathf.Ceil( ( MaxGameTime - GameTime ) * 5 );
		SetBasicInstructions( Instructions, 1 - ( GameTime / MaxGameTime ) );
		//SetBasicInstructions( Instructions + "\n" + Instructions2 + "{0}", time );

		Update_Game_Balance();
	}

	protected void Update_Game_Balance()
	{
		// Set the pitch of the ascending tone based on time left of the round
		Audio_AscendingTone.pitch = 0.5f + ( 1 - ( 1 / MaxGameTime * ( MaxGameTime - GameTime ) ) );

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
		MainLogic.SetBackgroundAlpha( GameTime / MaxPostGameTime );
		SetBasicInstructions( WinMessage, 0 );

		Update_PostGame_Balance();

		if ( GameTime >= MaxPostGameTime )
		{
			MainLogic.EndCurrentGame();
		}
	}

	protected void Update_PostGame_Balance()
	{
		// Ensure the looping ascending tone has been stopped
		if ( Audio_AscendingTone.isPlaying )
		{
			Audio_AscendingTone.Stop();
			Audio_OrchestraHit.Play();
		}
	}
}