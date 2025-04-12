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

	protected virtual void onPoisoned()
	{
		poisonedEffect.Play();

		//Always show contaminated
        //mat.SetFloat(lerpProperty, 100);
    }

	//tick
	//ater we update the contamination and buildup values
    protected override void atMinutePass()
    {
        base.atMinutePass();
		//if we exceed value,
		if(contaminationValue >= 100 && !poisonedEffect.isPlaying)
		{
            onPoisoned();
        }
    }
}
