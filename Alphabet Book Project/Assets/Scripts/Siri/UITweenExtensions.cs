using System;
using System.Collections;
using UnityEngine;
///
/// Easing Reference : http://easings.net/
/// 
namespace Siri
{
	public enum Ttype
	{
		Position,
		LocalPosition,
		AnchoredPosition,
		EulerAngles,
		LocalEulerAngles,
		Scale
	}
	public enum Rtype
	{
		Position,
		LocalPosition,
		AnchoredPosition,
		EulerAngles,
		LocalEulerAngles,
		Scale,
		SizeDelta
	}

	public class TweenExtensions 
	{
		public static void Transform(Transform trans,Ttype ttype,
							Vector3 aVec,Vector3 bVec,float duration,float delay = 0,
							Easing.Type easingType = Easing.Type.Linear,Action callback = null)
		{
			Tweener tweener = trans.gameObject.AddComponent<Tweener>();
			tweener.TweenTransform(trans,ttype,aVec,bVec,duration,delay,easingType,callback);
		}

		public static void Transform(RectTransform rTrans,Rtype rtype,
							Vector2 aVec,Vector2 bVec,float duration,float delay = 0,
							Easing.Type easingType = Easing.Type.Linear,Action callback = null)
		{
			Tweener tweener = rTrans.gameObject.AddComponent<Tweener>();
			tweener.TweenTransform(rTrans,rtype,aVec,bVec,duration,delay,easingType,callback);
		}

		public static void Float(Action<float> target,float start,float end,float duration,float delay = 0,
							Easing.Type easingType = Easing.Type.Linear,Action callback = null)
		{
			Tweener tweener = new GameObject("Tweener").AddComponent<Tweener>();
			tweener.Float(target,start,end,duration,delay,easingType,callback);
		}

		public static void Color(Action<Color> target,Color start,Color end,float duration,float delay = 0,
							Easing.Type easingType= Easing.Type.Linear,Action callback = null)
		{
			Tweener tweener = new GameObject("Tweener").AddComponent<Tweener>();
			tweener.TweenColor(target,start,end,duration,delay,easingType,callback);
		}

		public static void Alpha(CanvasGroup canvasGrp,float start,float end,float duration,
							Easing.Type easingType = Easing.Type.Linear,Action callback = null)
		{
			Tweener tweener = canvasGrp.gameObject.AddComponent<Tweener>();
			tweener.TweenAlpha(canvasGrp,start,end,duration,easingType,callback);
		}
	}
	public class Tweener : MonoBehaviour
	{
		public void TweenTransform(Transform trans,Ttype ttype,Vector3 aVec,Vector3 bVec,float duration,float delay,
									Easing.Type easingType,Action callback)
		{
			StartCoroutine(COTransform(trans,ttype,aVec,bVec,duration,delay,easingType,callback));
		}
		public void TweenTransform(RectTransform trans,Rtype rtype,Vector2 aVec,Vector2 bVec,float duration,float delay,
									Easing.Type easingType,Action callback)
		{
			StartCoroutine(COTransform(trans,rtype,aVec,bVec,duration,delay,easingType,callback));
		}
		IEnumerator COTransform(Transform trans,Ttype ttype,Vector3 aVec,Vector3 bVec,float duration,float delay,
									Easing.Type easingType,Action callback)
		{
			yield return new WaitForSeconds(delay);
			float countTime = 0;
			float perc = 0;
			float progress = 0;

			while (countTime < duration)
			{
				countTime += Time.deltaTime;
				if (countTime > duration)
				{
					countTime = duration;
					break;
				}
				perc = countTime / duration;
				progress = Easing.GetEasingFunction(easingType,perc);
				SetPos(trans,ttype,Vector3.Lerp(aVec,bVec,progress));
				yield return null;
			}
			SetPos(trans,ttype,bVec);
			if (callback != null)
				callback();
			Destroy(this);
		}
		IEnumerator COTransform(RectTransform trans,Rtype rtype,Vector2 aVec,Vector2 bVec,float duration,float delay,
									Easing.Type easingType,Action callback)
		{
			yield return new WaitForSeconds(delay);
			float countTime = 0;
			float perc = 0;
			float progress = 0;

			while (countTime < duration)
			{
				countTime += Time.deltaTime;
				if (countTime > duration)
				{
					countTime = duration;
					break;
				}
				perc = countTime / duration;
				progress = Easing.GetEasingFunction(easingType,perc);
				SetPos(trans,rtype,Vector2.Lerp(aVec,bVec,progress));
				yield return null;
			}
			SetPos(trans,rtype,bVec);

			if (callback != null)
				callback();
			Destroy(this);
		}
		private void SetPos(Transform trans,Ttype ttype,Vector3 pos)
		{
			switch (ttype)
			{
				case Ttype.Position:
					trans.position = pos;
					break;
				case Ttype.LocalPosition:
					trans.localPosition = pos;
					break;
				case Ttype.EulerAngles:
					trans.eulerAngles = pos;
					break;
				case Ttype.LocalEulerAngles:
					trans.localEulerAngles = pos;
					break;
				case Ttype.Scale:
					trans.localScale = pos;
					break;
			}
		}
		private void SetPos(RectTransform trans,Rtype rtype,Vector2 pos)
		{
			switch (rtype)
			{
				case Rtype.Position:
					trans.position = pos;
					break;
				case Rtype.LocalPosition:
					trans.localPosition = pos;
					break;
				case Rtype.AnchoredPosition:
					trans.anchoredPosition = pos;
					break;
				case Rtype.EulerAngles:
					trans.eulerAngles = pos;
					break;
				case Rtype.LocalEulerAngles:
					trans.localEulerAngles = pos;
					break;
				case Rtype.Scale:
					trans.localScale = pos;
					break;
				case Rtype.SizeDelta:
					trans.sizeDelta = pos;
					break;

			}
		}

