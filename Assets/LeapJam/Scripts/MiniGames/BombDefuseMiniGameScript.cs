using UnityEngine;
using System.Collections;
using Leap;

public class BombDefuseMiniGameScript : BaseMiniGameScript
{
	[Header( "Bomb Defuse" )]
	// The wire positions
	public Transform[] Wires;

	// The random wire to cut
	private int WireToCut = 0;
	// The name of each wire
	private string[] WireName = new string[] { "Red", "Yellow", "Blue" };
	// The wire each hand is in proximity to
	private int[] ProximityWire = new int[] { 0, 0 };
	// The gesture progression of each hand on the current wire
	private int[] GestureProgression = new int[] { -1, -1 };

	void Start()
	{
		base.Start();

		// Choose a random wire
		WireToCut = UnityEngine.Random.Range( 0, WireName.Length );

		SetBasicInstructions( string.Format( Instructions, WireName[WireToCut] ), 0 );
		//SetBasicInstructions( string.Format( Instructions, WireName[WireToCut] ) + "\nBeginning in: {0}", MaxPreGameTime );
	}

	protected override void Update_PreGame()
	{
		MainLogic.SetBackgroundAlpha( 1 - ( GameTime / MaxPreGameTime ) );
		SetBasicInstructions( string.Format( Instructions, WireName[WireToCut] ), 0 );
		//SetBasicInstructions( string.Format( Instructions, WireName[WireToCut] ) + "\nBeginning in: {0}", MaxPreGameTime );

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
		SetBasicInstructions( string.Format( Instructions, WireName[WireToCut] ), 1 - ( GameTime / MaxGameTime ) );
		//SetBasicInstructions( string.Format( Instructions, WireName[WireToCut] ) + "\n" + Instructions2 + "{0}", time );

		// Check the normal game end conditions (timer)
		if ( CheckGameEnd() )
		{
			WinMessage = "No!\nYou didn't defuse the bomb in time!";
			GameWon = false;
			MainLogic.RunWinLose( GameWon );
			return;
		}

		// Check win/lose conditions (scissor gesture on any of the wires)
		HandModel[] hands = LeapController.GetAllPhysicsHands();
		int wirecut = -1;
			int currenthand = 0;
			foreach ( HandModel hand in hands )
			{
				ProximityWire[currenthand] = -1;

				// Facing toward the bomb
				if ( Vector3.Distance( hand.GetPalmDirection(), Vector3.forward ) < 0.5f )
				{
					float maxdistance = 0.5f;
					int currentwire = 0;
					foreach ( Transform wire in Wires )
					{
						// Near this wire target position
						//wire.position = new Vector3( wire.position.x, hand.GetPalmPosition().y, wire.position.z );
						//float distance = Vector3.Distance( hand.GetPalmPosition(), wire.position );
						//if ( distance < maxdistance )
						float distancex = Mathf.Abs( hand.GetPalmPosition().x - wire.position.x );
						float distancez = Mathf.Abs( hand.GetPalmPosition().z - wire.position.z );
						if (
							( distancex < maxdistance ) &&
							( distancez < 1 )
						)
						{
							ProximityWire[currenthand] = currentwire;
							maxdistance = distancex;
						}
						currentwire++;
					}
				}

				// Check for scissors gesture
				float spread = hand.fingers[1].GetFingerJointSpreadMecanim();
				//if ( ProximityWire[currenthand] != -1 )
				{
					// Gesture hasn't started yet, fingers must be openned
					if ( GestureProgression[currenthand] == -1 )
					{
						if ( spread > 1 )
						{
							GestureProgression[currenthand]++;
						}
					}
					// Gesture fingers have been openned, must now be closed
					if ( GestureProgression[currenthand] == 0 )
					{
						if ( spread < -1 )
						{
							GestureProgression[currenthand]++;
							// Play sound
							GetComponent<AudioSource>().Play();
							// Check if it cut a wire
							if ( ProximityWire[currenthand] != -1 )
							{
								// Flag the wire as cut now
								wirecut = ProximityWire[currenthand];
							}
							GestureProgression[currenthand] = -1;
						}
					}
				}
				//else // Reset gesture progression if the hand moves away
				//{
				//	GestureProgression[currenthand] = -1;
				//}

				currenthand++;
			}
		// Cut the right wire
		if ( wirecut == WireToCut )
		{
			WinMessage = "Yes!";
			GameEnded = true;
			GameWon = true;
			ResetGameTime();
			MainLogic.RunWinLose( GameWon );
			return;
		}
		// Cut the wrong wire
		else if ( wirecut != -1 )
		{
			WinMessage = "No!\nYou cut the wrong wire!";
			GameEnded = true;
			GameWon = false;
			ResetGameTime();
			MainLogic.RunWinLose( GameWon );
			return;
		}
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