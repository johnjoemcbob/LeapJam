using UnityEngine;
using System.Collections;

public class CollisionSkeeballGoalScript : MonoBehaviour
{
	// Reference to the logic script of the Skeeball minigame
	public SkeeballMiniGameScript SkeeballLogic;
	// The particle effect prefab to play when scored
	public GameObject ParticlePrefab;
	// The score to award when this is triggered (multiplied within SkeeballLogic)
	public int Score;

	void OnTriggerEnter( Collider other )
	{
		// Play the particle effect
		GameObject particle = (GameObject) Instantiate( ParticlePrefab );
		particle.transform.SetParent( transform );
		particle.transform.position = other.transform.position;
		particle.transform.localScale = new Vector3( 2, 2, 2 );

		// Play the sound effect
		GetComponent<AudioSource>().Play();

		// Add score & delete ball
		SkeeballLogic.AddScore( Score );
		Destroy( other.gameObject );
	}
}