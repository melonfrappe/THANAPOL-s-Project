using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;

public class CloningComponent : MonoBehaviour {

	public int BookIndex;
	public List <Sprite> tmpPage = new List<Sprite>();
	public bool snapToSelectingBook = false;
	public Button coverButton;
	public Image coverImage;

	BookController bc;
	ImageDownloader id;

	void Start(){
		bc = BookController.Instance;
		id = ImageDownloader.Instance;
		//Add button listener
		coverButton.onClick.AddListener (()=>{
			bc.CurrentBookIndex = BookIndex;
			bc.OpenBook();	
		});
	}


	void Update(){
		//Set each book cover
		if(id.Counter == bc.BookDataLength && coverImage.sprite == null){
			coverImage.sprite = bc.BookCoverImage[BookIndex];
		}
		//Set color each book
		Color tmpColor = new Color();
		ColorUtility.TryParseHtmlString (bc.bookData [BookIndex].bookColor,out tmpColor);
		GetComponentInChildren<Image> ().color = tmpColor;
		//Always update
//		bookCover.sprite = bc.BookCoverImage[BookIndex];
//		bookCover.color = Color.white;
//				
	}
}
