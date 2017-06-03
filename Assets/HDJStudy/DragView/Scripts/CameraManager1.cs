using UnityEngine;
using System.Collections.Generic;

public class CameraManager1 : MonoBehaviour {
    private float eulerAngles_y;
    private float eulerAngles_x;

    Vector2 dragV2 = Vector2.zero;


    Transform TheTrans;
    public Transform TargetTransform;
    public Transform[] cubeTrans;
    public Transform headParent;

    Vector3 targetPos;
    public Vector3 RelateCamPos;

    public GameObject headText;

    void Awake()
    {
        TheTrans = this.transform;
    }

    void Start()
    {
        RelateCamPos = TheTrans.position - TargetTransform.position;
        SetHead();
    }

    public void SetDrag(Vector2 v2)
    {
        dragV2 = v2;
    }
    
    void LateUpdate()
    {
        targetPos = TargetTransform.position + RelateCamPos;
        TheTrans.position = targetPos;
    }

    // Update is called once per frame
    void FixedUpdate ()
    {

        float xx = 0;
        xx = dragV2.x;
        dragV2 = Vector2.zero;
        if (xx != 0)
        {
#if UNITY_EDITOR
            this.eulerAngles_x = (Input.GetAxis("Mouse X") * 300f) * Time.deltaTime;

            //this.eulerAngles_y -= (Input.GetAxis("Mouse Y") * 200f) * Time.deltaTime;
#else
					this.eulerAngles_x += (Input.GetAxis("Mouse X") * 60f) * Time.deltaTime;

					//this.eulerAngles_y -= (Input.GetAxis("Mouse Y") * 15f) * Time.deltaTime;
#endif


            eulerAngles_y = Mathf.Clamp(eulerAngles_y, 19f, 20f);
            Quaternion quaternion = Quaternion.Euler(this.eulerAngles_y, this.eulerAngles_x, (float)0);
            //transform.rotation = quaternion;
            transform.RotateAround(TargetTransform.position,Vector3.up, eulerAngles_x);
            RelateCamPos = transform.position - TargetTransform.position;
        }
        UpdatePos();
    }

    void SetHead()
    {
        GameObject go = Instantiate(headText);
        Vector3 pos = TargetTransform.position;
        pos.y += 1f;
        var p1 = Camera.main.WorldToScreenPoint(pos);
        Vector3 p2 = UICamera.mainCamera.ScreenToWorldPoint(p1);
        p2.z = 0;
        go.transform.parent = headParent;
        go.transform.localScale = Vector3.one;
        go.transform.position = p2;

        for(int i = 0; i < cubeTrans.Length; i++)
        {
            GameObject go1 = Instantiate(headText);
            Vector3 pos1 = cubeTrans[i].position;
            pos1.y += 1f;
            var p3 = Camera.main.WorldToScreenPoint(pos1);
            Vector3 p4 = UICamera.mainCamera.ScreenToWorldPoint(p3);
            p4.z = 0;
            go1.transform.parent = headParent;
            go1.transform.localScale = Vector3.one;
            go1.transform.position = p4;
            headGo.Add(go1);
        }
    }
    
    private List<GameObject> headGo = new List<GameObject>();
    public float maxDistance = 50f;
    void UpdatePos()
    {
        float distance = 0;
        for(int i = 0; i < headGo.Count; i++)
        {
            distance = Vector3.Distance(TargetTransform.position, cubeTrans[i].position);
            if(distance > maxDistance)
            {
                cubeTrans[i].gameObject.SetActive(false);
                headGo[i].SetActive(false);
            }
            else
            {
                cubeTrans[i].gameObject.SetActive(true);
                headGo[i].SetActive(true);
                Vector3 pos1 = cubeTrans[i].position;
                pos1.y += 1f;
                var p3 = Camera.main.WorldToScreenPoint(pos1);
                Vector3 p4 = UICamera.mainCamera.ScreenToWorldPoint(p3);
                p4.z = 0;
                headGo[i].transform.position = p4;
            }
        }
        
    }
}
