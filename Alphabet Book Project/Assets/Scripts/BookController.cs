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
	[SerializeField] ImageDownloader imageDownloader;
	public int CurrentBookIndex,CloningIndexCounter,MaxCloningCoverInScene = 5,BookPageLength;
	Vector2 beforeSnapPos,afterSnapPos;
	float[] snapPoint;
	float percentage;
	string dir;
	List<Sprite> tmpBookPage = new List<Sprite> ();
	void Start () {
		//Close the book when play
		openingBook.gameObject.SetActive(false);
		//Debug
		print (Application.dataPath);
		print (Application.persistentDataPath);

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
			muteButton.gameObject.SetActive(false);
			barrier.gameObject.SetActive (false);
			openingBook.gameObject.SetActive(false);
		});

	}
	void Update () {
		SnapToNeasrestPoint();
		TweenInAndOut ();
		if(imageDownloader.AvailToOpen){
			
			//Open the book
			openingBook.gameObject.SetActive (true);
			//Update before open
			book.UpdateSprites ();
			//Use temporary sprite
			if (imageDownloader.IndexIsLoaded == CurrentBookIndex && imageDownloader.IsFirstDownloading ) {
				UseTmpBookPage ();
			}
			imageDownloader.AvailToOpen = false;
		}

		else if(imageDownloader.Counter == BookPageLength && BookPageLength !=0){
			//Let the book appears
			openingBook.gameObject.SetActive (true);
			//Update before open
			book.UpdateSprites ();
			imageDownloader.Counter = 0;
		}

		if(Input.GetKey(KeyCode.Space)){
			book.UpdateSprites ();
		}
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
		//Update book pages
		book.UpdateSprites ();
		//Initiate to first download of the book is clicked
		GetDataFromJson ();
		//Set mute button to appears
		muteButton.gameObject.SetActive (true);
		//To blur bg
		barrier.gameObject.SetActive (true);
		//Load the book page which had been download in local before the book open
		LoadPageController ();
		//Reset to 1st page before open each book
		book.ResetCurrentPage ();
		//Snap to prev/next book when the book is cliked not be mid. of display
		SnapToPrevNext(CurrentBookIndex);

	}
	//Tween In and Out when the book is scrolling thru mid. of display.
	void TweenInAndOut(){
		for (int i=0;i < MaxCloningCoverInScene ;i++){
			float tmpScale = 1.00f - Mathf.Abs(snapPoint[i] - scrollRect.horizontalNormalizedPosition);
			content.transform.GetChild(i).transform.localScale = new Vector3(tmpScale,tmpScale,1);
		}
	}

	void LoadPageController(){
		
		for(int i=0;i<BookPageLength;i++){
			book.bookPages [i] = Sprite.Create(Resources.Load<Texture2D>(dir+"/"+i),new Rect(0,0,512f,512f),new Vector2(0,0));
		}
	}
		
	public string GetStreamingPath(){
		#if UNITY_EDITOR
		return Application.streamingAssetsPath;
		#elif UNITY_ANDROID
		return "jar:file://" + Application.dataPath + "!/assets/";
		#endif
	}

	//Now is not avail. in Unity 2017.2.0f3 and lower
	public string GetDataAsJson(string filePath){
		WWW reader = new WWW(filePath);
		while(!reader.isDone){}
		return reader.text;
	}
		
	void GetDataFromJson(){
		//Test the new structure of json file
		string filePath = Path.Combine(GetStreamingPath(),gameDataFileName);
		string dataAsJson = ReadJsonToString(filePath);
		//Map json file to object in project
		BookData [] bookData = JsonHelper.getJsonArray<BookData>(dataAsJson);
		BookPageLength = bookData [CurrentBookIndex].pageLength;

		//Set array size
		book.bookPages = new Sprite[BookPageLength];
		string [] url = new string[bookData[CurrentBookIndex].pageLength];
		dir = bookData[CurrentBookIndex].onBookCover.bookTitle;
		//First dir creating
		imageDownloader.CreateDirectory (dir);
		//Pull url from object to image downloader
		for(int i=0;i<bookData[CurrentBookIndex].pageLength;i++){
			url [i]=bookData[CurrentBookIndex].bookPage[i].bookPageImage;
			StartCoroutine(imageDownloader.Loader (url[i],dir,i,LoadingIsCompleted,CurrentBookIndex));
		}
	}

	private void LoadingIsCompleted(int index,Sprite spr)
	{
		book.bookPages [index] = spr;
		tmpBookPage.Add (spr);
	}
	public string ReadJsonToString(string fileName){
		using (StreamReader reader = new StreamReader(Path.Combine(GetStreamingPath(),fileName)))
		{
			return reader.ReadToEnd ();
		}
	}

	void UseTmpBookPage(){
		for(int i=0;i<BookPageLength;i++)
			book.bookPages [i] = tmpBookPage [i];
	}
}