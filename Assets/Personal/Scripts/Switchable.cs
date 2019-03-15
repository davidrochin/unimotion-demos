using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Switchable : MonoBehaviour {

    public abstract bool State { get; }

    public abstract void Switch();

}
