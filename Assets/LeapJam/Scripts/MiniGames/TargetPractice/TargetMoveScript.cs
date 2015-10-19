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
	// The minimum height to start the target at
	public float MinY = -2.5f;

	// The current lerp time value
	private float LerpTime = 0;
	// The current height lerp time value
	private float LerpTimeY = 0;
	// The state of animation; in0, loop1, out2
	private int AnimationState = 0;
	// The scene Y of the target when the minigame starts
	private float NormalY = 0;

	void Start()
	{
		NormalY = transform.position.y;
	}

	void Update()
	{
		if ( ( AnimationState == 0 ) || ( AnimationState == 1 ) )
		{
			// Increment the lerp time by frame time
			LerpTime += Time.deltaTime * Speed;

			// Move the target
			if ( Direction > 0 )
			{
				transform.position = Vector3.Lerp( MinimumTarget.position, MaximumTarget.position, LerpTime );
				NormalY = transform.position.y;
			}
			else if ( Direction < 0 )
			{
				transform.position = Vector3.Lerp( MaximumTarget.position, MinimumTarget.position, LerpTime );
				NormalY = transform.position.y;
			}
		}
		if ( AnimationState == 0 )
		{
			// Increment the lerp time by frame time
			LerpTimeY += Time.deltaTime * 1;
			LerpTimeY = Mathf.Clamp( LerpTimeY, 0, 1 );

			float distance = NormalY - MinY;
			transform.position = new Vector3( transform.position.x, MinY + ( distance * LerpTimeY ), transform.position.z );
		}
		if ( AnimationState == 2 )
		{
			// Increment the lerp time by frame time
			LerpTime += Time.deltaTime * 1;

			float distance = NormalY - MinY;
			//transform.position = new Vector3( transform.position.x, MinY - ( distance * LerpTime ) + distance, transform.position.z );
		}
		// Reverse the direction when the min/max is reached
		if ( LerpTime >= 1 )
		{
			LerpTime = 0;
			LerpTimeY = 0;
			Direction *= -1;

			// Move from in to loop
			if ( AnimationState == 0 )
			{
				AnimationState = 1;
			}
		}
	}

	public void SetAnimationState( int state )
	{
		AnimationState = state;
	}
}