using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnselectedPanel : MonoBehaviour {
	BookController bc;

	void Start () {
		bc = BookController.Instance;
	}

	public void OnClickUnselectedPanel(){
		bc.CancelSelecting ();
	}

}
