using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Shield", menuName = "Data/Shield", order = 3)]
public class Shield : Item {
    [Header("Stats")]
    public float protection = 1f;
}
