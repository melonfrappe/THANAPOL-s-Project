using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MidBookShelf : MonoBehaviour {
	public string CatagoryName;
	public int ShelfIndex;
	public Transform MidBookShelfContent;
	void Start () {
		ShelfIndex = transform.GetSiblingIndex()-1;
	}

}
