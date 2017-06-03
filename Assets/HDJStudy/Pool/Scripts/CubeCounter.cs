using UnityEngine;
using System.Collections;

public class CubeCounter : MonoBehaviour {
    public TestNewPool controller;
    public float time = 3f;
    public float timer;

    public bool cancelCounter = false;
	void Update()
    {
        if (cancelCounter) return;
        timer += Time.deltaTime;
        if (timer >= time)
        {
            this.gameObject.SetActive(false);
            cancelCounter = true;
            if (controller != null)
            {
                controller.pool.Store(this.gameObject);
            }
        }
    }
}
