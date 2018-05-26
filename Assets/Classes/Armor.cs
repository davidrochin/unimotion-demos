using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Armor", menuName = "Data/Armor", order = 3)]
public class Armor : Item {
    [Header("Armor")]
    public Type type;
    public float protection;
    public enum Type { Head, Torso, Legs, General }
}

