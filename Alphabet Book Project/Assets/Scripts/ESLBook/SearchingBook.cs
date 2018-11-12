using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SearchingBook : MonoBehaviour {
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
			bc.CurrentBookIndex = SearchingBookIndex;
			PlayButton.gameObject.SetActive(true);
			DescriptionButton.gameObject.SetActive(true);

			var sp = bc.SearchingPanel.HorizontalContent.gameObject.GetComponentsInChildren<SearchingBook>();
			foreach(SearchingBook sb in sp){
				if(sb.SearchingBookIndex!=this.SearchingBookIndex){
					sb.PlayButton.gameObject.SetActive(false);
					sb.DescriptionButton.gameObject.SetActive(false);
				}
			}
		});
		PlayButton.onClick.AddListener (()=>{
			bc.OpenBook();
		});
		DescriptionButton.onClick.AddListener (()=>{
			bc.DescriptionPanel.SetActive(true);
			bc.CoverImageDescription.sprite = bc.BookCoverImage[bc.CurrentBookIndex];
		});
	}

}
