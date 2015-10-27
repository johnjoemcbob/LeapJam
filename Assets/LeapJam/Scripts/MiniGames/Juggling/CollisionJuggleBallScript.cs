using UnityEngine;
using System.Collections;

public class CollisionJuggleBallScript : MonoBehaviour
{
	// Reference to the logic script of the minigame containing this juggle object
	public JugglingMiniGameScript JuggleLogic;

    void OnCollisionEnter( Collision collision )
	{
		string name = collision.transform.gameObject.name;
		if (
			name.Contains( "bone" ) ||
			name.Contains( "palm" ) ||
			name.Contains( "forearm" )
		)
		{
			Vector3 velocity = new Vector3();

			// Find closest lane, direct the object towards any others
			Transform[] lanes = JuggleLogic.GetLanes();
			float mindistance = -1;
			int closestlane = -1;
			for ( int lane = 0; lane < lanes.Length; lane++ )
			{
				Transform lanetransform = lanes[lane];
				float distance = Mathf.Abs( lanetransform.position.x - transform.position.x );
				if ( ( mindistance == -1 ) || ( distance < mindistance ) )
				{
					closestlane = lane;
					mindistance = distance;
				}
			}
			// Choose a random number from 0 to the length of the array - 1, to skip the current lane
			int randomlane = Random.Range( 0, lanes.Length - 1 );
			// If the random lane is the closest or is after the closest then add 1 to reach all other array positions
			if ( randomlane >= closestlane )
			{
				randomlane++;
			}
			velocity += new Vector3( lanes[randomlane].position.x * 0.6f, Random.Range( 2.5f, 4.5f ), 0 );

			// Fire upwards
			velocity += new Vector3( 0, 10, 0 );

			// Set velocity on the object
			GetComponent<Rigidbody>().velocity = velocity;
			GetComponent<Rigidbody>().angularVelocity = new Vector3( Random.Range( 0, 360 ), Random.Range( 0, 360 ), Random.Range( 0, 360 ) );
		}

		// Play collision audio
		GetComponent<AudioSource>().pitch = Random.Range( 0.8f, 1.2f );
		GetComponent<AudioSource>().Play();
	}
}