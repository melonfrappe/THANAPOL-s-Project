using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultPanel :  Singleton<ResultPanel> {
	public Button MenuButton;
	public Button NextButton;
	public Button ReplayButton;
	public Image NextBookTemplate;
	public Image NextBookCoverImage;
	BookController bc;
	void Start () {
		bc = BookController.Instance;
		MenuButton.onClick.AddListener (()=>{
			//TODO:back to menu
			bc.BackToCatalogue();
			gameObject.SetActive(false);
		});
		NextButton.onClick.AddListener (()=>{
			gameObject.SetActive(false);
			bc.CurrentBookIndex = (bc.CurrentBookIndex+1)%bc.BookDataLength;
			bc.OpeningBook.SetActive(false);
			bc.OpenBook();

		});
		ReplayButton.onClick.AddListener (()=>{
			gameObject.SetActive(false);
			bc.OpenBook();

		});
	}

	// void OnDisable(){
	// 	for(int i=0; i<NextBook.childCount; i++)
	// 		Destroy(NextBook.GetChild (i).gameObject);
	// }
}
