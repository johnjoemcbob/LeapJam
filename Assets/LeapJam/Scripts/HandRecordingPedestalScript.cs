using UnityEngine;
using System.Collections;
using Leap;

public class HandRecordingPedestalScript : MonoBehaviour
{
	private float ExistTime = 0;

	void Update()
	{
		ExistTime += Time.deltaTime;
		if ( ExistTime >= 1 )
		{
			GetComponent<HandController>().PauseRecording();
		}
	}
}