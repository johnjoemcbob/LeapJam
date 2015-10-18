using UnityEngine;
using System.Collections;
using Leap;

public class ScreamMiniGameScript : BaseMiniGameScript
{
	[Header( "Scream" )]
	// The left hand requried position
	public Transform Hand_Left;
	// The right hand requried position
	public Transform Hand_Right;

	void Start()
	{
		SetBasicInstructions( Instructions, 0 );
		//SetBasicInstructions( Instructions + "\nBeginning in: {0}", MaxPreGameTime );
	}

	protected override void Update_PreGame()
	{
		MainLogic.SetBackgroundAlpha( 1 - ( GameTime / MaxPreGameTime ) );
		SetBasicInstructions( Instructions, 0 );
		//SetBasicInstructions( Instructions + "\nBeginning in: {0}", MaxPreGameTime );

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
		SetBasicInstructions( Instructions, 0 );
		//SetBasicInstructions( Instructions + "\n" + Instructions2 + "{0}", time );

		// Check the normal game end conditions (timer)
		if ( CheckGameEnd() )
		{
			WinMessage = "No!\nYou didn't scream in time!";
			GameWon = false;
			MainLogic.RunWinLose( GameWon );
			return;
		}

		// Check win conditions (both hands are pointing to the sky and positioned at either side of the face)
		HandModel[] hands = LeapController.GetAllPhysicsHands();
		bool[] handinposition = new bool[] { false, false };
		float distance;
		float distancemultiplier = 0;
		if ( hands.Length > 1 )
		{
			foreach ( HandModel hand in hands )
			{
				// Facing upward
				if ( Vector3.Distance( hand.GetPalmDirection(), Vector3.up ) < 0.5f )
				{
					// Near left hand target position
					distance = Vector3.Distance( hand.GetPalmPosition(), Hand_Left.position );
					distancemultiplier += distance;
					if ( distance < 1 )
					{
						handinposition[0] = true;
					}
					// Near right hand target position
					distance = Vector3.Distance( hand.GetPalmPosition(), Hand_Right.position );
					distancemultiplier += distance;
					if ( distance < 1 )
					{
						handinposition[1] = true;
					}
				}
			}
		}
		if ( handinposition[0] && handinposition[1] )
		{
			WinMessage = "Yes!";
			GameEnded = true;
			GameWon = true;
			ResetGameTime();
			MainLogic.RunWinLose( GameWon );
			return;
		}

		// Change scream audio pitch and volume
		//AudioSource source = GetComponent<AudioSource>();
		//distancemultiplier /= 4;
		//if ( distancemultiplier  == 0 ) distancemultiplier = 1;
		//distancemultiplier = 1 - Mathf.Clamp( distancemultiplier, 0, 0.8f );
		//source.volume = distancemultiplier;
		//source.pitch = 1 + distancemultiplier;
	}

	protected override void Update_PostGame()
	{
		MainLogic.SetBackgroundAlpha( GameTime / MaxPostGameTime );
		SetBasicInstructions( WinMessage, 0 );
		//SetBasicInstructions( WinMessage, 0 );

		if ( GameTime >= MaxPostGameTime )
		{
			MainLogic.EndCurrentGame();
		}
	}
}