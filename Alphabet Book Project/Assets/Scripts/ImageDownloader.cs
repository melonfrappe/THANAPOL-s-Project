using UnityEngine;
using System.Collections;
using System.IO;

public class ImageDownloader : MonoBehaviour {
	public IEnumerator Loader (string url,string dirName,int fileName) {
		
		if(File.Exists(GetPlatformPath()+dirName+"/" + fileName +".jpg")){
			print("Loading from the device");
			byte[] byteArray = File.ReadAllBytes(GetPlatformPath()+dirName+"/"  + fileName+".jpg");
			Texture2D texture = new Texture2D(8,8);
			texture.LoadImage(byteArray);
//			this.GetComponent<Renderer>().material.mainTexture = texture;
		}
		else {
			print("Downloading from the web");
			WWW www = new WWW(url);
			yield return www;
			Texture2D texture = www.texture;
//			this.GetComponent<Renderer>().material.mainTexture = texture;
			byte[] bytes = texture.EncodeToJPG();
			File.WriteAllBytes(GetPlatformPath()+dirName+"/"  +fileName+".jpg", bytes);
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
			print ("Path is exist");
	}
}
