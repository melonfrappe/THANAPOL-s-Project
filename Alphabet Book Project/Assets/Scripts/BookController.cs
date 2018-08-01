using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;
#region TESTING_ZONE
[System.Serializable]
public class BookDataCatalog {
	public string bookColor;
	public OnBookCover onBookCover;
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
#endregion
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
	
	[SerializeField] string gameDataFileName = "BookCatalog.json";
	[SerializeField] Button backButton;
	[SerializeField] GameObject muteButton;
	[SerializeField] GameObject coverBook;
	[SerializeField] GameObject content;
	[SerializeField] GameObject openingBook;
	[SerializeField] ScrollRect scrollRect;
	[SerializeField] Book book;
	[SerializeField] CloningComponent cloningComponent;
	[SerializeField] GameObject barrier;

	public int CurrentBookIndex,CloningIndexCounter,BookCatalogLength,BookDataCatalogLength;

	Vector2 beforeSnapPos,afterSnapPos;
	Sprite[] coverImage;
	Sprite[] openingBookImage;
	float[] snapPoint;
	float percentage;
	List<Sprite> _bookPage = new List<Sprite>();
	Sprite []tmpSprite;
	string [] url;
	void Start () {
		print (Application.dataPath);
		print (Application.persistentDataPath);
		#region TESTING_ZONE
		//JsonTesting();
		//StartCoroutine(LoadData());
		#endregion
		//JsonHelper ,Solution to convert json file to array object
		GetDataFromJson();

		//Calculate percentage of scroll view per object
		percentage = 1.00f/(BookCatalogLength-1);
		for(int i=0;i<BookCatalogLength;i++){
			snapPoint[i] = percentage*i;
		}

		//Cloning to content
		foreach (Sprite sp in coverImage){
			CloningIndexCounter++;
			GameObject clone = Instantiate(coverBook.gameObject,content.transform);
			clone.GetComponent<Image>().sprite = sp;
			cloningComponent.CloningIndex = CloningIndexCounter;

		}

		//Add listener		
		backButton.onClick.AddListener(()=>{
			_bookPage.Clear();
			muteButton.gameObject.SetActive(false);
			barrier.gameObject.SetActive (false);
			openingBook.gameObject.SetActive(false);
			//content.transform.GetChild(CurrentBookIndex).transform.TweenTranfrom(Siri.Ttype.Scale, Easing.Type.EaseOutBounce, new Vector3 (1.1f, 1.1f, 1), new Vector3 (1f, 1f, 1), 0.75f);
		});

	}
	void Update () {
		SnapToNeasrestPoint();
		TweenInAndOut ();
	}

	void SnapToNeasrestPoint(){
		//Use 0.01f to check nearest because impossible to use absolute zero of delta
		for (int i=0 ;i<BookCatalogLength ;i++)
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
		muteButton.gameObject.SetActive (true);
		openingBook.GetComponent<Image>().sprite = openingBookImage[CurrentBookIndex];
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
		for (int i=0;i < BookCatalogLength ;i++){
			float tmpScale = 1.00f - Mathf.Abs(snapPoint[i] - scrollRect.horizontalNormalizedPosition);
			content.transform.GetChild(i).transform.localScale = new Vector3(tmpScale,tmpScale,1);
		}
	}

	void LoadPageController(){
		for(int i=0;;i++){
			_bookPage.Add (Resources.Load<Sprite>((Alphabet)CurrentBookIndex+"/"+i));
			if (_bookPage [i] == null) {
				_bookPage.Remove (_bookPage[i]);
				break;
			}
		}
		book.bookPages = new Sprite[_bookPage.Count];
		for(int i=0;i<_bookPage.Count;i++){
			book.bookPages [i] = _bookPage [i];
		}
		book.LoadPage ((Alphabet)CurrentBookIndex);
	}

	IEnumerator LoadData(){
		for(int i=0;i<BookDataCatalogLength;i++){
			WWW www = new WWW(url[i]);
			yield return www;
			book.bookPages[i] = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width,www.texture.height), new Vector2(0, 0));
			print ("Book page #"+i+" is loaded");
			if(i==BookDataCatalogLength-1)
				print ("Complete");
		}
	}
	public string GetStreamingPath(){
		#if UNITY_EDITOR
		return Application.streamingAssetsPath;
		#elif UNITY_ANDROID
		return "jar:file://" + Application.dataPath + "!/assets/";
		#endif
	}

	public string GetDataAsJson(string filePath){
		WWW reader = new WWW(filePath);
		while(!reader.isDone){}
		return reader.text;
	}

	void JsonTesting(){
		//Test the new structure of json file
		BookDataCatalog [] bookDataCatalog = JsonHelper.getJsonArray<BookDataCatalog>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath,"BookDataCatalog.json")));
		BookDataCatalogLength = bookDataCatalog[0].bookPage.Length;
		book.bookPages = new Sprite[BookDataCatalogLength];
		url = new string[BookDataCatalogLength];
		for(int i=0;i<BookDataCatalogLength;i++){
			url [i]=bookDataCatalog[0].bookPage[i].bookPageImage;
		}
	}

	void GetDataFromJson(){
		//Set path
		string filePath = Path.Combine(GetStreamingPath(),"BookCatalog.json");
		string dataAsJson = GetDataAsJson (filePath);
		BookCatalog [] bookCatalog = JsonHelper.getJsonArray<BookCatalog>(dataAsJson);

		//Set them size after json file is read
		BookCatalogLength = bookCatalog.Length;
		coverImage = new Sprite[bookCatalog.Length];
		openingBookImage = new Sprite[bookCatalog.Length];
		book.PageBGImage = new Sprite[bookCatalog.Length];
		snapPoint = new float[bookCatalog.Length];

		//Set them values after them size is set
		for(int i=0;i<bookCatalog.Length;i++){
			string coverPath = "AlphabetBook/"+bookCatalog[i].CoverImageName;
			coverImage [i] = Resources.Load<Sprite> (coverPath);
			string openingBookPath = "AlphabetBook/"+bookCatalog[i].OpeningBookImageName;
			openingBookImage [i] = Resources.Load<Sprite> (openingBookPath);
			string pageBGPath = "AlphabetBook/"+bookCatalog[i].PageBGImageName;
			book.PageBGImage [i] = Resources.Load<Sprite> (pageBGPath);

		}
	}
}