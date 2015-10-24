using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using LMWidgets;

public class ResetButtonScript : ButtonToggleBase
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
			// Reset the resetbutton text
			if ( PressDelay <= Time.time )
			{
				PressDelay = -1;
				ButtonText.text = "RESET?";
			}
		}
	}

	public override void ButtonTurnsOn()
	{
		PressDelay = Time.time + 1;
		ButtonText.text = "GAME\nRESET!";
		MainLogic.Reset();
		m_toggleState = false;
	}

	public override void ButtonTurnsOff()
	{
		
	}
}