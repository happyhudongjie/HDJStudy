using UnityEngine;
using System.Collections;

public class TestDrag : MonoBehaviour {
    
    public CameraManager1 cam;
    // Use this for initialization
    void Start()
    {
        UIEventListener.Get(gameObject).onDrag += OnDragObject;

    }
    
    private void OnDragObject(GameObject go, Vector2 deltaPos)
    {
        cam.SetDrag(deltaPos);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
