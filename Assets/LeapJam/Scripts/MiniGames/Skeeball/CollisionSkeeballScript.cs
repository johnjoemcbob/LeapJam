using UnityEngine;
using System.Collections;

public class CollisionSkeeballScript : MonoBehaviour
{
	void OnCollisionEnter( Collision collision )
	{
	//	string name = collision.transform.gameObject.name;
	//	if (
	//		name.Contains( "bone" ) ||
	//		name.Contains( "palm" ) ||
	//		name.Contains( "forearm" )
	//	)
	//	{
	//		GetComponent<Rigidbody>().useGravity = true;
	//		GetComponent<Rigidbody>().velocity = collision.relativeVelocity;
	//	}

		if ( collision.relativeVelocity.magnitude > 1 )
		{
			// Play collision audio
			GetComponent<AudioSource>().pitch = Random.Range( 0.8f, 1.2f );
			GetComponent<AudioSource>().Play();
		}
	}
}