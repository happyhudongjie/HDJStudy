using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestJSTimer : MonoBehaviour {

	// Use this for initialization
	void Start () {
		JSTimer.Instance.SetUpTimer ("TimerDDDDD", ()=> {
			print ("Timer");
		}, 1);

		JSTimer.Instance.SetUpCoolDown ("CdCoolDown",60,remainTime=>{
			print("Cd: "+remainTime);
		},()=>{
			print("CD is finished");
		},1);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
