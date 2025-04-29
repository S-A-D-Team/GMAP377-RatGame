using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IToggleable
{
    public void Toggle();
    public bool getCurrentFlag();

    public void setCurrentFlag(bool flag);

    public void notifyFlag();
}
