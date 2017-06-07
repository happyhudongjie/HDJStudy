using UnityEngine;
using System.Collections;

public class ddd : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Read();

    }
    

    void Read()
    {
        DataParse da=  Resources.Load("shield") as DataParse;
        print(da.theData[1]);
        
    }

    // Update is called once per frame
    void Update () {
	
	}
}
