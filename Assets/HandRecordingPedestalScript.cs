using UnityEngine;
using System.Collections;
using Leap;

public class HandRecordingPedestalScript : MonoBehaviour
{
	void Update()
	{
		GetComponent<HandController>().PauseRecording();
	}
}