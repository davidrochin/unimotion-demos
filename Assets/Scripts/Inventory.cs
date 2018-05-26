using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour {

    //Events
    public Action OnItemAquired;
    public Action OnItemDropped;
    public Action OnItemUsed;

    public List<Item> list;

}
