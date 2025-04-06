using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodItem : Contaminatable
{
    [Space]
    [Header("Food Details")]
    public int poisoned;

	protected override void Start()
	{
		base.Start();
	}

	protected override void Update()
	{
		base.Update();
	}

}
