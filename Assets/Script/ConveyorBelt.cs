using UnityEngine;
using System.Collections.Generic;

public class ConveyorBelt : MonoBehaviour
{
    public float speed = 2f;
    public float spacing = 1.5f;
    public float leftLimit = -8f;
    // public int removeFirstCount = 4;

    private List<Transform> items = new List<Transform>();
    // private int removedCount = 0;
    // private bool isStarted = false;

    public GameObject tutorial;

    [Header("Setting")]
    public float bottomLimit = -8f;
    public bool isDown;
    private bool isRunning = false;


    void Start()
    {
        foreach (Transform child in transform)
        {
            items.Add(child);
        }

        SortItems();
    }

    void Update()
    {
        // if (!isStarted)
        // {
        //     if (Input.GetMouseButtonDown(0))
        //     {
        //         isStarted = true;
        //         tutorial.SetActive(false);
        //     }
        //     return;
        // }
        if (!isRunning && Input.GetMouseButtonDown(0))
            isRunning = true;
        
        if (!isRunning) return;
        DirectionOfMove(isDown);
        HandleItemOutOfBound();
    }

    // Hướng di chuyển của Tray (lên hoặc xuống)
    public void DirectionOfMove(bool isDown)
    {
        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (isDown)
                items[i].Translate(Vector3.down * speed * Time.deltaTime);
            else
                items[i].Translate(Vector3.up * speed * Time.deltaTime);
        }
    }

    void HandleItemOutOfBound()
    {
        if (items.Count == 0) return;

        Transform first = items[0];

        // CỘT ĐI XUỐNG
        if (isDown && first.position.y < bottomLimit)
        {
            RecycleItem(first, true);
        }
        // CỘT ĐI LÊN
        else if (!isDown && first.position.y > bottomLimit)
        {
            RecycleItem(first, false);
        }
    }

    void RecycleItem(Transform first, bool isDown)
    {
        // if (removedCount < removeFirstCount)
        // {
        //     items.RemoveAt(0);
        //     Destroy(first.gameObject);
        //     removedCount++;
        //     return;
        // }

        Transform last = items[items.Count - 1];
        Vector3 newPos = last.position;

        if (isDown)
            newPos.y += spacing;   // spawn phía trên
        else
            newPos.y -= spacing;   // spawn phía dưới

        first.position = newPos;

        items.RemoveAt(0);
        items.Add(first);
    }

    void SortItems()
    {
        if (isDown)
            items.Sort((a, b) => a.position.y.CompareTo(b.position.y));
        else
            items.Sort((a, b) => b.position.y.CompareTo(a.position.y));
    }
}
