using UnityEngine;

public struct TransformState {

    public Transform transform;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public Vector3 forward;
    public Vector3 right;
    public Vector3 up;

    public TransformState(Vector3 position, Quaternion rotation, Vector3 scale, Transform transform) {
        this.position = position;
        this.rotation = rotation;
        this.scale = scale;
        this.transform = transform;

        if(transform != null) {
            forward = transform.forward;
            right = transform.right;
            up = transform.up;
        } else {
            forward = Vector3.zero;
            right = Vector3.zero;
            up = Vector3.zero;
        }
    }

    public static TransformState From(Transform transform) {
        return new TransformState(transform.position, transform.rotation, transform.localScale, transform);
    }

    public static TransformState Empty() {
        return new TransformState(Vector3.zero, Quaternion.identity, Vector3.zero, null);
    }
}
