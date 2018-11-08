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
    public string bookColor;
    public OnBookCover onBookCover;
    public int pageLength;
    public BookPage[] bookPage;
	public string textAnchor;
	public string textColor;
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

    public ScrollRect ScrollRect;

    public Book Book;

	public ImageDownloader ImageDownloader;

	public int CatagoryCount,CurrentBookIndex, CloningIndexCounter, BookDataLength, MaxPageLengthEachBook;

	public Sprite[] BookCoverImage;

	public List<int> BookPageLength = new List<int>();

    public BookData[] bookData;

	string[] imageURL;
    string[] curDir;

    float percentage;
    float[] snapPoint;

    Vector2 beforeSnapPos, afterSnapPos;

    void Start()
    {
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
		//Load book cover first
		LoadBookCover();
        //Set book page length size
		SetBookPageLeght();
        //Close the book when play
        OpeningBook.gameObject.SetActive(false);
        //Cloning into content
        CloneFromBookDataLength();
        //Add listener of back button
        BackButton.onClick.AddListener(() =>
	        {
	            Barrier.gameObject.SetActive(false);
	            OpeningBook.gameObject.SetActive(false);
				//Reset to first page when close a book
				Book.ResetCurrentPage();
	        });
        //Add listener of delete button
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

	void Update(){
		if(Input.GetKey(KeyCode.Space)){
			SetBookCover ();
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
	public void Unselected(){
		for(int i=0; i<UnselectedPanel.gameObject.transform.childCount; i++)
			Destroy(UnselectedPanel.gameObject.transform.GetChild(i).gameObject);
		UnselectedPanel.gameObject.SetActive(false);
	}
	void CloneFromBookDataLength(){
		for (int i = 0; i < BookDataLength; i++)
		{
			GameObject clone = Instantiate(MidCoverBook.gameObject, MidBookShelfContent.transform);
			clone.GetComponent<CloningComponent>().BookIndex = CloningIndexCounter;
			CloningIndexCounter++;
		}
	}
    public void OpenBook()
    {
        //Initiate to first download of selected book is clicked
        LoadSelectedBook();
        //To blur bg
        Barrier.gameObject.SetActive(true);
    }
	void SetBookCover(){
		int bookIndex=0;
		var ct = MidBookShelfContent.GetComponentsInChildren<CloningComponent> ();
		foreach(CloningComponent cc in ct){
			cc.coverImage.sprite = BookCoverImage[bookIndex];
			cc.coverImage.color = Color.white;
			//Set color each book
			Color tmpColor = new Color();
			ColorUtility.TryParseHtmlString (bookData [bookIndex].bookColor,out tmpColor);
			cc.coverColor.color = tmpColor;
			bookIndex++;
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
		//Set array size
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
    void SetSnappoint()
    {
        //Calculate percentage of scroll view per object
        snapPoint = new float[BookDataLength];
        percentage = 1.00f / (BookDataLength - 1);
        for (int i = 0; i < BookDataLength; i++)
        {
            snapPoint[i] = percentage * i;
        }
    }

	//Tween In and Out when the book is scrolling thru mid. of display.
	void TweenInAndOut()
	{
		for (int i = 0; i < BookDataLength; i++)
		{
			float tmpScale = 1.00f - Mathf.Abs(snapPoint[i] - ScrollRect.horizontalNormalizedPosition);
			MidBookShelfContent.transform.GetChild(i).transform.localScale = new Vector3(tmpScale, tmpScale, 1);
		}
	}

	void SnapToNearestPoint()
	{
		//Use 0.01f to check nearest because impossible to use absolute zero of delta
		for (int i = 0; i < BookDataLength; i++)
			if (snapPoint[i] - ScrollRect.horizontalNormalizedPosition > 0 && snapPoint[i] - ScrollRect.horizontalNormalizedPosition < 0.01f)
			{
				ScrollRect.horizontalNormalizedPosition = snapPoint[i];
				CurrentBookIndex = i;
			}
	}

	public void SnapToPrevNext(int selectedPoint)
	{
		//Set position before snap
		beforeSnapPos = MidBookShelfContent.transform.localPosition;
		//Snap to selected
		ScrollRect.horizontalNormalizedPosition = snapPoint[selectedPoint];
		//Set position after snap
		afterSnapPos = MidBookShelfContent.transform.localPosition;
		//Animate
		MidBookShelfContent.GetComponent<RectTransform>().TweenRectTrans(Siri.Rtype.LocalPosition, Easing.Type.EaseOutQuad, beforeSnapPos, afterSnapPos, 0.25f);
	}
	#endregion
}
