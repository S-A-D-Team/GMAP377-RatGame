using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodItem : Contaminatable
{
    [Space]
    [Header("Food Details")]
    public bool poisoned;
	public ParticleSystem poisonedEffect;

	protected override void Start()
	{
		base.Start();
		//make sure the fx is not playing 
		poisonedEffect.Stop();
	}

	protected override void Update()
	{
		base.Update();
	}

	protected virtual void onPoisoned()
	{
		poisonedEffect.Play();
	}

}
