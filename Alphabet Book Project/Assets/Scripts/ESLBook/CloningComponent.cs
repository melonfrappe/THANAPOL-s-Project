﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;

public class CloningComponent : MonoBehaviour {
	public string Catagory;
	public int BookIndex;
	public List <Sprite> tmpPage = new List<Sprite>();
	public bool snapToSelectingBook = false;
	public Button coverButton;
	public Button playButton;
	public Button descriptionButton;
	public Image coverImage;
	public Image coverColor;
	BookController bc;

	void Start(){
		bc = BookController.Instance;
		coverButton.onClick.AddListener (()=>{
			bc.NextBookIndex = (this.transform.GetSiblingIndex()+1)%this.transform.parent.transform.childCount;
			bc.Barrier.SetActive(true);
			var bsc= bc.BookShelfContent.GetComponentsInChildren<UnselectedPanel>();
			foreach(UnselectedPanel up in bsc){
				Image iup = up.GetComponent<Image>();
				iup.raycastTarget = true;
				iup.color = new Color(1,1,1,0.5f);
			}
			bc.CurrentBookIndex = BookIndex;
			SetIsSelected();
		});
		playButton.onClick.AddListener (()=>{
			bc.OpenBook();	
		});
		descriptionButton.onClick.AddListener (()=>{
			bc.DescriptionPanel.SetActive(true);
			bc.CoverImageDescription.sprite = bc.BookCoverImage[bc.CurrentBookIndex];
		});
	}

	public void SetIsSelected(){
		GameObject clone = Instantiate(this.gameObject,bc.UnusedObject.transform,true);
		for(int i=0; i<clone.transform.childCount; i++){
			if(!clone.transform.GetChild (i).gameObject.activeInHierarchy)
				clone.transform.GetChild (i).gameObject.SetActive(true);
		}
		Siri.TweenExtensions.Transform (clone.GetComponent<RectTransform>(), Siri.Rtype.Scale, clone.transform.localScale, new Vector2 (1.2f, 1.2f), 0.5f, 0, Easing.Type.EaseOutBounce);
	}
}