﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class CloningComponent : MonoBehaviour {

	public int CloningIndex;
	public List <Sprite> tmpPage = new List<Sprite>();
	[SerializeField] Button cloningComponent;
	[SerializeField] BookController bookController;
	void Start(){
		cloningComponent.onClick.AddListener (()=>{
			if (bookController.CurrentBookIndex != this.CloningIndex){
				bookController.SnapToPrevNext(this.CloningIndex);
				bookController.CurrentBookIndex = this.CloningIndex;
			}
			else {
				bookController.OpenBook();
			}
		});
	}
}