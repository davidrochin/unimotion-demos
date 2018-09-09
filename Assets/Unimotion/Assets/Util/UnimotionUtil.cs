using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnimotionUtil {

    public static RaycastHit[] hits = new RaycastHit[20];
    public static Collider[] overlaps = new Collider[20];

    public static RaycastHit CapsuleCastIgnoreSelf(Vector3 point1, Vector3 point2, float radius, Vector3 direction, float maxDistance, int mask, QueryTriggerInteraction queryTriggerInteraction, Collider self) {
        int n = Physics.CapsuleCastNonAlloc(point1, point2, radius, direction, hits, maxDistance, mask, queryTriggerInteraction);

        if (n > 0) {
            RaycastHit best = new RaycastHit();

            for (int i = 0; i < n; i++) {
                if (best.collider == null) {
                    if (hits[i].collider != self) {
                        best = hits[i];
                    }
                } else {
                    if (hits[i].distance < best.distance && hits[i].collider != self) {
                        best = hits[i];
                    }
                }
            }

            if (best.collider != null) {
                return best;
            }
        }

        return new RaycastHit();
    }

    public static T[] RemoveNull<T>(T[] array) {
        List<T> list = new List<T>();
        foreach (T t in array) {
            if (t != null) {list.Add(t);}
        }
        return list.ToArray();
    }


    public static Texture2D CreateEmpty(Color color) {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, color);
        tex.Apply();
        return tex;
    }

    public static Vector3 MeanDirection(Vector3[] directions) {
        Vector3 meanDirection = directions[0];
        if (directions.Length > 1) {
            for (int i = 1; i < directions.Length; i++) { meanDirection = Vector3.Slerp(meanDirection, directions[i], 0.5f); }
        }
        return meanDirection;
    }

}
