using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterMaterialAnimator : MonoBehaviour {

    Material materialInstance;

	void Awake () {
        materialInstance = Instantiate<Material>(GetComponent<Renderer>().material);
        GetComponent<Renderer>().material = materialInstance;
	}
	
	void Update () {
        materialInstance.SetTextureOffset("_MainTex", materialInstance.GetTextureOffset("_MainTex") +  Vector2.up * 0.2f * Time.deltaTime);
        materialInstance.SetTextureOffset("_DetailAlbedoMap", materialInstance.GetTextureOffset("_DetailAlbedoMap") + Vector2.right * 0.1f * Time.deltaTime);
    }
}
