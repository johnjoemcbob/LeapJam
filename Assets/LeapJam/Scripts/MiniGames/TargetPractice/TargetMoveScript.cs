using UnityEngine;
using System.Collections;

public class TargetMoveScript : MonoBehaviour
{
	// The minimum and maximum positions to lerp this target between
	public Transform MinimumTarget;
	public Transform MaximumTarget;
	// The direction to start movement in (positive or negative)
	public int Direction = 1;
	// The speed to lerp between the min/max positions
	public float Speed = 1;

	// The current lerp time value
	private float LerpTime = 0;

	void Update()
	{
		// Increment the lerp time by frame time
		LerpTime += Time.deltaTime * Speed;
		// Move the target
		if ( Direction > 0 )
		{
			transform.position = Vector3.Lerp( MinimumTarget.position, MaximumTarget.position, LerpTime );
		}
		else if ( Direction < 0 )
		{
			transform.position = Vector3.Lerp( MaximumTarget.position, MinimumTarget.position, LerpTime );
		}
		// Reverse the direction when the min/max is reached
		if ( LerpTime >= 1 )
		{
			LerpTime = 0;
			Direction *= -1;
		}
	}
}