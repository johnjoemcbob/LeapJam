using UnityEngine;
using System.Collections;

public class BuzzLineRingCollisionScript : MonoBehaviour
{
	// Reference to the logic script for this minigame, to trigger failure/success
	public BuzzLineMiniGameScript MinigameLogic;
	// The maximum number of collisions during this minigame
	public int MaxCollisions = 3;

	// The current number of collisions of the ring
	private int Collisions = 0;

	void OnTriggerEnter( Collider other )
	{
		if ( other.transform.gameObject.layer == LayerMask.NameToLayer( "BuzzLine_Wire" ) )
		{
			GetComponent<AudioSource>().Play();
			Collisions++;
		}
		else if ( other.transform.gameObject.layer == LayerMask.NameToLayer( "BuzzLine_Goal" ) )
		{
			MinigameLogic.Win();
		}
		else if ( other.transform.gameObject.layer == LayerMask.NameToLayer( "BuzzLine_Boundary" ) )
		{
			MinigameLogic.Lose();
		}

		if ( Collisions > MaxCollisions )
		{
			MinigameLogic.Lose();
		}
	}
}