using UnityEngine;
using System.Collections;

public class HandWinAnimationScript : MonoBehaviour
{
	public float PlaybackSpeed = 10;

	private float Offset = 0;
	private float Sway = 0;

	void Start()
	{
		Offset = Random.Range( -200.0f, 200.0f ) / 100.0f;
	}

	void Update()
	{
		Sway += Time.deltaTime * PlaybackSpeed;
		float sin = Mathf.Sin( Sway + Offset );
		float cos = Mathf.Cos( Sway + Offset );
		float sincos = sin * cos;
		transform.localEulerAngles = new Vector3( sincos * -5, 0, sin * 20 );
		transform.localPosition = new Vector3( sincos * 0.05f, sincos * 0.08f, 0 );
	}
}