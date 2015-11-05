﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SkeeballMiniGameScript : BaseMiniGameScript
{
	[Header( "Skeeball" )]
	// The ball prefab to spawn
	public GameObject SkeeballPrefab;
	// The UI for this minigame, display of the number of remaining balls
	public Text Text_Ball;
	// The maximum balls for this game
	public int MaxBalls = 1;
	// The multiplier applied to each score segment
	public int ScoreMultiplier = 1;

	// The number of balls which have been thrown during this instance of the game
	private int BallsThrown = 0;
	// After throwing, wait for the velocity of the hand to be small again before allowing another ball spawn
	private bool[] CanThrow = { true, true };

	void Start()
	{
		base.Start();

		SetBasicInstructions( Instructions, 0 );
		UpdateBallUI();
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

		// Check for hand velocity to initiate throwing
		HandModel[] hands = LeapController.GetAllPhysicsHands();
		int currenthand = 0;
		foreach ( HandModel hand in hands )
		{
			if ( BallsThrown < MaxBalls )
			{
				Vector3 velocity = new Vector3( hand.GetLeapHand().PalmVelocity.x, hand.GetLeapHand().PalmVelocity.y, -hand.GetLeapHand().PalmVelocity.z );
				if ( velocity.z > 100 )
				{
					if ( CanThrow[currenthand] )
					{
						GameObject thrownball = (GameObject) Instantiate( SkeeballPrefab );
						thrownball.transform.SetParent( transform );
						thrownball.transform.position = hand.GetPalmPosition();
						thrownball.GetComponent<Rigidbody>().AddForce( velocity.normalized * 7.5f );
						BallsThrown++;
						UpdateBallUI();
						CanThrow[currenthand] = false;
					}
				}
				else if ( velocity.z < 10 )
				{
					CanThrow[currenthand] = true;
				}
				currenthand++;
			}
		}

		// Check the unique game end conditions
		
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

	private void UpdateBallUI()
	{
		Text_Ball.text = string.Format( "BALLS REMAINING: {0}", MaxBalls - BallsThrown );
	}

	public void AddScore( int score )
	{
		Score += score * ScoreMultiplier;
	}
}