using UnityEngine;
using System.Collections;
using System.IO;

public class ImageDownloader : MonoBehaviour {

	// Use this for initialization
	IEnumerator Start () {
		if(File.Exists(Application.persistentDataPath + "testTexture.jpg")){
			print("Loading from the device");
			byte[] byteArray = File.ReadAllBytes(Application.persistentDataPath + "testTexture.jpg");
			Texture2D texture = new Texture2D(8,8);
			texture.LoadImage(byteArray);
			this.GetComponent<Renderer>().material.mainTexture = texture;
		}
		else {
			print("Downloading from the web");
			WWW www = new WWW("https://lh3.googleusercontent.com/UWjhYQauLwmEh2DNyej--iXJMqIkRM5811suN1u5GgGz6JUxsDiCNV84LVxvHTz8L1dZMw=s85");
			yield return www;
			Texture2D texture = www.texture;
			this.GetComponent<Renderer>().material.mainTexture = texture;
			byte[] bytes = texture.EncodeToJPG();
			File.WriteAllBytes(Application.persistentDataPath + "testTexture.jpg", bytes);
		}


		
	}

}
