using UnityEngine;
using System.Collections;

public class JugglingMiniGameScript : BaseMiniGameScript
{
	[Header( "Juggle" )]
	// The juggle gameobjects, to be juggled
	public GameObject[] JuggleObjects;
	// The lanes to direct the juggle objects towards
	public Transform[] JuggleLanes;

	void Start()
	{
		base.Start();

		SetBasicInstructions( Instructions, 0 );
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
	}

	protected void StartBalance()
	{
		// Begin physics
		foreach ( GameObject balanceobject in JuggleObjects )
		{
			balanceobject.GetComponent<Rigidbody>().isKinematic = false;
		}
	}

	protected override void Update_Game()
	{
		MainLogic.SetBackgroundAlpha( 0 );
		int time = (int) Mathf.Ceil( ( MaxGameTime - GameTime ) * 5 );
		SetBasicInstructions( Instructions, 1 - ( GameTime / MaxGameTime ) );
		//SetBasicInstructions( Instructions + "\n" + Instructions2 + "{0}", time );

		// Check the normal game end conditions (timer)
		if ( CheckGameEnd() )
		{
			WinMessage = "Yes!";
			GameWon = true;
			MainLogic.RunWinLose( GameWon );
			return;
		}

		// Check the unique game end conditions
		foreach ( GameObject balanceobject in JuggleObjects )
		{
			bool notonscreen = ( balanceobject.transform.position.y < -2 );
			if ( notonscreen )
			{
				// Format lose message
				WinMessage = "No!\nYou dropped the object!";
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

		if ( GameTime >= MaxPostGameTime )
		{
			MainLogic.EndCurrentGame();
		}
	}

	public Transform[] GetLanes()
	{
		return JuggleLanes;
	}
}