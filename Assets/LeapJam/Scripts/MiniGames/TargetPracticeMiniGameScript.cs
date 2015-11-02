using UnityEngine;
using System.Collections;
using Leap;

public class TargetPracticeMiniGameScript : BaseMiniGameScript
{
	[Header( "Target Practice" )]
	// The target objects
	public GameObject[] Targets;
	// The bullet prefab to fire
	public GameObject BulletPrefab;
	// The firing particle effect prefab
	public GameObject GunParticlePrefab;
	// The speed for the bullet to travel at
	public float BulletSpeed = 1000;
	// The time between gesture recog and first bullet fire
	public float BulletFirstTime = 0.5f;
	// The time between bullet fires
	public float BulletReloadTime = 0.5f;
	// Audio clip references for firing and reloading
	[Header( "Audio" )]
	public AudioClip Audio_Fire;
	public AudioClip Audio_Reload;

	// The gesture progression of each hand as a gun (hand recoil)
	private int[] GestureProgression = new int[] { -1, -1 };
	// The gesture progression of each hand as a gun (thumb hammer)
	private int[] GestureProgressionThumb = new int[] { -1, -1 };
	// Flag for each target being shot or not
	private bool[] TargetHit;
	// Time until the next bullet can be fired
	private float[] NextFireTime = new float[] { 0, 0 };
	// The last tracked hand normal of each hand
	private Vector[] HandNormal = new Vector[2];
	// The time at which to play the reload sound effect (0 is don't play)
	private float[] AudioReloadTime = new float[] { 0, 0 };

	void Start()
	{
		SetBasicInstructions( Instructions, 0 );
		//SetBasicInstructions( Instructions + "\nBeginning in: {0}", MaxPreGameTime );

		// Initialize the TargetHit array to be the size of the Targets objects information array
		TargetHit = new bool[Targets.Length];
		for ( int target = 0; target < Targets.Length; target++ )
		{
			TargetHit[target] = false;
		}
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
		SetBasicInstructions( Instructions, 1 - ( GameTime / MaxGameTime ) );
		//SetBasicInstructions( Instructions + "\n" + Instructions2 + "{0}", time );

		// Check the normal game end conditions (timer)
		if ( CheckGameEnd() )
		{
			WinMessage = "No!\nYou didn't hit the targets in time!";
			GameWon = false;
			MainLogic.RunWinLose( GameWon );
			return;
		}

		// Play the reload sound effect when flagged
		for ( int hand = 0; hand < 2; hand++ )
		{
			if ( ( AudioReloadTime[hand] != 0 ) && ( AudioReloadTime[hand] <= Time.time ) )
			{
				// Play reload sound effect
				GetComponent<AudioSource>().PlayOneShot( Audio_Reload );
				AudioReloadTime[hand] = 0;
			}
		}

		// Check for targets being fired at
		HandModel[] hands = LeapController.GetAllPhysicsHands();
		int currenthand = 0;
		foreach ( HandModel hand in hands )
		{
			// Check that the hand is shaped like a gun (GestureProgression=0)
			// (All fingers curled except index, thumb/middle don't matter)
			if (
				( hand.fingers[1].GetLeapFinger().IsExtended ) &&
				//( !hand.fingers[2].GetLeapFinger().IsExtended ) &&
				( !hand.fingers[3].GetLeapFinger().IsExtended ) &&
				( !hand.fingers[4].GetLeapFinger().IsExtended )
			)
			{
				if ( GestureProgression[currenthand] == -1 )
				{
					GestureProgression[currenthand] = 0;
					NextFireTime[currenthand] = Time.time + BulletFirstTime;
				}
			}
			else
			{
				GestureProgression[currenthand] = -1;
			}
			// Check that the thumb is extended like a hammer
			Vector3 thumbtip = hand.fingers[0].bones[2].position;
			Vector3 indextip = hand.fingers[1].bones[3].position;
			float thumbdistance = Vector3.Distance( thumbtip, indextip );
			if ( thumbdistance > 1 )
			{
				// The hand must also be gun shaped to proceed
				if ( GestureProgression[currenthand] == 0 )
				{
					GestureProgressionThumb[currenthand] = 0;
				}
			}
			else if ( GestureProgressionThumb[currenthand] == 0 )
			{
				if ( NextFireTime[currenthand] <= Time.time )
				{
					GestureProgressionThumb[currenthand] = 1;
				}
			}
			else
			{
				GestureProgressionThumb[currenthand] = -1;
			}

			// Check for gun firing (hand recoil gesture) (GestureProgression=1)
			if ( GestureProgression[currenthand] == 0 )
			{
				if (
					( NextFireTime[currenthand] <= Time.time ) &&
					(
						( hand.GetLeapHand().PalmVelocity.z > 150 ) ||
						( HandNormal[currenthand].IsValid() && ( HandNormal[currenthand].DistanceTo( hand.GetLeapHand().PalmNormal ) > 0.1f ) )
					)
				)
				{
					//GestureProgression[currenthand] = 1;
				}
			}

			// If the gun fired then fire the bullet prefab
			if ( ( GestureProgression[currenthand] == 1 ) || ( GestureProgressionThumb[currenthand] == 1 ) )
			{
				// Create the bullet prefab and fire it
				Transform fingertip = hand.fingers[1].bones[hand.fingers[1].bones.Length - 2];
				GameObject bullet = (GameObject) Instantiate(
					BulletPrefab,
					hand.fingers[1].GetTipPosition(),
					Quaternion.Euler( fingertip.rotation.eulerAngles + new Vector3( 90, 0, 0 ) )
				);
				bullet.transform.parent = transform;
				bullet.GetComponent<Rigidbody>().AddForce( fingertip.forward * BulletSpeed );
				// Play sound effect
				GetComponent<AudioSource>().PlayOneShot( Audio_Fire );
				// Play the particle effect
				GameObject particle = (GameObject) Instantiate(
					GunParticlePrefab,
					hand.fingers[1].GetTipPosition(),
					Quaternion.Euler( fingertip.rotation.eulerAngles )
				);
				particle.transform.parent = transform;

				// Flag as not firing again
				GestureProgression[currenthand] = -1;
				GestureProgressionThumb[currenthand] = -1;

				// Timers for next firing
				NextFireTime[currenthand] = Time.time + BulletReloadTime;
				AudioReloadTime[currenthand] = Time.time + 0.5f;
			}

			// Update last palm normal for this hand
			HandNormal[currenthand] = hand.GetLeapHand().PalmNormal;

			currenthand++;
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

	// Called each time a target is hit to see if it was the only remaining target
	public void CheckTargetWin( GameObject targetdestroyed )
	{
		// Flag this target as destroyed
		int targetid = 0;
			foreach( GameObject target in Targets )
			{
				if ( target == targetdestroyed )
				{
					TargetHit[targetid] = true;
					targetdestroyed.GetComponent<TargetMoveScript>().SetAnimationState( 2 );
					break;
				}
				targetid++;
			}

		// Check if all of the targets are now destroyed
		bool win = true;
			foreach( bool hit in TargetHit )
			{
				if ( !hit )
				{
					win = false;
				}
			}
		if ( win )
		{
			WinMessage = "Yes!";
			GameEnded = true;
			GameWon = true;
			ResetGameTime();
			MainLogic.RunWinLose( GameWon );
			return;
		}
	}
}