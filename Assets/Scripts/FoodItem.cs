using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodItem : Contaminable
{
    [Space]
    [Header("Food Details")]
    public bool poisoned;
	public ParticleSystem poisonedEffect;
    public ParticleSystem poisoningEffect;
    public GameObject chompEffect;
    public float chompAnimLength;
    private bool isPoisoning = false;
    private bool isEaten = false;
    [SerializeField] private bool isColliding = false;

    AudioSource audioData;

    protected override void Start()
	{
		base.Start();
		//make sure the fx is not playing 
		poisonedEffect.Stop();
        if (poisoningEffect != null) { poisoningEffect.Stop(); }
        audioData = GetComponent<AudioSource>();
	}
    private void Update()
    {
        if (isColliding)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (!poisoned && !isEaten)
                {
                    EatItem();
                }
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                if (poisoningEffect == null)
                {
                    ContaminateItem();
                }
                else if (!isPoisoning)
                {
                    StartCoroutine(Contaminating());
                }
                
            }
        }
    }

    IEnumerator Contaminating()
    {
        isPoisoning = true;
        //Minimum input held window to start infecting
        float holdWindow = 0.1f;
        while (holdWindow > 0f)
        {
            if (!Input.GetKey(KeyCode.F) || Input.GetKeyUp(KeyCode.F))
            {
                yield break;
            }
            holdWindow -= Time.deltaTime;
        }
        //Must hold the input for a certain amount of time to apply contamination build up, with particle system to visualize the charge
        poisoningEffect.Play();
        float chargeWindow = 2f;
        while (Input.GetKey(KeyCode.F) && chargeWindow > 0f)
        {
            if (!Input.GetKey(KeyCode.F) || Input.GetKeyUp(KeyCode.F))
            {
                break;
            }
            chargeWindow -= Time.deltaTime;
            yield return null;
        }
        //If action fully charged, apply contamination
        poisoningEffect.Stop();
        if (chargeWindow <= 0f)
        {
            ContaminateItem();
        }
        isPoisoning = false;
    }
    protected virtual void onPoisoned()
	{
		poisonedEffect.Play();
        poisoned = true;

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
        isEaten = true;
<<<<<<< Updated upstream
        UIManager.Instance.changeHungerBar(0.1f);
=======
        UIManager.Instance.changeHungerBar(0.1f * (int)potency);
        audioData.Play(0);
>>>>>>> Stashed changes
        //Prefab should simply play a chomp animation on spawn and have the object despawn when it's finished
        if (chompEffect != null)
        {
            Instantiate(chompEffect, transform.position, Quaternion.identity);
            Destroy(gameObject, chompAnimLength);

        }
        else
        {
            Destroy(gameObject);
        }
        
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
