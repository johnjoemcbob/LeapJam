using UnityEngine;
using System.Collections;

public class CollisionSmokePuffScript : MonoBehaviour
{
	public GameObject ParticlePrefab;
	public float BetweenParticleFire = 2;

	private float NextParticleFire = 0;

    void OnCollisionEnter( Collision collision )
	{
		if ( NextParticleFire > Time.time ) return;

        foreach ( ContactPoint contact in collision.contacts )
		{
			GameObject particle = (GameObject) Instantiate( ParticlePrefab );
			particle.transform.position = contact.point;
			//particle.transform.LookAt( contact.normal );

			NextParticleFire = Time.time + BetweenParticleFire;
			break;
		}
	}
}