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

    public Book Book;

	public ImageDownloader ImageDownloader;

	public int CatagoryAmount,CurrentBookIndex,CloningIndexCounter,BookDataLength,MaxPageLengthEachBook,NextBookIndex;

	public Sprite[] BookCoverImage;

	public List<int> BookPageLength = new List<int>();

    public BookData[] bookData;

	string[] imageURL;
    string[] curDir;

	void Awake(){
		OpeningBook.gameObject.SetActive(false);
		string filePath = Path.Combine(GetStreamingPath(), GameDataFileName);
		#if UNITY_EDITOR_OSX
		string dataAsJson = ReadJsonToString(filePath);
		#else
		string dataAsJson = GetDataAsJson(filePath);
		#endif
		bookData = JsonHelper.getJsonArray<BookData>(dataAsJson);
		BookDataLength = bookData.Length;
		BookCoverImage = new Sprite[BookDataLength];
		curDir = new string[BookDataLength];

		//Initialization
		CountCatagory();
		SetBookPageLeght();

	}
    void Start()
    {
		//Initialization
		CloneShelf();
        CloneBook();
		LoadBookCover();

		BackButton.onClick.AddListener(BackToCatalogue);
        DeleteButton.onClick.AddListener(() =>
        {
            for (int i = 0; i <= BookPageLength[CurrentBookIndex]; i++)
            {
                if (i < BookPageLength[CurrentBookIndex])
                    ImageDownloader.DeleteFile(curDir[CurrentBookIndex], i);
                else
                    ImageDownloader.RemoveDirectory(curDir[CurrentBookIndex]);
            }
        });
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
			BookPageLength.Add(bookData[i].pageLength);
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
	// void ClearNextBookChild(){
	// 	for(int i=0; i<ResultPanel.NextBook.childCount; i++)
	// 		Destroy(ResultPanel.NextBook.GetChild (i).gameObject);
	// }
	public void BackToCatalogue(){
		// ClearNextBookChild ();
		CancelSelecting ();
		Barrier.SetActive(false);
		OpeningBook.SetActive(false);
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
		CancelSelecting ();
		Book.ResetCurrentPage();
        LoadSelectedBook();
        Barrier.SetActive(true);
		Color tmpColor = new Color();
		ColorUtility.TryParseHtmlString (bookData [(CurrentBookIndex+1)%BookDataLength].bookColor,out tmpColor);
		ResultPanel.NextBookTemplate.color = tmpColor;
		ResultPanel.NextBookCoverImage.sprite = BookCoverImage[(CurrentBookIndex+1)%BookDataLength];

		//Transform nextBook = ResultPanel.NextBook.transform;
		// for(int i=0; i<CatagoryContent.Count; i++){
		// 	//loop find a shelf which current book is in
		// 	if(bookData[CurrentBookIndex].catagory == CatagoryContent[i].GetComponent<MidBookShelf>().Catagory)
		// 		Instantiate (CatagoryContent[i].GetComponent<MidBookShelf>()
		// 			.MidBookShelfContent.GetChild((CurrentBookIndex+1)%BookDataLength).gameObject,nextBook);
		// }
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

		ImageDownloader.CreateDirectory(curDir[CurrentBookIndex]);
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
}
