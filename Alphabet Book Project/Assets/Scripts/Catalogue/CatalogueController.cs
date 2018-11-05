using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CatalogueController : Singleton<CatalogueController> {

	public GameObject TopShelf;
	public GameObject MidShelf;
	public GameObject BookCover;
	public Button BackButton;
	public Button SearchBar;
	public Transform VerticalContent;
	public InputField SearchInput;
	public int catalogueCount;
	public GameObject Book;
	// Use this for initialization
	void Start () {
		CloneShelf (catalogueCount);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void CloneShelf(int count){
		for(int i=0; i<count ;i++){
			Instantiate (MidShelf, VerticalContent);
		}
	}

	public void OpenBook(){
		if(!Book.activeInHierarchy)
			Book.SetActive (true);
	}
}
