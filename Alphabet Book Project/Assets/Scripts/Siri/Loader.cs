using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Siri
{
	public class Loader : MonoBehaviour
	{
		public void LoadPhoto(string photo_uri,Action<Sprite> setSprite)
		{
			StartCoroutine(WaitLoadPhoto(photo_uri,setSprite));
		}

		IEnumerator WaitLoadPhoto(string photo_uri,Action<Sprite> onFinish)
		{
            bool isSuccess = false;
            string error_message = null;
            int time = 15;
            for (int i = 0; i < time; i++)
            {
                WWW url = new WWW(photo_uri);
                yield return url;
                if (string.IsNullOrEmpty(url.error))
                {
                    Debug.Log("Completed loaded photo." + photo_uri);
                    onFinish(Sprite.Create(url.texture, new Rect(0, 0, url.texture.width, url.texture.height), new Vector2(0.5f, 0.5f), 100.0f));
                    isSuccess = true;
                    break;
                }
                else
                {
                    error_message = url.error;
                    Debug.LogWarning("Try again\nLoad Photo Error Message :" + error_message);                   
                }
            }
			
			if(!isSuccess)
			{
				onFinish(null);
                Debug.LogWarning("Load Photo Error Message :" + error_message);
            }
			Destroy(this);
		}
	}
}