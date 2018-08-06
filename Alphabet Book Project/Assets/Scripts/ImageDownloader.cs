using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System;

public class ImageDownloader : MonoBehaviour {
	public int Counter;
	[SerializeField] GameObject content;
	public IEnumerator Loader (string url,string dirName,int fileName,Action<int,Sprite> callbackSuccess,int curIndex) {
		
		if(File.Exists(GetResourcesPath()+dirName+"/" + fileName +".png")){
			//File does exist, can open

			byte[] bytes = File.ReadAllBytes(GetResourcesPath()+dirName+"/"  + fileName+".png");
			Texture2D texture = new Texture2D(8,8);
			texture.LoadImage(bytes);
			Counter++;
			print ("Page #"+Counter+" is loading from device");
			Sprite spr = texture.ToSprite();
			if (callbackSuccess != null) {
				callbackSuccess.Invoke (fileName,spr);

			}
		}
		else {
			//File doesn't exist, need to download

			//Set flag in selected book (clone) that first time download
			WWW www = new WWW(url);
			yield return www;
			Texture2D texture = www.texture;

			byte[] bytes = texture.EncodeToPNG();
			texture.LoadImage (bytes);
			Sprite spr = texture.ToSprite();

			//Count to check that last image has been download.
			Counter++;
			print ("Page #"+Counter+" is loading from server");

			if (callbackSuccess != null) {
				callbackSuccess.Invoke (fileName,spr);

			}
				
			File.WriteAllBytes(GetResourcesPath()+dirName+"/"  +fileName+".png", bytes);
		}

	}

	public string GetResourcesPath(){
		#if UNITY_EDITOR
		return Application.dataPath + "/Resources/";
		#else
		return Application.persistentDataPath+ "/Resources/";
		#endif
	}

	public void CreateDirectory(string dirName){
		if (!Directory.Exists (GetResourcesPath () + dirName)) {
			Directory.CreateDirectory (GetResourcesPath () + dirName);
			print ("Path " + GetResourcesPath () + dirName + " is created");
		} else
			print ("Path does exist");
	}

	public void RemoveDirectory(string dirName){
		if (Directory.Exists (GetResourcesPath () + dirName)) {
			Directory.Delete (GetResourcesPath () + dirName);
			print ("Path " + GetResourcesPath () + dirName + " is deleted");
		} else
			print ("Path doesn't exist");
	}

	public void DeleteFile(string dirName,int fileName){
		if (File.Exists (GetResourcesPath () +dirName+"/"+ fileName+".png")) {
			print ("Path " + GetResourcesPath () +dirName+"/" + fileName + " is deleted");
			File.Delete (GetResourcesPath ()  +dirName+"/"+ fileName+".png");
		} else
			print ("File doesn't exist");
	}
}
