using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;
using System;

[Serializable]
public class BookData
{
	public string catagory;
    public string bookColor;
    public int pageLength;
    public BookPage[] bookPage;
	public OnBookCover onBookCover;
}
[Serializable]
public class BookPage
{
    public string bookPageImage;
    public string bookContent;
}
[Serializable]
public class OnBookCover
{
    public string bookCoverImage;
    public string bookTitle;
}

public class JsonHelper
{
    public static T[] getJsonArray<T>(string json)
    {
        string newJson = "{ \"array\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.array;
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] array;
    }
}

public class BookController : Singleton<BookController>
{
    public string GameDataFileName = "BookData.json";
	public List<string> CatagoryName = new List<string> ();

    public Button BackButton;
    public Button DeleteButton;
	public Button UnselectedPanel;

	public GameObject MidCoverBook;
	public GameObject BookShelfContent;
	public GameObject MidBookShelfContent;
    public GameObject OpeningBook;
	public GameObject Barrier;
	public GameObject TopBookShelf;
	public GameObject MidBookShelf;
	public GameObject DescriptionPanel;
	public GameObject UnusedObject;
	public List<GameObject> CatagoryContent = new List<GameObject> ();

	public ResultPanel ResultPanel;

	public Image CoverImageDescription;

//    public ScrollRect ScrollRect;

    public Book Book;

	public ImageDownloader ImageDownloader;

	public int CatagoryAmount,CurrentBookIndex,CloningIndexCounter,BookDataLength,MaxPageLengthEachBook,NextBookIndex;

	public Sprite[] BookCoverImage;

	public List<int> BookPageLength = new List<int>();

    public BookData[] bookData;

	string[] imageURL;
    string[] curDir;

//    float percentage;
//    float[] snapPoint;
//
//    Vector2 beforeSnapPos, afterSnapPos;
	void Awake(){
		//Close the book when play
		OpeningBook.gameObject.SetActive(false);
		//TODO: check catagory from json data soon
		//Get file path
		string filePath = Path.Combine(GetStreamingPath(), GameDataFileName);
		//Finding the best way to use one-for-all platform
		#if UNITY_EDITOR_OSX
		string dataAsJson = ReadJsonToString(filePath);
		#else
		string dataAsJson = GetDataAsJson(filePath);
		#endif
		//Map json file to object in project
		bookData = JsonHelper.getJsonArray<BookData>(dataAsJson);
		//Set book data length
		BookDataLength = bookData.Length;
		//Book cover size
		BookCoverImage = new Sprite[BookDataLength];
		//Set current directory size for store each directory name with a book title
		curDir = new string[BookDataLength];
		//Count catagory
		CountCatagory();
		//Set book page length size
		SetBookPageLeght();

	}
    void Start()
    {
		//Initialization
		CloneShelf();
        CloneBook();
		LoadBookCover();

        //Add listener
		BackButton.onClick.AddListener(BackToCatalogue);
        DeleteButton.onClick.AddListener(() =>
        {
            for (int i = 0; i <= BookPageLength[CurrentBookIndex]; i++)
            {
                if (i < BookPageLength[CurrentBookIndex])
                    ImageDownloader.DeleteFile(curDir[CurrentBookIndex], i);
                //Need to wait for the folder is empty
                else
                    ImageDownloader.RemoveDirectory(curDir[CurrentBookIndex]);
            }
        });
//		print (dataAsJson);
//		print ("Max page length is : " + MaxPageLengthEachBook);
//		print ("dataPath : " + Application.dataPath);
//		print ("persistentPath : " + Application.persistentDataPath);
    }

