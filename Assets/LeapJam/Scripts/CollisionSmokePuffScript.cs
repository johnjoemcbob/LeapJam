﻿using UnityEngine;
using System.Collections;

public class CollisionSmokePuffScript : MonoBehaviour
{
	public GameObject ParticlePrefab;
	public GameObject ParticleHolderObject;
	public float TimeBetweenParticleFire = 0.5f;

	private float NextParticleFire = 0;

    void OnCollisionEnter( Collision collision )
	{
		// Not moving fast enough
		if ( NextParticleFire > Time.time ) return;
		if ( collision.relativeVelocity.y < 1.5f ) return;

        foreach ( ContactPoint contact in collision.contacts )
		{
			GameObject particle = (GameObject) Instantiate( ParticlePrefab );
			particle.transform.position = contact.point;
			particle.transform.SetParent( ParticleHolderObject.transform );

			NextParticleFire = Time.time + TimeBetweenParticleFire;
			break;
		}
	}
}