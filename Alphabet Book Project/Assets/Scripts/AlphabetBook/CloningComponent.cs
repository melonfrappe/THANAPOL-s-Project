using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace UnityEngine.UI.Extensions
{
	public class CloningComponent : MonoBehaviour {

		public int CloningIndex;
		public List <Sprite> tmpPage = new List<Sprite>();
		public bool snapToSelectingBook = false;
		[SerializeField] Button cloningComponent;
		[SerializeField] BookController bookController;
		[SerializeField] ScrollPositionController spc;
		void Start(){
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
	}
}