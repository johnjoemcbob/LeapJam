using UnityEngine;
using System.Collections;

public class SpinPlateMiniGameScript : BalanceMiniGameScript
{
	[Header( "Spin Plate" )]
	// The spin gameobjects, to be spun by velocity proximity of hands
	public GameObject[] SpinObjects;

	void Start()
	{
		base.Start();

		SetBasicInstructions( Instructions, 0 );

		// Give the spinning plates the initial rotational velocity
		foreach ( GameObject spin in SpinObjects )
		{
			spin.GetComponent<Rigidbody>().AddTorque( new Vector3( 0, 1000000, 0 ) );
		}
	}

	protected override void Update_PreGame()
	{
		MainLogic.SetBackgroundAlpha( 1 - ( GameTime / MaxPreGameTime ) );
		int time = (int) Mathf.Ceil( ( MaxPreGameTime - GameTime ) * 5 );
		SetBasicInstructions( Instructions, 0 );

		// Start the game
		if ( GameTime / MaxPreGameTime >= 1 )
		{
			GameStarted = true;
			ResetGameTime();
			StartBalance();
		}

		// Give the spinning plates the initial rotational velocity
		foreach ( GameObject spin in SpinObjects )
		{
			spin.GetComponent<Rigidbody>().AddTorque( new Vector3( 0, 1000000, 0 ) );
		}
	}

	protected override void Update_Game()
	{
		MainLogic.SetBackgroundAlpha( 0 );
		int time = (int) Mathf.Ceil( ( MaxGameTime - GameTime ) * 5 );
		SetBasicInstructions( Instructions, 1 - ( GameTime / MaxGameTime ) );

		Update_Game_Balance();

		// Check hands for closeness to plates and velocity
		HandModel[] hands = LeapController.GetAllPhysicsHands();
		foreach ( HandModel hand in hands )
		{
			// Spinning gesture
			if ( hand.GetLeapHand().PalmVelocity.z > 300 )
			{
				// Increase angular velocity of each spinning object
				foreach ( GameObject spin in SpinObjects )
				{
					spin.GetComponent<Rigidbody>().AddTorque( new Vector3( 0, 100, 0 ) );
				}
			}
		}

		// Check the unique game end conditions (spinning has to be fast)
		foreach ( GameObject spin in SpinObjects )
		{
			float speed = spin.GetComponent<Rigidbody>().angularVelocity.y;
			if ( speed < 3 )
			{
				// Format lose message
				WinMessage = "No!\nThe plate stopped spinning!";
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
}