	void CloneShelf(){
		int tmpChildCount = BookShelfContent.transform.childCount;
		for(int i=0; i<=CatagoryAmount- tmpChildCount; i++){
			Instantiate (MidBookShelf,BookShelfContent.transform);
		}
		for(int i=1; i<BookShelfContent.transform.childCount; i++){
			BookShelfContent.transform.GetChild (i).name = CatagoryName[i-1]+"Shelf";
			CatagoryContent.Add (BookShelfContent.transform.GetChild(i).gameObject);
		}
	}
	void LoadBookCover(){
		ImageDownloader.CreateDirectory("_BookCover");
		for(int i=0; i<BookDataLength; i++){
			StartCoroutine(ImageDownloader.Loader(bookData[i].onBookCover.bookCoverImage,"_BookCover",i,BookCoverLoadingCallback,i));
		}
	}
	void SetBookPageLeght(){
		for (int i=0; i < BookDataLength; i++)
		{
			//Set each book page length
			BookPageLength.Add(bookData[i].pageLength);
			//Get max page length
			if (i==0)
			{
				MaxPageLengthEachBook = bookData[0].pageLength;
			}
			else if (bookData[i].pageLength > bookData[i - 1].pageLength)
			{
				MaxPageLengthEachBook = bookData[i].pageLength;
			}
		}
	}
	void CountCatagory(){
		string tmpCatagory = "";
		for(int i=0; i<BookDataLength; i++){
			if (tmpCatagory != bookData [i].catagory) {
				tmpCatagory = bookData [i].catagory;
				CatagoryName.Add (tmpCatagory);
				CatagoryAmount++;
				print ("Catagory #"+CatagoryAmount+" ("+CatagoryName[CatagoryAmount-1]+")");
			}
		}
	}
	void ClearNextBookChild(){
		for(int i=0; i<ResultPanel.NextBook.childCount; i++)
			Destroy(ResultPanel.NextBook.GetChild (i).gameObject);
	}
	public void BackToCatalogue(){
		ClearNextBookChild ();
		CancelSelecting ();
		Barrier.SetActive(false);
		OpeningBook.SetActive(false);
		//Reset to first page when close a book
		Book.ResetCurrentPage();
	}


	public void CancelSelecting(){
		for(int i=0; i<UnusedObject.gameObject.transform.childCount; i++)
			Destroy(UnusedObject.gameObject.transform.GetChild(i).gameObject);
		var bsc= BookShelfContent.GetComponentsInChildren<UnselectedPanel>();
			foreach(UnselectedPanel up in bsc){
				Image iup = up.GetComponent<Image>();
				iup.raycastTarget = false;
				iup.color = new Color(1,1,1,0);
			}
	}
	void CloneBook(){
		for (int i = 0; i < BookDataLength; i++)
		{
			GameObject clone = Instantiate(MidCoverBook.gameObject, MidBookShelfContent.transform);
			CloningComponent cc = clone.GetComponent<CloningComponent> ();
			cc.BookIndex = CloningIndexCounter;
			cc.Catagory = bookData[i].catagory;
			CloningIndexCounter++;
		}
	}
    public void OpenBook()
    {
		//reset selected when open a book
		CancelSelecting ();
		//reset book page
		Book.ResetCurrentPage();
        //Initiate to first download of selected book is clicked
        LoadSelectedBook();
        //To blur bg
        Barrier.SetActive(true);
		//clone next book on result panel
		Transform nextBook = ResultPanel.NextBook.transform;
		//TODO:Correct a next book must select in each catagory
		for(int i=0; i<CatagoryContent.Count; i++){
			//loop find a shelf which current book is in
			if(bookData[CurrentBookIndex].catagory == CatagoryContent[i].GetComponent<MidBookShelf>().Catagory)
				Instantiate (CatagoryContent[i].GetComponent<MidBookShelf>()
					.MidBookShelfContent.GetChild(NextBookIndex).gameObject,nextBook);
		}

    }
	void SetBookCover(){
		int bookIndex=0;
		var _midBookShelfContent = MidBookShelfContent.GetComponentsInChildren<CloningComponent> ();
		foreach(CloningComponent cc in _midBookShelfContent){
			cc.coverImage.sprite = BookCoverImage[bookIndex];
			cc.coverImage.color = Color.white;
			Color tmpColor = new Color();
			ColorUtility.TryParseHtmlString (bookData [bookIndex].bookColor,out tmpColor);
			cc.coverColor.color = tmpColor;
			bookIndex++;
			SetBookToThemShelf (cc);
		}
	}
	void SetBookToThemShelf(CloningComponent cc){
		for(int i=0; i<CatagoryContent.Count; i++){
			MidBookShelf _midBookShelf = CatagoryContent[i].GetComponent<MidBookShelf>();
			if(cc.Catagory == _midBookShelf.Catagory){
				cc.transform.SetParent (_midBookShelf.MidBookShelfContent);
			}
		}
	}
	void BookCoverLoadingCallback(int index,Sprite spr){
		BookCoverImage[index] = spr;
		if(ImageDownloader.Counter == BookDataLength){
			StartCoroutine (WaitForCallback (0, () => {
				SetBookCover ();
			}));
		}
	}
	void BookPageLoadingCallback(int index, Sprite spr)
	{
		Book.bookPages[index] = spr;
		if (ImageDownloader.Counter == BookPageLength [CurrentBookIndex]) {
			OpeningBook.gameObject.SetActive (true);
			//Update before open
			Book.UpdateSprites();
		}
	}
	void LoadSelectedBook()
	{
		ImageDownloader.Counter = 0;
		Book.bookPages = new Sprite[BookPageLength[CurrentBookIndex]];
		Book.pageText = new string[BookPageLength[CurrentBookIndex]];
		imageURL = new string[bookData[CurrentBookIndex].pageLength];
		curDir[CurrentBookIndex] = bookData[CurrentBookIndex].onBookCover.bookTitle;
		//First dir creating
		ImageDownloader.CreateDirectory(curDir[CurrentBookIndex]);
		//Pull imageURL from object to image downloader
		for (int i = 0; i < bookData[CurrentBookIndex].pageLength; i++)
		{
			Book.pageText [i] = bookData [CurrentBookIndex].bookPage [i].bookContent;
			imageURL[i] = bookData[CurrentBookIndex].bookPage[i].bookPageImage;
			StartCoroutine(ImageDownloader.Loader(imageURL[i], curDir[CurrentBookIndex], i, BookPageLoadingCallback, CurrentBookIndex));
		}
	}
	public IEnumerator WaitForCallback(float seconds,Action callback){
		yield return new WaitForSeconds (seconds);
		callback ();
	}
	#region DataPathMethods
	public string GetStreamingPath()
	{
		#if UNITY_EDITOR
		return Application.streamingAssetsPath;
		#elif UNITY_ANDROID
		return "jar:file://" + Application.dataPath + "!/assets/";
		#endif
	}
    //Work on all platfrom except OSX
    public string GetDataAsJson(string filePath)
    {
        WWW reader = new WWW(filePath);
        while (!reader.isDone) {
		}
		return reader.text;
    }
    //Use for OSX
    public string ReadJsonToString(string filePath)
    {
        using (StreamReader reader = new StreamReader(filePath))
        {
            return reader.ReadToEnd();
        }
    }
	#endregion

