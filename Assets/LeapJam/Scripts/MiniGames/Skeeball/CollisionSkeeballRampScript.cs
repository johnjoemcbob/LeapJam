using UnityEngine;
using System.Collections;

public class CollisionSkeeballRampScript : MonoBehaviour
{
	void OnTriggerEnter( Collider other )
	{
		// If the skeeball is rolling towards the ring targets, boost it
		if (
			( other.gameObject.layer == LayerMask.NameToLayer( "Ball_Skeeball" ) ) &&
			( other.GetComponent<Rigidbody>().velocity.z > 0 )
		)
		{
			other.GetComponent<Rigidbody>().AddForce( new Vector3( 0, 10, 4 ) );
		}
	}
}