using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SearchingBook : MonoBehaviour ,IPointerEnterHandler,IPointerExitHandler{
	[SerializeField] Button BookCoverButton;
	[SerializeField] Button PlayButton;
	[SerializeField] Button DescriptionButton;
	public Image SearchingCoverImage;
	public Image SearchingCoverBook;
	public int SearchingBookIndex;
	BookController bc;
	void Start(){
		bc = BookController.Instance;
		BookCoverButton.onClick.AddListener (()=>{
//			bc.CurrentBookIndex = SearchingBookIndex;
//			PlayButton.gameObject.SetActive(true);
//			DescriptionButton.gameObject.SetActive(true);
		});
		PlayButton.onClick.AddListener (()=>{
			bc.OpenBook();
		});
		DescriptionButton.onClick.AddListener (()=>{
			bc.DescriptionPanel.SetActive(true);
			bc.CoverImageDescription.sprite = bc.BookCoverImage[bc.CurrentBookIndex];
		});
	}

	public void OnPointerEnter(PointerEventData pointerEventData)
	{
		bc.CurrentBookIndex = SearchingBookIndex;
		PlayButton.gameObject.SetActive (true);
		DescriptionButton.gameObject.SetActive(true);
	}

	public void OnPointerExit(PointerEventData pointerEventData)
	{
		PlayButton.gameObject.SetActive (false);
		DescriptionButton.gameObject.SetActive(false);
	}

}
