using UnityEngine;
using System.Collections;

public class CubeMove : MonoBehaviour {



    void Update()
    {
        MoveCube();
    }



    void MoveCube()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        this.transform.Translate(new Vector3(h, 0, v));
    }
}
