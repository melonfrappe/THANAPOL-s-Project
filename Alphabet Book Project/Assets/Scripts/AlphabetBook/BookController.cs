using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;
using UnityEditor;
namespace UnityEngine.UI.Extensions
{
	[System.Serializable]
	public class BookData
	{
	    public string bookColor;
	    public OnBookCover onBookCover;
	    public int pageLength;
	    public BookPage[] bookPage;
	}
	[System.Serializable]
	public class BookPage
	{
	    public string bookPageImage;
	    public string bookContent;
	}
	[System.Serializable]
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

	public class BookController : MonoBehaviour
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

	    string[] curDir;
	    float percentage;
	    float[] snapPoint;
	    public bool finishToRead = false;

	    Vector2 beforeSnapPos, afterSnapPos;

	    void Start()
	    {
	        //Get file path
	        string filePath = Path.Combine(GetStreamingPath(), gameDataFileName);
	        //Finding the best way to use one-for-all platform
	        string dataAsJson = ReadJsonToString(filePath);
	        //Map json file to object in project
	        bookData = JsonHelper.getJsonArray<BookData>(dataAsJson);
	        //Set book data length
	        BookDataLength = bookData.Length;
	        //Set current directory size for store each directory name with a book title
	        curDir = new string[BookDataLength];
	        //Set book page length size

	        for (int i = 0; i < BookDataLength; i++)
	        {
	            //Set each book page length
	            BookPageLength.Add(bookData[i].pageLength);
	            //Get max page length
	            if (i == 0)
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
	//        SetSnappoint();
	        //Cloning into content
	//        CloneFromBookDataLength();
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
	//        if (finishToRead)
	//            SnapToNearestPoint();
	        //A little animate when snap to prev or next
	//        TweenInAndOut();
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
	        //Snap to prev/next book when the book is cliked not be mid. of display
	//        SnapToPrevNext(CurrentBookIndex);

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

	    public string GetStreamingPath()
	    {
		#if UNITY_EDITOR
		        return Application.streamingAssetsPath;
		#elif UNITY_ANDROID
				return "jar:file://" + Application.dataPath + "!/assets/";
		#endif
	    }

	    //Now can't work in Unity 2017.2.0f3 and lower
	    public string GetDataAsJson(string filePath)
	    {
	        WWW reader = new WWW(filePath);
	        while (!reader.isDone) { }
	        return reader.text;
	    }
	    //Still not work in this verison...
	    public string ReadJsonToString(string fileName)
	    {
	        using (StreamReader reader = new StreamReader(Path.Combine(GetStreamingPath(), fileName)))
	        {
	            return reader.ReadToEnd();
	        }
	    }

	    void LoadingIsCompleted(int index, Sprite spr)
	    {
	        book.bookPages[index] = spr;
	//        content.transform.GetChild(CurrentBookIndex).GetComponent<CloningComponent>().tmpPage.Add(spr);
	    }

	    void LoadSelectedBook()
	    {
	        //Set array size
	        book.bookPages = new Sprite[BookPageLength[CurrentBookIndex]];
	        string[] url = new string[bookData[CurrentBookIndex].pageLength];
	        curDir[CurrentBookIndex] = bookData[CurrentBookIndex].onBookCover.bookTitle;
	        //First dir creating
	        imageDownloader.CreateDirectory(curDir[CurrentBookIndex]);
	        //Pull url from object to image downloader
	        for (int i = 0; i < bookData[CurrentBookIndex].pageLength; i++)
	        {
	            url[i] = bookData[CurrentBookIndex].bookPage[i].bookPageImage;
	            StartCoroutine(imageDownloader.Loader(url[i], curDir[CurrentBookIndex], i, LoadingIsCompleted, CurrentBookIndex));
	        }
	    }

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
				Clone();
			}
		}

		void Clone(){
			CloningIndexCounter++;
			GameObject clone = Instantiate(coverBook.gameObject, content.transform);
			cloningComponent.CloningIndex = CloningIndexCounter;
		}
	}
}