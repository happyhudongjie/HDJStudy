using UnityEngine;
using System.Collections;

public class TestResourcesUnLoad : MonoBehaviour {

    public GameObject cube;
    public UILabel label;
    private ArrayList goArray = new ArrayList();

    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            StartCoroutine(SetString());
        }
    }

    IEnumerator SetString()
    {
        string temp = "";
        for(int i = 0; i < 10; i++)
        {
            temp += i;
            yield return new WaitForSeconds(0.1f);
            label.text = temp;
        }
    }
}
