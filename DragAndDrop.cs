using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DragAndDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    RectTransform rt;
    Image image;
    int index;
    Transform defaultParent ;
    Transform topParent;
    Vector2 beginPos;
    Vector2 offset;
    List<Transform> hasOverlapTfList;
    void Awake()
    {
        image = GetComponent<Image>();
        rt = GetComponent<RectTransform>();
        index = transform.GetSiblingIndex();
        defaultParent = GetComponentInParent<Canvas>().transform;
        topParent = GameObject.Find("Top").GetComponent<Transform>();
        hasOverlapTfList = new List<Transform>();
    }
    // 记录开始拖动时的位置，用于计算偏移量
    private Vector2 m_StartPos;

    public void OnBeginDrag(PointerEventData eventData)
    {
        transform.SetParent(topParent);
        beginPos = GetMouseWorldPosition(eventData);
        offset = beginPos - rt.anchoredPosition;

        // 记录开始拖动时UI元素的世界位置
        Debug.LogError("Drag " + gameObject.name);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 newPos = GetMouseWorldPosition(eventData);
        // image.raycastTarget  = false;
        rt.anchoredPosition = newPos - offset;
        // 注意：这里的转换可能需要根据你的UI Canvas设置进行调整，比如是否使用UI Scale Mode为Screen Space - Overlay等
    }
    private Vector2 GetMouseWorldPosition(PointerEventData eventData)
    {
        Vector2 localMousePos;
        var parentRT = topParent.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRT, eventData.position, null, out localMousePos);
        return localMousePos;
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        // 在拖动结束后可以执行一些清理或回调操作
        var newParent = GetOverlapObj();
        if (newParent != null)
        {
            rt.SetParent(newParent);
            rt.anchoredPosition = Vector2.zero;
        }else
        {
            if(transform.parent != defaultParent)
            {
                transform.SetParent(defaultParent);
            }
        }
        // image.raycastTarget  = true;
    }
    //是否存在重叠
    public Transform GetOverlapObj()
    {
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        hasOverlapTfList.Clear();
        for(int i  = 0 ; i < corners.Length; i++)
        {
            Transform hasOverlapTf = GetHoverObj(corners[i]);
            if(hasOverlapTf != null)
            {
                hasOverlapTfList.Add(hasOverlapTf);
            }
        }
        if(hasOverlapTfList.Count> 1)
        {
            float minDis = 999f;
            int minDisIndex = 0;
            for(int i = 0 ; i < hasOverlapTfList.Count; i++)
            {
                float dis = Vector3.Distance(transform.position, hasOverlapTfList[i].position);
                if(dis < minDis)
                {
                    minDis = dis;
                    minDisIndex = i;
                }
            }
            return hasOverlapTfList[minDisIndex];
        }else if(hasOverlapTfList.Count > 0)
        {
            return hasOverlapTfList[0];
        }
        return null;

    }

    public Transform GetHoverObj(Vector2 pos)
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = pos;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        if (results.Count > 0)
        {
            RaycastResult topMostUI = results[0];
            GameObject hoveredObject = topMostUI.gameObject;
            Transform child = hoveredObject.transform.Find("Child");
            Debug.Log("鼠标悬停在：" + hoveredObject.name);
            return child;
        }
        else
        {
            
            Debug.Log("鼠标未悬停在任何UI元素上");
        }
        return null;
    }
}
