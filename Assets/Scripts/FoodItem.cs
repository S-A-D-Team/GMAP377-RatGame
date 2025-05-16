using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodItem : Contaminable
{
    [Space]
    [Header("Food Details")]
    public bool poisoned;
	public ParticleSystem poisonedEffect;
    [SerializeField] private bool isColliding = false;

    protected override void Start()
	{
		base.Start();
		//make sure the fx is not playing 
		poisonedEffect.Stop();
	}
    private void Update()
    {
        if (isColliding)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                EatItem();
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                ContaminateItem();
            }
        }
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

    protected virtual void EatItem()
    {
        GameManager.Instance.changeHunger(0.1f);
        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        //because we need to triggr UI first
        if (collision.gameObject.tag.ToLower().Contains("player"))
        {
            isColliding = true;
            UIManager.Instance.showInteractCue(true);
            UIManager.Instance.showInfectionCue(true);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag.ToLower().Contains("player"))
        {
            isColliding = true;
            Debug.Log("FOODING");
            UIManager.Instance.showInteractCue(true);
            UIManager.Instance.showInfectionCue(true);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag.ToLower().Contains("player"))
        {
            isColliding = false;
            UIManager.Instance.showInteractCue(false);
            UIManager.Instance.showInfectionCue(false);
        }
    }
}
