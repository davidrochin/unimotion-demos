using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Data/Item", order = 1)]
public class Item : ScriptableObject {

    [Header("Basic")]
    public string name;
    public string description;
    public float cost = 300f;

    [Header("Visual")]
    public GameObject prefab;
    public Sprite sprite;
	
}
