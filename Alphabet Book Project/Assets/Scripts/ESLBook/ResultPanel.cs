using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultPanel :  Singleton<ResultPanel> {
	public Button MenuButton;
	public Button NextButton;
	public Button ReplayButton;
	public Transform NextBook;
	BookController bc;
	void Start () {
		bc = BookController.Instance;
		MenuButton.onClick.AddListener (()=>{
			//TODO:back to menu
			gameObject.SetActive(false);
		});
		NextButton.onClick.AddListener (()=>{
			gameObject.SetActive(false);
			bc.CurrentBookIndex = (bc.CurrentBookIndex+1)%bc.BookDataLength;
			bc.OpenBook();
		});
		ReplayButton.onClick.AddListener (()=>{
			bc.OpenBook();
			gameObject.SetActive(false);
		});
	}
}
