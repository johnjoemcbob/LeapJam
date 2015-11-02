using UnityEngine;
using System.Collections;
using Leap;

public class BuzzLineMiniGameScript : BaseMiniGameScript
{
	[Header( "Buzz Line" )]
	// The ring object to be moved through the obstacles, a child of the minigame prefab
	public GameObject Ring;

	private GameObject RingParent;
	// The offset position inherited when the hand picks up the ring
	private Vector3 OffsetPosition;
	// The offset angle inherited when the hand picks up the ring
	private float OffsetAngle;

	void Start()
	{
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
		}
	}

	protected override void Update_Game()
	{
		MainLogic.SetBackgroundAlpha( 0 );
		int time = (int) Mathf.Ceil( ( MaxGameTime - GameTime ) * 5 );
		SetBasicInstructions( Instructions, 1 - ( GameTime / MaxGameTime ) );

		// Check the normal game end conditions (timer)
		if ( CheckGameEnd() )
		{
			WinMessage = "No!\nYou ran out of time!";
			GameWon = false;
			MainLogic.RunWinLose( GameWon );
			return;
		}

		// Ensure that the ring is parented to the index finger, if the finger is close enough to the ring
		HandModel[] hands = LeapController.GetAllPhysicsHands();
		if ( ( !RingParent ) && ( hands.Length > 0 ) )
		{
			foreach ( HandModel hand in hands )
			{
				Transform indextip = hand.fingers[1].bones[3];
				if ( Vector3.Distance( indextip.position, Ring.transform.position ) < 1 )
				{
					RingParent = indextip.gameObject;
					OffsetPosition = Ring.transform.position - indextip.position;
					OffsetAngle = Ring.transform.localEulerAngles.z;
				}
			}
		}

		// Don't parent to transform or it will be removed when the hands are removed from the scene
		if ( RingParent )
		{
			Ring.transform.position = new Vector3(
				RingParent.transform.position.x + OffsetPosition.x,
				RingParent.transform.position.y + OffsetPosition.y,
				Ring.transform.position.z
			);
			Ring.transform.localEulerAngles = new Vector3( 0, 0, RingParent.transform.eulerAngles.z - OffsetAngle );
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

	// Triggered by the BuzzLineRingCollisionScript contained within the ring gameobject
	// Flags the minigame as lost, the ring touched a wire
	public void Lose()
	{
		WinMessage = "No!\nThe ring touched a wire!";
		GameEnded = true;
		GameWon = false;
		ResetGameTime();
		MainLogic.RunWinLose( GameWon );
	}

	// Triggered by the BuzzLineRingCollisionScript contained within the ring gameobject
	// Flags the minigame as won, the ring gets to the end of the wire
	public void Win()
	{
		WinMessage = "Yes!\nYou reached the end!";
		GameEnded = true;
		GameWon = true;
		ResetGameTime();
		MainLogic.RunWinLose( GameWon );
	}
}