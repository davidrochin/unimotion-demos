using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Data/Weapon", order = 2)]
public class Weapon : Item {
    [Header("Stats")]
    public float damage = 70f;

    [Header("Moveset")]
    public WeaponMove[] moves;
}

public enum WeaponMove { OneHandedVerticalSwipe, OneHandedHorizontalSwipe }
