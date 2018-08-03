using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System;

public class ImageDownloader : MonoBehaviour {
	public int Counter,IndexIsLoaded;
	public bool AvailToOpen = false,IsFirstDownloading = false;
	public IEnumerator Loader (string url,string dirName,int fileName,Action<int,Sprite> callbackSuccess,int curIndex) {
		
		if(File.Exists(GetPlatformPath()+dirName+"/" + fileName +".png")){
			//File does exist, can open
			AvailToOpen = true;

			print("Loading from the device");
			byte[] byteArray = File.ReadAllBytes(GetPlatformPath()+dirName+"/"  + fileName+".png");
			Texture2D texture = new Texture2D(8,8);
			texture.LoadImage(byteArray);
		}
		else {
			//File doesn't exist, need to download
			IndexIsLoaded = curIndex;
			IsFirstDownloading = true;
			print("Downloading from the web");
			WWW www = new WWW(url);
			yield return www;
			Texture2D texture = www.texture;

			byte[] bytes = texture.EncodeToPNG();
			texture.LoadImage (bytes);
			Sprite spr = texture.ToSprite();

			if (callbackSuccess != null) {
				callbackSuccess.Invoke (fileName,spr);

				//Count to check that last image has been download.
				Counter++;
				print (Counter);
			}
				
			File.WriteAllBytes(GetPlatformPath()+dirName+"/"  +fileName+".png", bytes);
		}

	}

	public static string GetPlatformPath(){
		#if UNITY_EDITOR
		return Application.dataPath + "/Resources/";
		#else
		return Application.persistentDataPath+ "/Resources/";
		#endif
	}

	public void CreateDirectory(string dirName){
		if (!Directory.Exists (GetPlatformPath () + dirName)) {
			Directory.CreateDirectory (GetPlatformPath () + dirName);
			print ("Path " + GetPlatformPath () + dirName + " is created");
		} else
			print ("Path  exist");
	}
}
