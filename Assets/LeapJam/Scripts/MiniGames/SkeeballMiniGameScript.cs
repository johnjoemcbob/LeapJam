using UnityEngine;
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
	// The average of movement while the hand is moving quickly
	private Vector3[] AverageDirection = { new Vector3(), new Vector3() };
	private int[] AverageDirectionCount = { 0, 0 };
	// The ball objects which have been thrown into the scene, used to detect if the game is over
	private ArrayList Balls = new ArrayList();

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
				if ( velocity.z > 200 )
				{
					if ( CanThrow[currenthand] )
					{
						// Calculate direction to fire in
						velocity.Normalize();
						if ( AverageDirectionCount[currenthand] == 0 )
						{
							AverageDirection[currenthand] = velocity.normalized;
							AverageDirectionCount[currenthand]++;
						}
						AverageDirection[currenthand] /= AverageDirectionCount[currenthand];
						Vector3 direction = new Vector3(
							Mathf.Clamp( AverageDirection[currenthand].x, -0.5f, 0.5f ),
							Mathf.Clamp( AverageDirection[currenthand].y, 0.2f, 0.9f ),
							Mathf.Clamp( AverageDirection[currenthand].z, 0.2f, 0.9f ) * 2
						);
						AverageDirection[currenthand] = new Vector3( 0, 0, 0 );
						AverageDirectionCount[currenthand] = 0;

						// Create and launch ball
						GameObject thrownball = (GameObject) Instantiate( SkeeballPrefab );
							thrownball.transform.SetParent( transform );
							thrownball.transform.position = hand.GetPalmPosition();
							thrownball.GetComponent<Rigidbody>().AddForce( direction * 12.5f );
						Balls.Add( thrownball );

						BallsThrown++;
						UpdateBallUI();
						CanThrow[currenthand] = false;
					}
				}
				else if ( velocity.z > 100 )
				{
					AverageDirection[currenthand] += velocity.normalized;
					AverageDirectionCount[currenthand]++;
				}
				else if ( velocity.z < 10 )
				{
					CanThrow[currenthand] = true;
					AverageDirection[currenthand] = new Vector3( 0, 0, 0 );
				}
				currenthand++;
			}
		}

		// Check the unique game end conditions (When all balls have been thrown and reached a target
		if ( BallsThrown == MaxBalls )
		{
			bool end = true;
				foreach ( GameObject ball in Balls )
				{
					// If it still exists, is still moving & is moving in the general direction of the goals
					if ( ball )
					{
						Rigidbody body = ball.GetComponent<Rigidbody>();
						if (
							( body.velocity.magnitude > 0.05f ) && // Is moving
							( ! ( ( body.transform.position.z < 7 ) && ( body.velocity.z < 0 ) ) ) && // Not moving backwards down the lane
							( body.transform.position.y > 0 ) // Above board
						)
						{
							end = false;
							break;
						}
					}
				}
			if ( end )
			{
				WinMessage = "Yes!";
				GameWon = true;
				GameEnded = true;
				MainLogic.RunWinLose( GameWon );
				return;
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

	private void UpdateBallUI()
	{
		Text_Ball.text = string.Format( "BALLS REMAINING: {0}", MaxBalls - BallsThrown );
	}

	public void AddScore( int score )
	{
		Score += score * ScoreMultiplier;
	}
}