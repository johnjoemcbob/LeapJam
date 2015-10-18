using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using LMWidgets;

public class PlayButtonScript : ButtonToggleBase
{
	// Reference to the main game logic script
	public MainLogicScript MainLogic;
	// Reference to the text UI object to change from PLAY to ... and back again
	public Text ButtonText;

	// The delay after pressing after which the state will be changed to play
	private float PressDelay = -1;

	protected override void Start()
	{
		base.Start();
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();

		if ( PressDelay != -1 )
		{
			// While delaying, animate the text a little
			float time = PressDelay - Time.time;
			if ( ( Mathf.Ceil( time * 10 ) % 9 ) == 0 )
			{
				ButtonText.text = "Cancel?\n.";
			}
			else if ( ( Mathf.Ceil( time * 10 ) % 6 ) == 0 )
			{
				ButtonText.text = "Cancel?\n..";
			}
			else if ( ( Mathf.Ceil( time * 10 ) % 3 ) == 0 )
			{
				ButtonText.text = "Cancel?\n...";
			}
			// Start the game when the delay ends
			if ( PressDelay <= Time.time )
			{
				MainLogic.StartPlay();
			}
		}
	}

	public override void ButtonTurnsOn()
	{
		PressDelay = Time.time + 1;
		ButtonText.text = "Cancel?\n...";
	}

	public override void ButtonTurnsOff()
	{
		PressDelay = -1;
		ButtonText.text = "PLAY";
	}
}