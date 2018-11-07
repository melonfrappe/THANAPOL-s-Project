﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;

public class CloningComponent : MonoBehaviour {
	public bool IsSelected;
	public int BookIndex;
	public List <Sprite> tmpPage = new List<Sprite>();
	public bool snapToSelectingBook = false;
	public Button coverButton;
	public Button playButton;
	public Button descriptionButton;
	public Image coverImage;
	public Image coverColor;
	BookController bc;
	ImageDownloader id;

	void Start(){
		bc = BookController.Instance;
		id = ImageDownloader.Instance;
		//Add button listener
		coverButton.onClick.AddListener (()=>{
			bc.UnselectedPanel.gameObject.SetActive(true);
			IsSelected = true;
			bc.IsSelecting = true;
			bc.CurrentBookIndex = BookIndex;
			SetIsSelected();
		});
		playButton.onClick.AddListener (()=>{
			bc.OpenBook();	
		});
		descriptionButton.onClick.AddListener (()=>{
			print(">>>Show description");
		});
	}

	public void SetIsSelected(){
		GameObject clone = Instantiate(this.gameObject,bc.UnselectedPanel.transform,true);
		for(int i=0; i<clone.transform.childCount; i++){
			if(!clone.transform.GetChild (i).gameObject.activeInHierarchy)
				clone.transform.GetChild (i).gameObject.SetActive(true);
		}
		Siri.TweenExtensions.Transform (clone.GetComponent<RectTransform>(), Siri.Rtype.Scale, clone.transform.localScale, new Vector2 (1.2f, 1.2f), 0.5f, 0, Easing.Type.EaseOutBounce);

	}
//	void Update(){
//		//Set each book cover
//		if(id.Counter == bc.BookDataLength && coverImage.sprite == null){
//			coverImage.sprite = bc.BookCoverImage[BookIndex];
//			coverImage.color = Color.white;
//			//Set color each book
//			Color tmpColor = new Color();
//			ColorUtility.TryParseHtmlString (bc.bookData [BookIndex].bookColor,out tmpColor);
//			coverColor.color = tmpColor;
//			print (">>>Book#"+BookIndex+" is set");
//		}
//	}

}
