using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestNewPool : MonoBehaviour {

    public GameObject cube;

    public ObjectPool<GameObject> pool = new ObjectPool<GameObject>();
    
    public float spawnTime = 2f;
    public float spawnTimer = 2f;

    private int index = -1;
    private int lastIndex = 0;
    List<GameObject> _goList = new List<GameObject>();

    void Update()
    {
        spawnTimer += Time.deltaTime;
        if (Input.GetMouseButtonDown(0) && spawnTimer >= spawnTime)
        {
            GameObject go = null;
            spawnTimer = 0;
            bool isNew = true;
            go = pool.New("Cube",out isNew);
            if(isNew == true || go.activeSelf == true)
            {
                go = Instantiate(go);
                go.GetComponent<CubeCounter>().controller = this;
                go.name = "cube";
                go.transform.localScale = Vector3.one;
                go.transform.position = Vector3.zero;
            }
            else
            {

                go.SetActive(true);
                go.GetComponent<CubeCounter>().timer = 0;
                go.GetComponent<CubeCounter>().cancelCounter = false;
            }

            _goList.Add(go);
        }
    }
}
