using UnityEngine;
using System.Collections;

public class TargetBulletScript : MonoBehaviour
{
	void OnTriggerEnter( Collider other )
	{
		if ( other.gameObject.layer == LayerMask.NameToLayer( "Target" ) )
		{
			// Check for it being an actual target (obstacles in the target practice game are layered the same)
			TargetExplodeScript target = other.GetComponentInChildren<TargetExplodeScript>();
			if ( target )
			{
				// Blow up target
				target.Explode();
				// Check for win
				other.transform.parent.parent.GetComponentInChildren<TargetPracticeMiniGameScript>().CheckTargetWin( other.gameObject );
			}

			// Remove this bullet
			Destroy( gameObject );
		}
	}
}