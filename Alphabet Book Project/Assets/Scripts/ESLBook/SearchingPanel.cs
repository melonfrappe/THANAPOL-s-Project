using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SearchingPanel : MonoBehaviour {

	public GameObject SearchingBook;
	public Transform HorizontalContent;
	public InputField SearchingInput;
	bool cloned;
	BookController bc;
	string tmpInputFieldText;
	void Start () {
		bc = BookController.Instance;
	}

	void Update(){
		if(this.gameObject.activeInHierarchy && !System.String.IsNullOrEmpty(SearchingInput.text) && !cloned){
			for(int i=0; i<bc.BookDataLength; i++){
				if (bc.bookData [i].onBookCover.bookTitle.Contains(SearchingInput.text)) {
					tmpInputFieldText = SearchingInput.text;
					GameObject _clone = Instantiate (SearchingBook,HorizontalContent);
					_clone.GetComponent<SearchingBook>().SearchingCoverImage.sprite = bc.BookCoverImage [i];
					Color tmpColor = new Color();
					ColorUtility.TryParseHtmlString (bc.bookData [i].bookColor,out tmpColor);
					_clone.GetComponent<SearchingBook>().SearchingCoverBook.color = tmpColor;
					_clone.GetComponent<SearchingBook> ().SearchingBookIndex = i;
					cloned = true;
				}
			}
		}

		if(!System.String.IsNullOrEmpty(tmpInputFieldText) && tmpInputFieldText != SearchingInput.text){
			for(int i=0; i<HorizontalContent.childCount; i++){
				Destroy(HorizontalContent.GetChild (i).gameObject);
			}
			cloned = false;
		}
	}
}