		public void Float(Action<float> target,float start,float end,float duration,float delay,Easing.Type easingType,Action callback)
		{
			StartCoroutine(COFloat(target,start,end,duration,delay,easingType,callback));
		}
		IEnumerator COFloat(Action<float> target,float start,float end,float duration,float delay,Easing.Type easingType,Action callback)
		{
			yield return new WaitForSeconds(delay);
			float countTime = 0;
			float perc = 0;
			float progress = 0;

			while (countTime < duration)
			{
				countTime += Time.deltaTime;
				perc = countTime / duration;
				progress = Easing.GetEasingFunction(easingType,perc);
				target(Mathf.Lerp(start,end,progress));
				yield return null;
			}
			target(end);
			if (callback != null)
				callback();
			Destroy(gameObject);
		}

		public void TweenColor(Action<Color> target,Color start,Color end,float duration,float delay,Easing.Type easingType,Action callback)
		{
			StartCoroutine(COTweenColor(target,start,end,duration,delay,easingType,callback));
		}
		IEnumerator COTweenColor(Action<Color> target,Color start,Color end,float duration,float delay,Easing.Type easingType,Action callback)
		{
			yield return new WaitForSeconds(delay);
			float countTime = 0;
			float perc = 0;
			float progress = 0;

			while (countTime < duration)
			{
				countTime += Time.deltaTime;
				perc = countTime / duration;
				progress = Easing.GetEasingFunction(easingType,perc);
				target(Color.Lerp(start,end,progress));
				yield return null;
			}
			target(Color.Lerp(start,end,progress));
			if (callback != null)
				callback();
			Destroy(gameObject);
		}
		
		public void TweenAlpha(CanvasGroup canvasGrp,float start,float end,float duration,Easing.Type easingType,Action callback)
		{
			StartCoroutine(COTweenAlpha(canvasGrp,start,end,duration,easingType,callback));
		}
		IEnumerator COTweenAlpha(CanvasGroup canvasGrp,float start,float end,float duration,Easing.Type easingType,Action callback)
		{
			float cnt = 0;
			while (cnt < duration)
			{
				cnt += Time.deltaTime / duration;
				canvasGrp.alpha = Mathf.Lerp(start,end,cnt);
				yield return null;
			}
			canvasGrp.alpha = end;
			if (callback != null)
				callback();
			//Delete Script
			Destroy(this);
		}
	}

	public class UITweenExtensions
	{
		public static void ToTarget(GameObject objUI,Vector3 startPos,Vector3 endPos,float time,Action callback)
		{
			UITweener tweener = objUI.AddComponent<UITweener>();
			tweener.ToTarget(startPos,new Vector2(-startPos.x,endPos.y),endPos,time,callback);
		}

	}
	public class UITweener : MonoBehaviour
	{
		public void ToTarget(Vector3 p0,Vector3 p1,Vector3 p2,float time,Action callback)
		{
			StartCoroutine(COToTarget(p0,p1,p2,time,callback));
		}
		IEnumerator COToTarget(Vector3 p0,Vector3 p1,Vector3 p2,float time,Action callback)
		{
			RectTransform rect = GetComponent<RectTransform>();
			float cnt = 0;
			while (cnt < 1)
			{
				cnt += Time.deltaTime / time;
				rect.anchoredPosition = Bezier.GetPoint(p0,p1,p2,cnt);
				yield return new WaitForFixedUpdate();
			}
			if (callback != null)
				callback();

			//Delete Script
			Destroy(this);
		}
	}
}
