using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;
[System.Serializable]
public class BookData {
	public string bookColor;
	public OnBookCover onBookCover;
	public int pageLength;
	public BookPage [] bookPage;
}
[System.Serializable]
public class BookPage{
	public string bookPageImage;
	public string bookContent;
}
[System.Serializable]
public class OnBookCover{
	public string bookCoverImage;
	public string bookTitle;
}
public enum Alphabet{
	A,B,C,D,E,F,G,H,
	I,J,K,L,M,N,O,P,
	Q,R,S,T,U,V,W,X,
	Y,Z
}
public class JsonHelper
{
	public static T[] getJsonArray<T>(string json)
	{
		string newJson = "{ \"array\": " + json + "}";
		Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>> (newJson);
		return wrapper.array;
	}

	[System.Serializable]
	private class Wrapper<T>
	{
		public T[] array;
	}
}
public class BookController : MonoBehaviour {
	
	[SerializeField] string gameDataFileName = "BookData.json";
	[SerializeField] Button backButton;
	[SerializeField] GameObject muteButton;
	[SerializeField] GameObject coverBook;
	[SerializeField] GameObject content;
	[SerializeField] GameObject openingBook;
	[SerializeField] ScrollRect scrollRect;
	[SerializeField] Book book;
	[SerializeField] CloningComponent cloningComponent;
	[SerializeField] GameObject barrier;

	public int CurrentBookIndex,CloningIndexCounter,MaxCloningCoverInScene = 5,BookPageLength;

	ImageDownloader imageDownloader;
	Vector2 beforeSnapPos,afterSnapPos;
	float[] snapPoint;
	float percentage;
	List<Sprite> _bookPage = new List<Sprite>();
	void Start () {
		imageDownloader = new ImageDownloader ();
		//Test WWW class to read json file
		print (Application.dataPath);
		print (Application.persistentDataPath);
		//Initiate to get data from Json

		//Calculate percentage of scroll view per object
		snapPoint = new float[MaxCloningCoverInScene];
		percentage = 1.00f/(MaxCloningCoverInScene-1);
		for(int i=0;i<MaxCloningCoverInScene;i++){
			snapPoint[i] = percentage*i;
		}

		//Cloning to content
		for (int i=0;i<MaxCloningCoverInScene;i++){
			CloningIndexCounter++;
			GameObject clone = Instantiate(coverBook.gameObject,content.transform);
			cloningComponent.CloningIndex = CloningIndexCounter;

		}

		//Add listener		
		backButton.onClick.AddListener(()=>{
			_bookPage.Clear();
			muteButton.gameObject.SetActive(false);
			barrier.gameObject.SetActive (false);
			openingBook.gameObject.SetActive(false);
		});

	}
	void Update () {
		SnapToNeasrestPoint();
		TweenInAndOut ();
	}

	void SnapToNeasrestPoint(){
		//Use 0.01f to check nearest because impossible to use absolute zero of delta
		for (int i=0 ;i<MaxCloningCoverInScene ;i++)
			if(snapPoint[i] - scrollRect.horizontalNormalizedPosition > 0 && snapPoint[i] - scrollRect.horizontalNormalizedPosition < 0.01f){
				scrollRect.horizontalNormalizedPosition = snapPoint[i];
				CurrentBookIndex = i;
			}
	}

	public void SnapToPrevNext(int selectedPoint){
		beforeSnapPos = content.transform.localPosition;
		scrollRect.horizontalNormalizedPosition = snapPoint[selectedPoint];
		afterSnapPos = content.transform.localPosition;
		content.GetComponent<RectTransform> ().TweenRectTrans (Siri.Rtype.LocalPosition,Easing.Type.EaseOutQuad,beforeSnapPos,afterSnapPos,0.25f);
	}

	public void OpenTheBook(){
		GetData ();
		muteButton.gameObject.SetActive (true);
		barrier.gameObject.SetActive (true);
		LoadPageController ();
		book.ResetCurrentPage ();
		openingBook.gameObject.SetActive (true);
		book.UpdateSprites ();
		openingBook.transform.TweenTranfrom (Siri.Ttype.Scale, Easing.Type.EaseOutBounce, new Vector3 (1, 1, 1), new Vector3 (1.1f, 1.1f, 1), 0.75f);
		SnapToPrevNext(CurrentBookIndex);

	}
	//Tween In and Out when the book is scrolling thru focus point of display.
	void TweenInAndOut(){
		for (int i=0;i < MaxCloningCoverInScene ;i++){
			float tmpScale = 1.00f - Mathf.Abs(snapPoint[i] - scrollRect.horizontalNormalizedPosition);
			content.transform.GetChild(i).transform.localScale = new Vector3(tmpScale,tmpScale,1);
		}
	}

	void LoadPageController(){
//		for(int i=0;;i++){
//			_bookPage.Add (Resources.Load<Sprite>((Alphabet)CurrentBookIndex+"/"+i));
//			if (_bookPage [i] == null) {
//				_bookPage.Remove (_bookPage[i]);
//				break;
//			}
//		}
		book.bookPages = new Sprite[BookPageLength];
		for(int i=0;i<BookPageLength;i++){
			book.bookPages [i] = Sprite.Create(Resources.Load<Texture2D>((Alphabet)CurrentBookIndex+"/"+i),new Rect(0,0,512f,384f),new Vector2(0,0));
		}
	}
		
	public string GetStreamingPath(){
		#if UNITY_EDITOR
		return Application.streamingAssetsPath;
		#elif UNITY_ANDROID
		return "jar:file://" + Application.dataPath + "!/assets/";
		#endif
	}
	//Now is not avail because something that I dont know... :(
	public string GetDataAsJson(string filePath){
		WWW reader = new WWW(filePath);
		while(!reader.isDone){}
		return reader.text;
	}

	void GetData(){
		//Test the new structure of json file
		string filePath = Path.Combine(GetStreamingPath(),gameDataFileName);
		string dataAsJson = File.ReadAllText(filePath);
		BookData [] bookData = JsonHelper.getJsonArray<BookData>(dataAsJson);
		BookPageLength = bookData [CurrentBookIndex].pageLength;
		string [] url = new string[bookData[CurrentBookIndex].pageLength];
		string dir = bookData[CurrentBookIndex].onBookCover.bookTitle;
		imageDownloader.CreateDirectory (dir);
		for(int i=0;i<bookData[CurrentBookIndex].pageLength;i++){
			url [i]=bookData[CurrentBookIndex].bookPage[i].bookPageImage;
			StartCoroutine( imageDownloader.Loader (url[i],dir,i));
		}
	}
}