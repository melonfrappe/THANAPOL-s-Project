using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Exit : MonoBehaviour {

	public void ExitPanel(){
		this.transform.parent.gameObject.SetActive(false);
	}
}
