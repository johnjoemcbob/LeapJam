using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using LMWidgets;

public class BackButtonScript : ButtonToggleBase
{
	// References to both the current state and the state to switch to
	public GameObject CurrentState;
	public GameObject NextState;

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

		// Change states
		//CurrentState.SetActive( false );
		//NextState.SetActive( true );
		CurrentState.transform.position = new Vector3( 0, 0, 30 );
		NextState.transform.position = new Vector3( 0, 0, 0 );

		// Respawn any objects which require respawning in the new state
		foreach ( ButtonToggleBase button in NextState.transform.GetComponentsInChildren<ButtonToggleBase>() )
		{
			button.Invoke( "OnEnable", 0 );
		}

		TimeCreated = Time.time;
	}

	public override void ButtonTurnsOff()
	{
	}
}