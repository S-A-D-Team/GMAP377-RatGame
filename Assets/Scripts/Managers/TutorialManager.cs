using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [Header("Narration Texts")]
    [TextArea] public string introNarration;
    [TextArea] public string hungerNarration;
    [TextArea] public string staminaNarration;
    [TextArea] public string mutationNarration;
    [TextArea] public string endTutorialNarration;
    [TextArea] public string humanNarration;
    [TextArea] public string catNarration;

    public int tutorialStage = 0;

    private void Start()
    {
        UIManager.Instance.beginTutorial(0);
    }


    public void PlayIntroNarration()
    {
        UIManager.Instance.SetTutorialCaption(introNarration);
        UIManager.Instance.hungerHighlight.gameObject.SetActive(false);
        UIManager.Instance.staminaHighlight.gameObject.SetActive(false);
        UIManager.Instance.mutationHighlight.gameObject.SetActive(false);
    }

    public void PlayHungerNarration()
    {
        UIManager.Instance.SetTutorialCaption(hungerNarration);
        UIManager.Instance.hungerHighlight.gameObject.SetActive(true);
        UIManager.Instance.staminaHighlight.gameObject.SetActive(false);
        UIManager.Instance.mutationHighlight.gameObject.SetActive(false);
    }

    public void PlayStaminaNarration()
    {
        UIManager.Instance.SetTutorialCaption(staminaNarration);
        UIManager.Instance.hungerHighlight.gameObject.SetActive(false);
        UIManager.Instance.staminaHighlight.gameObject.SetActive(true);
        UIManager.Instance.mutationHighlight.gameObject.SetActive(false);
    }

    public void PlayMutationNarration()
    {
        UIManager.Instance.SetTutorialCaption(mutationNarration);
        UIManager.Instance.hungerHighlight.gameObject.SetActive(false);
        UIManager.Instance.staminaHighlight.gameObject.SetActive(false);
        UIManager.Instance.mutationHighlight.gameObject.SetActive(true);
    }

    public void PlayEndTutorialNarration()
    {
        UIManager.Instance.SetTutorialCaption(endTutorialNarration);
        UIManager.Instance.hungerHighlight.gameObject.SetActive(false);
        UIManager.Instance.staminaHighlight.gameObject.SetActive(false);
        UIManager.Instance.mutationHighlight.gameObject.SetActive(false);
    }

    public void PlayHumanNarration()
    {
        UIManager.Instance.SetTutorialCaption(humanNarration);
        UIManager.Instance.hungerHighlight.gameObject.SetActive(false);
        UIManager.Instance.staminaHighlight.gameObject.SetActive(false);
        UIManager.Instance.mutationHighlight.gameObject.SetActive(false);
    }

    public void PlayCatNarration()
    {
        UIManager.Instance.SetTutorialCaption(catNarration);
        UIManager.Instance.hungerHighlight.gameObject.SetActive(false);
        UIManager.Instance.staminaHighlight.gameObject.SetActive(false);
        UIManager.Instance.mutationHighlight.gameObject.SetActive(false);
    }

    public void SetTutorial()
    {
        switch (tutorialStage)
        {
            case 0:
                PlayIntroNarration();
                break;
            case 1:
                PlayHungerNarration();
                break;
            case 2:
                PlayStaminaNarration();
                break;
            case 3:
                PlayMutationNarration();
                break;
            case 4:
                PlayEndTutorialNarration();
                GameObject.FindGameObjectWithTag("player").GetComponent<PlayerMovement>().spawnRandom();
                tutorialStage = 9;
                break;
            case 5:
                PlayHumanNarration();
                tutorialStage = 9;
                break;
            case 6:
                PlayCatNarration();
                tutorialStage = 9;
                break;
            case 10:
                UIManager.Instance.SetTutorialCaption("");
                UIManager.Instance.endTutorial();
                return;
        }
    }

    public void ContinueTutorial()
    {
        tutorialStage++;
        SetTutorial();
    }
}
