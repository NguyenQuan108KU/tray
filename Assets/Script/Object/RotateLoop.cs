using UnityEngine;

public class RotateLoop : MonoBehaviour
{
    public float rotateSpeed = 20f; // độ/giây, chỉnh cho nhanh/chậm

    void Update()
    {
        transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
    }
}
