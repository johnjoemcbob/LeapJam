using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerCountSlider : SliderDemo
{
	public MainLogicScript MainLogic;
	public Text PlayerText;

	protected override void Start()
	{
		base.Start();

		transform.localPosition = new Vector3( -2, transform.localPosition.y, transform.localPosition.z );
	}

	protected override void sliderReleased()
	{
		base.sliderReleased();

		int dot = 0;
			for ( int i = 0; i < dots.Count; ++i )
			{
				if ( dots[i].transform.localPosition.x < transform.localPosition.x )
				{
					dot = i;
				}
			}
		MainLogic.SetPlayers( dot + 1 );
		PlayerText.text = string.Format( "PLAYERS: {0}", dot + 1 );
	}
}