﻿using UnityEngine;
using System.Collections;

public class PlayerCountSlider : SliderDemo
{
	public MainLogicScript MainLogic;

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
	}
}