	#region SnappingMethods
//    void SetSnappoint()
//    {
//        //Calculate percentage of scroll view per object
//        snapPoint = new float[BookDataLength];
//        percentage = 1.00f / (BookDataLength - 1);
//        for (int i = 0; i < BookDataLength; i++)
//        {
//            snapPoint[i] = percentage * i;
//        }
//    }
//
//	//Tween In and Out when the book is scrolling thru mid. of display.
//	void TweenInAndOut()
//	{
//		for (int i = 0; i < BookDataLength; i++)
//		{
//			float tmpScale = 1.00f - Mathf.Abs(snapPoint[i] - ScrollRect.horizontalNormalizedPosition);
//			MidBookShelfContent.transform.GetChild(i).transform.localScale = new Vector3(tmpScale, tmpScale, 1);
//		}
//	}
//
//	void SnapToNearestPoint()
//	{
//		//Use 0.01f to check nearest because impossible to use absolute zero of delta
//		for (int i = 0; i < BookDataLength; i++)
//			if (snapPoint[i] - ScrollRect.horizontalNormalizedPosition > 0 && snapPoint[i] - ScrollRect.horizontalNormalizedPosition < 0.01f)
//			{
//				ScrollRect.horizontalNormalizedPosition = snapPoint[i];
//				CurrentBookIndex = i;
//			}
//	}
//
//	public void SnapToPrevNext(int selectedPoint)
//	{
//		//Set position before snap
//		beforeSnapPos = MidBookShelfContent.transform.localPosition;
//		//Snap to selected
//		ScrollRect.horizontalNormalizedPosition = snapPoint[selectedPoint];
//		//Set position after snap
//		afterSnapPos = MidBookShelfContent.transform.localPosition;
//		//Animate
//		MidBookShelfContent.GetComponent<RectTransform>().TweenRectTrans(Siri.Rtype.LocalPosition, Easing.Type.EaseOutQuad, beforeSnapPos, afterSnapPos, 0.25f);
//	}
	#endregion
}
