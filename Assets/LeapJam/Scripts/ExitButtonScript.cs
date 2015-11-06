using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using LMWidgets;

public class ExitButtonScript : ButtonToggleBase
{
	// Stop presses being registered by mistake on creation
	static private float TimeBeforePress = 0.4f;
	private float TimeCreated = 0;

	protected override void Start()
	{
		base.Start();

		TimeCreated = Time.time;
	}

	void OnEnable()
	{
		TimeCreated = Time.time;
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
	}

	public override void ButtonTurnsOn()
	{
		m_toggleState = false;
		if ( ( TimeCreated + TimeBeforePress ) > Time.time ) return;

		Application.Quit();
		m_toggleState = false;
	}

	public override void ButtonTurnsOff()
	{
	}
}