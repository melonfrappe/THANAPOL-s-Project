using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPanelBehavior : MonoBehaviour {

    protected RectTransform rectTrf;

    protected virtual void Awake()
    {
        rectTrf = GetComponent<RectTransform>();
    }

    public virtual void Show()
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
    }

    public virtual void Hide()
    {
        if (gameObject.activeSelf)
            gameObject.SetActive(false);
    }
}
