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

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] array;
    }
}

public class BookController : Singleton<BookController>
{
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

    public int CurrentBookIndex, CloningIndexCounter, BookDataLength, MaxPageLengthEachBook;
    public List<int> BookPageLength = new List<int>();

    public BookData[] bookData;
	string[] imageURL;
    string[] curDir;
    float percentage;
    float[] snapPoint;
    public bool finishToRead = false;
	public Sprite[] BookCoverImage;
    Vector2 beforeSnapPos, afterSnapPos;

    void Start()
    {
		//Get file path
		string filePath = Path.Combine(GetStreamingPath(), gameDataFileName);
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
		//Debug
		print(">>>BookDataLenght : "+BookDataLength);
		//Book cover size
		BookCoverImage = new Sprite[BookDataLength];
        //Set current directory size for store each directory name with a book title
        curDir = new string[BookDataLength];
		//Load book cover first
		imageDownloader.CreateDirectory("_BookCover");
		for(int i=0; i<BookDataLength; i++){
			StartCoroutine(imageDownloader.Loader(bookData[i].onBookCover.bookCoverImage,"_BookCover",i,LoadingBookCover,i));
		}
        //Set book page length size
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
            //Set flag that reading is finished
            if (i == BookDataLength - 1)
            {
                finishToRead = true;
            }

        }

        //Close the book when play
        openingBook.gameObject.SetActive(false);
        //Set snappoint
        //SetSnappoint();
        //Cloning into content
        CloneFromBookDataLength();
        //Add listener of back button
        backButton.onClick.AddListener(() =>
        {
            muteButton.gameObject.SetActive(false);
            barrier.gameObject.SetActive(false);
            openingBook.gameObject.SetActive(false);
        });
        //Add listener of delete button
        deleteButton.onClick.AddListener(() =>
        {
            for (int i = 0; i <= BookPageLength[CurrentBookIndex]; i++)
            {
                if (i < BookPageLength[CurrentBookIndex])
                    imageDownloader.DeleteFile(curDir[CurrentBookIndex], i);
                //Need to wait for the folder is empty
                else
                    imageDownloader.RemoveDirectory(curDir[CurrentBookIndex]);
            }
        });

        //Debuging Zone
        print(dataAsJson);
        print("Max page length is : " + MaxPageLengthEachBook);
        print("dataPath : " + Application.dataPath);
        print("persistentPath : " + Application.persistentDataPath);
    }
    void Update()
    {
        //Wait for reading is finished
        //if (finishToRead)
        //SnapToNearestPoint();
        //A little animate when snap to prev or next
        //TweenInAndOut();
        //Use to open the selected book when it was clicked
        if (CurrentBookIndex >=0 && imageDownloader.Counter == BookPageLength[CurrentBookIndex] && BookPageLength[CurrentBookIndex] != 0)
        {
            //Let the book appears
            openingBook.gameObject.SetActive(true);
            //Update before open
            book.UpdateSprites();
            //Reset counter
            imageDownloader.Counter = 0;
        }
    }

    public void OpenBook()
    {
        //Initiate to first download of selected book is clicked
        LoadSelectedBook();
        //Set mute button to appears
        muteButton.gameObject.SetActive(true);
        //To blur bg
        barrier.gameObject.SetActive(true);
        //Reset to 1st page before open each book
        book.ResetCurrentPage();

    }

    public string GetStreamingPath()
    {
	#if UNITY_EDITOR
	        return Application.streamingAssetsPath;
	#elif UNITY_ANDROID
			return "jar:file://" + Application.dataPath + "!/assets/";
	#endif
    }

	#region FILE_READER_METHODS

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
	void LoadingBookCover(int index,Sprite spr){
		BookCoverImage [index] = spr;
	}
    void FinishToLoad(int index, Sprite spr)
    {
        book.bookPages[index] = spr;
    }

    void LoadSelectedBook()
    {
		imageDownloader.Counter = 0;
        //Set array size
        book.bookPages = new Sprite[BookPageLength[CurrentBookIndex]];
		book.pageText = new string[BookPageLength[CurrentBookIndex]];
        imageURL = new string[bookData[CurrentBookIndex].pageLength];
        curDir[CurrentBookIndex] = bookData[CurrentBookIndex].onBookCover.bookTitle;
        //First dir creating
        imageDownloader.CreateDirectory(curDir[CurrentBookIndex]);
        //Pull imageURL from object to image downloader
        for (int i = 0; i < bookData[CurrentBookIndex].pageLength; i++)
        {
			book.pageText [i] = bookData [CurrentBookIndex].bookPage [i].bookContent;
            imageURL[i] = bookData[CurrentBookIndex].bookPage[i].bookPageImage;
            StartCoroutine(imageDownloader.Loader(imageURL[i], curDir[CurrentBookIndex], i, FinishToLoad, CurrentBookIndex));
        }
		//Text alignment
		book.bookText.alignment = (TextAnchor)System.Enum.Parse (typeof(TextAnchor), bookData [CurrentBookIndex].textAnchor);
		book.bookTextL.alignment = (TextAnchor)System.Enum.Parse (typeof(TextAnchor), bookData [CurrentBookIndex].textAnchor);
		//Change text color follow the json
		Color tmpColor = new Color();
		ColorUtility.TryParseHtmlString (bookData [CurrentBookIndex]. textColor,out tmpColor);
		book.bookText.color = tmpColor;
		book.bookTextL.color = tmpColor;

    }

	#region OLD_METHODS
	//==========//==========//==========//==========//==========//==========//==========
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

	void CloneFromBookDataLength(){
	for (int i = 0; i < BookDataLength; i++)
		{
			GameObject clone = Instantiate(coverBook.gameObject, content.transform);
			clone.GetComponent<CloningComponent>().BookIndex = CloningIndexCounter;
			CloningIndexCounter++;
		}
	}

	//Tween In and Out when the book is scrolling thru mid. of display.
	void TweenInAndOut()
	{
		for (int i = 0; i < BookDataLength; i++)
		{
			float tmpScale = 1.00f - Mathf.Abs(snapPoint[i] - scrollRect.horizontalNormalizedPosition);
			content.transform.GetChild(i).transform.localScale = new Vector3(tmpScale, tmpScale, 1);
		}
	}

	void SnapToNearestPoint()
	{
		//Use 0.01f to check nearest because impossible to use absolute zero of delta
		for (int i = 0; i < BookDataLength; i++)
			if (snapPoint[i] - scrollRect.horizontalNormalizedPosition > 0 && snapPoint[i] - scrollRect.horizontalNormalizedPosition < 0.01f)
			{
				scrollRect.horizontalNormalizedPosition = snapPoint[i];
				CurrentBookIndex = i;
			}
	}

	public void SnapToPrevNext(int selectedPoint)
	{
		//Set position before snap
		beforeSnapPos = content.transform.localPosition;
		//Snap to selected
		scrollRect.horizontalNormalizedPosition = snapPoint[selectedPoint];
		//Set position after snap
		afterSnapPos = content.transform.localPosition;
		//Animate
		content.GetComponent<RectTransform>().TweenRectTrans(Siri.Rtype.LocalPosition, Easing.Type.EaseOutQuad, beforeSnapPos, afterSnapPos, 0.25f);
	}
	//==========//==========//==========//==========//==========//==========//==========
	#endregion
}
