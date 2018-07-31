using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Siri;
using System;
using System.Collections.Generic;
using System.Linq;

public static class ExtensionMethods
{
    /// <summary>
    /// Gets or add a component. Usage example:
    /// BoxCollider boxCollider = transform.GetOrAddComponent<BoxCollider>();
    /// </summary>
    public static T GetOrAddComponent<T>(this Component child) where T : Component
    {
        T result = child.GetComponent<T>();
        if (result == null)
        {
            result = child.gameObject.AddComponent<T>();
        }
        return result;
    }

	public static float LinearRemap(this float value,float vRangeMin,float vRangeMax,float nRangeMin,float nRangeMax)
	{
		return (value - vRangeMin) / (vRangeMax - vRangeMin) * (nRangeMax - nRangeMin) + nRangeMin;
	}

	public static int WithRandomSign(this int value,float negativeProbability = 0.5f)
	{
		return UnityEngine.Random.value < negativeProbability ? -value : value;
	}

	public static T MostCommon<T>(this IEnumerable<T> list)
	{
		return (from i in list
				group i by i into grp
				orderby grp.Count() descending
				select grp.Key).First();
	}

	public static T ParseEnum<T>(string value)
	{
		return (T)Enum.Parse(typeof(T),value,true);
	}

	public static void TweenTranfrom(this Transform trans,Ttype ttype,Easing.Type easingType,
						Vector3 aVec,Vector3 bVec,float duration,float delay = 0,Action callback = null)
	{
		TweenExtensions.Transform(trans,ttype,aVec,bVec,duration,delay,easingType,callback);
	}

	public static void TweenRectTrans(this RectTransform trans,Rtype rtype,Easing.Type easingType,
							Vector2 aVec,Vector2 bVec,float duration,float delay = 0,Action callback = null)
	{
		TweenExtensions.Transform(trans,rtype,aVec,bVec,duration,delay,easingType,callback);
	}
}



public static class UIExtensions
{
	public static RectOffset GetPadding(GameObject obj)
	{
		if (obj.GetComponent<LayoutGroup>())
			return obj.GetComponent<LayoutGroup>().padding;
		return null;
	}

    public static void SetRect(this RectTransform trs, float left, float top, float right, float bottom)
    {
        trs.offsetMin = new Vector2(left, bottom);
        trs.offsetMax = new Vector2(-right, -top);
    }
    public static void SetRect(this RectTransform trs, RectTransform.Edge edge, float value)
    {
        float _left = trs.offsetMin.x;
        float _right = -trs.offsetMax.x;
        float _top = -trs.offsetMax.y;
        float _bottom = trs.offsetMin.y;
        switch (edge)
        {
            case RectTransform.Edge.Left:
            _left = value;
            break;
            case RectTransform.Edge.Right:
            _right = value;
            break;
            case RectTransform.Edge.Top:
            _top = value;
            break;
            case RectTransform.Edge.Bottom:
            _bottom = value;
            break;
        }
        trs.SetRect(_left, _top, _right, _bottom);
    }

    public static void AnchorToCorners(this RectTransform trans)
	{
		if (trans == null)
		{
			Debug.Log("transform == null");
			return;
		}
		//	throw new ArgumentNullException("transform");

		if (trans.parent == null)
			return;

		var parent = trans.parent.GetComponent<RectTransform>();

		Vector2 newAnchorsMin = new Vector2(trans.anchorMin.x + trans.offsetMin.x / parent.rect.width,
						 trans.anchorMin.y + trans.offsetMin.y / parent.rect.height);

		Vector2 newAnchorsMax = new Vector2(trans.anchorMax.x + trans.offsetMax.x / parent.rect.width,
						 trans.anchorMax.y + trans.offsetMax.y / parent.rect.height);

		trans.anchorMin = newAnchorsMin;
		trans.anchorMax = newAnchorsMax;
		trans.offsetMin = trans.offsetMax = new Vector2(0,0);
	}
	public static void SetPivotAndAnchors(this RectTransform trans,Vector2 aVec)
	{
		trans.pivot = aVec;
		trans.anchorMin = aVec;
		trans.anchorMax = aVec;
	}
	public static Vector2 GetSize(this RectTransform trans)
	{
		return trans.rect.size;
	}
	public static float GetWidth(this RectTransform trans)
	{
		return trans.rect.width;
	}
	public static float GetHeight(this RectTransform trans)
	{
		return trans.rect.height;
	}
	public static void SetSize(this RectTransform trans,Vector2 newSize)
	{
		Vector2 oldSize = trans.rect.size;
		Vector2 deltaSize = newSize - oldSize;
		trans.offsetMin = trans.offsetMin - new Vector2(deltaSize.x * trans.pivot.x,deltaSize.y * trans.pivot.y);
		trans.offsetMax = trans.offsetMax + new Vector2(deltaSize.x * (1f - trans.pivot.x),deltaSize.y * (1f - trans.pivot.y));
	}
	public static void SetWidth(this RectTransform trans,float newSize)
	{
		SetSize(trans,new Vector2(newSize,trans.rect.size.y));
	}
	public static void SetHeight(this RectTransform trans,float newSize)
	{
		SetSize(trans,new Vector2(trans.rect.size.x,newSize));
	}
	public static void SetBottomLeftPosition(this RectTransform trans,Vector2 newPos)
	{
		trans.localPosition = new Vector3(newPos.x + (trans.pivot.x * trans.rect.width),newPos.y + (trans.pivot.y * trans.rect.height),trans.localPosition.z);
	}
	public static void SetTopLeftPosition(this RectTransform trans,Vector2 newPos)
	{
		trans.localPosition = new Vector3(newPos.x + (trans.pivot.x * trans.rect.width),newPos.y - ((1f - trans.pivot.y) * trans.rect.height),trans.localPosition.z);
	}
	public static void SetBottomRightPosition(this RectTransform trans,Vector2 newPos)
	{
		trans.localPosition = new Vector3(newPos.x - ((1f - trans.pivot.x) * trans.rect.width),newPos.y + (trans.pivot.y * trans.rect.height),trans.localPosition.z);
	}
	public static void SetRightTopPosition(this RectTransform trans,Vector2 newPos)
	{
		trans.localPosition = new Vector3(newPos.x - ((1f - trans.pivot.x) * trans.rect.width),newPos.y - ((1f - trans.pivot.y) * trans.rect.height),trans.localPosition.z);
	}

