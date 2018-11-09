using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnselectedPanel : MonoBehaviour {
	BookController bc;

	void Start () {
		bc = BookController.Instance;
	}

	public void OnClick(){
		bc.CancelSelecting ();
		gameObject.SetActive (false);
	}
}
