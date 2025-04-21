using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ToggleBox : MonoBehaviour
{
    // Reference to the button component
    public Button toggleButton;

    // The child GameObject to toggle
    public GameObject childObject;
    public bool value;
    public int paramID;

    public System.Action<bool> onValueChanged;

    public void Init(bool _initValue)
    {
        value = _initValue;
        childObject.SetActive(value);

        toggleButton.onClick.AddListener(() =>
        {
            value = !value;
            childObject.SetActive(value);
            onValueChanged?.Invoke(value); //ping em
        });

    }

}
