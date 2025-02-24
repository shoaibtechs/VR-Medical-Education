﻿using Packages.Scene; 
using Packages.Scene.Generic;
using UnityEngine;

public enum NeedleOption
{
    IV,
    IM,
    SC,
    Transfer
}
public class NeedleUse : MonoBehaviour {
    //used to check what kind of injections this needle can be used for
    //also used on body parts to know if injectionmethod can be used here
    public NeedleOption[] CanBeUsedFor;
}