	public static void EventTrigger(this UIBehaviour ui,EventTriggerType eventType,UnityAction<BaseEventData> action)
	{
		EventTrigger trigger = ui.GetComponentInParent<EventTrigger>();
		if (trigger == null)
			trigger = ui.gameObject.AddComponent<EventTrigger>();
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = eventType;
		entry.callback.AddListener(action);
		trigger.triggers.Add(entry);
	}
	public static void EventTrigger(this Selectable selectable,EventTriggerType eventType,UnityAction<BaseEventData> action)
	{
		EventTrigger trigger = selectable.GetComponentInParent<EventTrigger>();
		if (trigger == null)
			trigger = selectable.gameObject.AddComponent<EventTrigger>();
		EventTrigger.Entry entry = new EventTrigger.Entry();
		entry.eventID = eventType;
		entry.callback.AddListener((eventData) => {
			if (selectable.interactable)
			{
				action(eventData);
			}
		});
		trigger.triggers.Add(entry);
	}

	public static void TweenAlpha(this CanvasGroup canvas,float start,float end,float time,
								Easing.Type easingType = Easing.Type.Linear,Action callback = null)
	{
		TweenExtensions.Alpha(canvas,start,end,time,easingType,callback);
	}
}



public static class ShuffleListExtensions
{

	/// <summary>
	/// Shuffle the list in place using the Fisher-Yates method.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="list"></param>
	public static void Shuffle<T>(this IList<T> list)
	{
		System.Random rng = new System.Random();
		int n = list.Count;
		while (n > 1)
		{
			n--;
			int k = rng.Next(n + 1);
			T value = list[k];
			list[k] = list[n];
			list[n] = value;
		}
	}

	/// <summary>
	/// Return a random item from the list.
	/// Sampling with replacement.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="list"></param>
	/// <returns></returns>
	public static T RandomItem<T>(this IList<T> list)
	{
		if (list.Count == 0) throw new System.IndexOutOfRangeException("Cannot select a random item from an empty list");
		return list[UnityEngine.Random.Range(0,list.Count)];
	}

	/// <summary>
	/// Removes a random item from the list, returning that item.
	/// Sampling without replacement.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="list"></param>
	/// <returns></returns>
	public static T RemoveRandom<T>(this IList<T> list)
	{
		if (list.Count == 0) throw new System.IndexOutOfRangeException("Cannot remove a random item from an empty list");
		int index = UnityEngine.Random.Range(0, list.Count);
		T item = list[index];
		list.RemoveAt(index);
		return item;
	}


}

	public static class SafetyAction
	{
		public static void SafeInvoke(this Action action)
		{
			if (action != null)
			{
				action.Invoke();
			}
		}

		public static void SafeInvoke<T>(this Action<T> action,T arg1)
		{
			if (action != null)
			{
				action.Invoke(arg1);
			}
		}

		public static void SafeInvoke<T1, T2>(this Action<T1,T2> action,T1 arg1,T2 arg2)
		{
			if (action != null)
			{
				action.Invoke(arg1,arg2);
			}
		}

	}

public static class UnityEventExtensions
{
	public static void onClick_SetListener (this Button btn ,UnityAction new_event)
	{
		Button.ButtonClickedEvent events = new Button.ButtonClickedEvent();
		events.AddListener(new_event);
        btn.onClick = events;
	}

    static Slider.SliderEvent emptySliderEvent = new Slider.SliderEvent();
    public static void SetValue(this Slider instance, float value)
    {
        var originalEvent = instance.onValueChanged;
        instance.onValueChanged = emptySliderEvent;
        instance.value = value;
        instance.onValueChanged = originalEvent;
    }

    static Toggle.ToggleEvent emptyToggleEvent = new Toggle.ToggleEvent();
    public static void SetValue(this Toggle instance, bool value)
    {
        var originalEvent = instance.onValueChanged;
        instance.onValueChanged = emptyToggleEvent;
        instance.isOn = value;
        instance.onValueChanged = originalEvent;
    }

    static InputField.OnChangeEvent emptyInputFieldEvent = new InputField.OnChangeEvent();
    public static void SetValue(this InputField instance, string value)
    {
        var originalEvent = instance.onValueChanged;
        instance.onValueChanged = emptyInputFieldEvent;
        instance.text = value;
        instance.onValueChanged = originalEvent;
    }

    static Dropdown.DropdownEvent emptyDropdownFieldEvent = new Dropdown.DropdownEvent();
    public static void SetValue(this Dropdown instance, int value)
    {
        var originalEvent = instance.onValueChanged;
        instance.onValueChanged = emptyDropdownFieldEvent;
        instance.value = value;
        instance.onValueChanged = originalEvent;
    }

    // TODO: Add more UI types here.

}




