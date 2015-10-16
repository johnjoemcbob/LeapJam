using UnityEngine;
using System.Collections;

public class TargetExplodeScript : MonoBehaviour
{
	// Reference to the target to disable
	public GameObject Target;
	// Reference to the explosion objects to enable
	public GameObject Explosion;

	// The time at which to play the breaking sound effect (if 0 don't play)
	private float SoundTime = 0;

	void Update()
	{
		if ( ( SoundTime != 0 ) && ( SoundTime <= Time.time ) )
		{
			// Play sound effect
			AudioSource source = Explosion.GetComponent<AudioSource>();
			source.pitch = Random.Range( 0.8f, 1.0f );
			source.Play();
			Target.SetActive( false );
			SoundTime = 0;
		}
	}

	public void Explode()
	{
		Explosion.transform.localPosition = Target.transform.localPosition;
		Explosion.SetActive( true );
		SoundTime = Time.time + 0.1f;
	}
}