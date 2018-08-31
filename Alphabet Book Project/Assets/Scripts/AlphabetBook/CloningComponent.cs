using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
namespace UnityEngine.UI.Extensions
{
	public class CloningComponent : MonoBehaviour {

		public int CloningIndex;
		public List <Sprite> tmpPage = new List<Sprite>();
		public bool snapToSelectingBook = false;
		[SerializeField] Button cloningComponent;
		[SerializeField] BookController bookController;
		[SerializeField] ScrollPositionController spc;
		[SerializeField] Image bookCover;
		[SerializeField] ImageDownloader imageDownloader;

		void Start(){
			//Add button listener
			cloningComponent.onClick.AddListener (()=>{
				if(bookController.CurrentBookIndex == bookController.BookDataLength-1 && this.CloningIndex == 0){
					spc.SnapToNext();
				}
				else if(bookController.CurrentBookIndex == 0 && this.CloningIndex == bookController.BookDataLength-1){
					spc.SnapToPrev();
				}
				else if(bookController.CurrentBookIndex < this.CloningIndex){
					print(this.CloningIndex);
					spc.SnapToNext();
				}
				else if(bookController.CurrentBookIndex > this.CloningIndex){
					print(this.CloningIndex);
					spc.SnapToPrev();
				}
				else{
					bookController.OpenBook();	
				}
			});
		}

		void LoadBookCover(){
			byte[] bytes = File.ReadAllBytes(imageDownloader.GetResourcesPath()+bookController.bookData[this.CloningIndex].onBookCover.bookTitle+"/"+"0.png");
			Texture2D texture = new Texture2D(8,8);
			texture.LoadImage(bytes);
			Sprite spr = texture.ToSprite();
			this.bookCover.sprite = spr;
		}

		void Update(){
			//Set color each book
			Color tmpColor = new Color();
			ColorUtility.TryParseHtmlString (bookController.bookData [this.CloningIndex].bookColor,out tmpColor);
			this.GetComponent<Image> ().color = tmpColor;

			//Set each book cover
			if(imageDownloader.Counter == bookController.BookDataLength && this.bookCover.sprite == null){
				this.bookCover.sprite = bookController.BookCoverImage[this.CloningIndex];
				this.bookCover.color = new Color (1,1,1,1);

				//Debug
				print("Set book cover#"+this.CloningIndex);
			}
			//Always update
			this.bookCover.sprite = bookController.BookCoverImage[this.CloningIndex];
			this.bookCover.color = new Color (1,1,1,1);
				
		}
	}
}