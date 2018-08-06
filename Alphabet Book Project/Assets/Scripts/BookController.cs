using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;
using UnityEditor;

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
	[SerializeField] Button deleteButton;
	[SerializeField] GameObject muteButton;
	[SerializeField] GameObject coverBook;
	[SerializeField] GameObject content;
	[SerializeField] GameObject openingBook;
	[SerializeField] ScrollRect scrollRect;
	[SerializeField] Book book;
	[SerializeField] CloningComponent cloningComponent;
	[SerializeField] GameObject barrier;
	[SerializeField] ImageDownloader imageDownloader;
	public int CurrentBookIndex,CloningIndexCounter,CloningInScene = 5,MaxPageLengthEachBook = 10;
	public int [] BookPageLength;
	Vector2 beforeSnapPos,afterSnapPos;
	float[] snapPoint;
	float percentage;
	string [] curDir;
	public BookData [] bookData;
	void Start () {
		curDir = new string[CloningInScene];
		BookPageLength = new int[MaxPageLengthEachBook];
		//Test the new structure of json file
		string filePath = Path.Combine(GetStreamingPath(),gameDataFileName);
		string dataAsJson = ReadJsonToString(filePath);
		print(dataAsJson);
		//Map json file to object in project
		bookData = JsonHelper.getJsonArray<BookData>(dataAsJson);
		//Set each book page length
		for(int i=0;i<bookData.Length;i++)
			BookPageLength[i] = bookData [i].pageLength;
		//Close the book when play
		openingBook.gameObject.SetActive(false);
		//Debug
		print (Application.dataPath);
		print (Application.persistentDataPath);

		//Calculate percentage of scroll view per object
		snapPoint = new float[CloningInScene];
		percentage = 1.00f/(CloningInScene-1);
		for(int i=0;i<CloningInScene;i++){
			snapPoint[i] = percentage*i;
		}

		//Cloning to content
		for (int i=0;i<CloningInScene;i++){
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

		deleteButton.onClick.AddListener(()=>{
			for(int i=0;i<BookPageLength[CurrentBookIndex];i++){
				imageDownloader.DeleteFile(curDir[CurrentBookIndex],i);
			}
			imageDownloader.RemoveDirectory(curDir[CurrentBookIndex]);
			
		});

	}
	void Update () {
		SnapToNeasrestPoint();
		TweenInAndOut ();
		if(imageDownloader.Counter == BookPageLength[CurrentBookIndex] && BookPageLength[CurrentBookIndex] !=0){
			//Let the book appears
			openingBook.gameObject.SetActive (true);
			//Update before open
			book.UpdateSprites ();
			imageDownloader.Counter = 0;
		}

		if(Input.GetKey(KeyCode.Space)){
			AssetDatabase.Refresh ();
		}
	}

	void SnapToNeasrestPoint(){
		//Use 0.01f to check nearest because impossible to use absolute zero of delta
		for (int i=0 ;i<CloningInScene ;i++)
			if(snapPoint[i] - scrollRect.horizontalNormalizedPosition > 0 && snapPoint[i] - scrollRect.horizontalNormalizedPosition < 0.01f){
				scrollRect.horizontalNormalizedPosition = snapPoint[i];
				CurrentBookIndex = i;
			}
	}

	public void SnapToPrevNext(int selectedPoint){
		//Set position before snap
		beforeSnapPos = content.transform.localPosition;
		//Snap to selected
		scrollRect.horizontalNormalizedPosition = snapPoint[selectedPoint];
		//Set position after snap
		afterSnapPos = content.transform.localPosition;
		//Animate
		content.GetComponent<RectTransform> ().TweenRectTrans (Siri.Rtype.LocalPosition,Easing.Type.EaseOutQuad,beforeSnapPos,afterSnapPos,0.25f);
	}

	public void OpenBook(){
		//Initiate to first download of selected book is clicked
		LoadSelectedBook ();
		//Set mute button to appears
		muteButton.gameObject.SetActive (true);
		//To blur bg
		barrier.gameObject.SetActive (true);
		//Reset to 1st page before open each book
		book.ResetCurrentPage ();
		//Snap to prev/next book when the book is cliked not be mid. of display
		SnapToPrevNext(CurrentBookIndex);

	}
	//Tween In and Out when the book is scrolling thru mid. of display.
	void TweenInAndOut(){
		for (int i=0;i < CloningInScene ;i++){
			float tmpScale = 1.00f - Mathf.Abs(snapPoint[i] - scrollRect.horizontalNormalizedPosition);
			content.transform.GetChild(i).transform.localScale = new Vector3(tmpScale,tmpScale,1);
		}
	}
		
	public string GetStreamingPath(){
		#if UNITY_EDITOR
		return Application.streamingAssetsPath;
		#elif UNITY_ANDROID
		return "jar:file://" + Application.dataPath + "!/assets/";
		#endif
	}

	//Now can't work in Unity 2017.2.0f3 and lower
	public string GetDataAsJson(string filePath){
		WWW reader = new WWW(filePath);
		while(!reader.isDone){}
		return reader.text;
	}

	
	public string ReadJsonToString(string fileName){
		using (StreamReader reader = new StreamReader(Path.Combine(GetStreamingPath(),fileName)))
		{
			return reader.ReadToEnd ();
		}
	}

//	void UseTmpBookPage(){
//		for(int i=0;i<BookPageLength[CurrentBookIndex];i++)
//			book.bookPages [i] = content.transform.GetChild(CurrentBookIndex).GetComponent<CloningComponent>().tmpPage[i];
//	}
	
	void LoadingIsCompleted(int index,Sprite spr)
		{
			book.bookPages [index] = spr;
			content.transform.GetChild(CurrentBookIndex).GetComponent<CloningComponent>().tmpPage.Add(spr);
		}
	
	void LoadSelectedBook(){
		//Set array size
		book.bookPages = new Sprite[BookPageLength[CurrentBookIndex]];
		string [] url = new string[bookData[CurrentBookIndex].pageLength];
		curDir[CurrentBookIndex] = bookData[CurrentBookIndex].onBookCover.bookTitle;
		//First dir creating
		imageDownloader.CreateDirectory (curDir[CurrentBookIndex]);
		//Pull url from object to image downloader
		for(int i=0;i<bookData[CurrentBookIndex].pageLength;i++){
			url [i]=bookData[CurrentBookIndex].bookPage[i].bookPageImage;
			StartCoroutine(imageDownloader.Loader (url[i],curDir[CurrentBookIndex],i,LoadingIsCompleted,CurrentBookIndex));
		}
	}

	//Optional : Get 5 books when app. is opened
	void OptionalLoading(int bookCount){

	}
}