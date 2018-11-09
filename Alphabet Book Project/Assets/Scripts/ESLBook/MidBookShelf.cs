using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class MidBookShelf : MonoBehaviour {
	public string Catagory;
	public Text CatagoryLabel;
	public int ShelfIndex;
	public Transform MidBookShelfContent;
	BookController bc;

	void Start () {
		bc = BookController.Instance;
		ShelfIndex = transform.GetSiblingIndex()-1;
		CatagoryLabel.text = bc.CatagoryName[ShelfIndex];
		Catagory = CatagoryLabel.text.ToString ();
	}
}
