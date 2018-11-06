using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CatalogueController : Singleton<CatalogueController> {

	public GameObject TopShelf;
	public GameObject MidShelf;
	public GameObject MidBookCover;
	public Button BackButton;
	public Button SearchBar;
	public Transform HorizontalContent;
	public Transform VerticalContent;
	public InputField SearchInput;
	public int catalogueCount;
	public GameObject OpenedBook;
	public Book Book;

	BookController bc;

	void Start () {
		
		bc = BookController.Instance;

		//CloneShelf (catalogueCount);
	}

	public void CloneShelf(int count){
		for(int i=0; i<count ;i++){
			Instantiate (MidShelf, VerticalContent);
		}
	}
}
