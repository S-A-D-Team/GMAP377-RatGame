using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodItem : Contaminable
{
    [Space]
    [Header("Food Details")]
    public bool poisoned;
	public ParticleSystem poisonedEffect;
    //public ParticleSystem poisoningEffect;
    public GameObject chompEffect;
    public float chompAnimLength;
    private bool isPoisoning = false;
    private bool isEaten = false;
    [SerializeField] private bool isColliding = false;

    private AudioSource audioData;

    protected override void Awake()
    {
        base.Awake();
        audioData = GetComponent<AudioSource>();
    }
    protected override void Start()
	{
		base.Start();
		//make sure the fx is not playing and the mesh is active
		poisonedEffect.Stop();
        audioData.Stop();
        gameObject.GetComponent<MeshRenderer>().enabled = true;
        //if (poisoningEffect != null) { poisoningEffect.Stop(); }

	}
    protected void Update()
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
            else if (Input.GetKeyDown(KeyCode.F) && !poisoned)
            {
                /*
                if (poisoningEffect == null)
                {
                    ContaminateItem();
                }
                else if (!isPoisoning)
                {
                    StartCoroutine(Contaminating());
                }*/
                StartCoroutine(Contaminating());
            }
        }
    }

    IEnumerator Contaminating()
    {
        isPoisoning = true;
        //Minimum input held window to start infecting
        float holdWindow = 0.1f;
        //start the UI
        UIManager.Instance.showContainationBuildUp(true, 0f);
        while (holdWindow > 0f)
        {
            if (!Input.GetKey(KeyCode.F) || Input.GetKeyUp(KeyCode.F))
            {
                yield break;
            }
            holdWindow -= Time.deltaTime;
        }
        //Must hold the input for a certain amount of time to apply contamination build up, with particle system to visualize the charge
        //poisoningEffect.Play();
        float chargeWindow = 2f;
        float _completeChargeTime = chargeWindow;
        while (Input.GetKey(KeyCode.F) && chargeWindow > 0f)
        {
            if (!Input.GetKey(KeyCode.F) || Input.GetKeyUp(KeyCode.F))
            {
                break;
            }
            chargeWindow -= Time.deltaTime;
            //update UI
            UIManager.Instance.showContainationBuildUp(true, ((_completeChargeTime - chargeWindow)/_completeChargeTime));
            yield return null;
        }
        //close UI
        UIManager.Instance.showContainationBuildUp(false, 0f);
        //If action fully charged, apply contamination
        //poisoningEffect.Stop();
        if (chargeWindow <= 0f)
        {
            ContaminateItem();
            onPoisoned();
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

    protected virtual void onFullyContaminated()
    {
        //change the contam to differeft color ig
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
            onFullyContaminated(); 
        }
    }

    protected virtual void EatItem()
    {
        isEaten = true;
        audioData.Play();
        gameObject.GetComponent<MeshRenderer>().enabled = false;
        GameManager.Instance.changeHunger(0.1f * (int)potency);

        //UI 
        UIManager.Instance.showInteractCue(false);
        UIManager.Instance.showInfectionCue(false);

        //Prefab should simply play a chomp animation on spawn and have the object despawn when it's finished
        if (chompEffect != null)
        {
            Instantiate(chompEffect, transform.position, Quaternion.identity);
            Destroy(gameObject, chompAnimLength);

        }
        else
        {
            Destroy(gameObject, audioData.clip.length);
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
            //Debug.Log("FOODING");
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
