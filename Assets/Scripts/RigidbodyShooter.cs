using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyShooter : MonoBehaviour {

    public GameObject bullet;
    public Vector3 shootAreaSize = Vector3.one;
    public float frecuency = 0.3f;

    Bounds bounds;

    Queue<GameObject> poolQueue;

	void Awake () {

        RecalculateBounds();

        poolQueue = new Queue<GameObject>();
        for (int i = 0; i < 10; i++) {
            GameObject go = Instantiate(bullet, transform);
            poolQueue.Enqueue(go);
            go.SetActive(false);
        }

        StartCoroutine(Routine());
	}

    public void Shoot() {
        Vector3 shootPosition = new Vector3(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.y, bounds.max.y), Random.Range(bounds.min.z, bounds.max.z));
        GameObject bullet = poolQueue.Dequeue();
        bullet.SetActive(true);
        bullet.transform.position = shootPosition;
        bullet.GetComponent<Rigidbody>().velocity = Vector3.zero;
        bullet.GetComponent<Rigidbody>().AddForce(transform.forward * 1000f);
        poolQueue.Enqueue(bullet);
    }

    void RecalculateBounds() {
        bounds = new Bounds(transform.position, shootAreaSize);
    }

    IEnumerator Routine() {
        while (true) {
            Shoot();
            yield return new WaitForSeconds(frecuency);
        }
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, shootAreaSize);
        Gizmos.DrawRay(transform.position, transform.forward);
    }
}
