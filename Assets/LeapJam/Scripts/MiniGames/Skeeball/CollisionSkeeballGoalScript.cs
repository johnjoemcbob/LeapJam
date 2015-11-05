using UnityEngine;
using System.Collections;

public class CollisionSkeeballGoalScript : MonoBehaviour
{
	// Reference to the logic script of the Skeeball minigame
	public SkeeballMiniGameScript SkeeballLogic;
	// The score to award when this is triggered (multiplied within SkeeballLogic)
	public int Score;

	void OnTriggerEnter( Collider other )
	{
		SkeeballLogic.AddScore( Score );
		Destroy( other.gameObject );
	}
}