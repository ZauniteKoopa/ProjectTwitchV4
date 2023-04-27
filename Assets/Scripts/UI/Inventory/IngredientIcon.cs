using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;

// [System.Serializable]
// public class IngredientSelectDelegate : UnityEvent<Ingredient> {}

public class IngredientIcon : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    //UI
    [SerializeField]
    private Image icon = null;
    [SerializeField]
    private TMP_Text countText = null;
    [SerializeField]
    private Color emptyColor = Color.clear;
    [SerializeField]
    private Color filledColor = Color.red;
    private PoisonVialStat representedStat;
    private int count;

    // UI layer groups: render order
    private Transform defaultParent;
    private Transform selectedParent;

    //Variables for managing drag and drop
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 startPosition;
    [SerializeField]
    private Canvas canvas = null;
    [SerializeField]
    private Canvas selectedCanvasLayer = null;
    public bool dropped;

    //Event when dragged on
    private const float ICON_SNAPBACK_TIME = 0.1f;

    //On awake, set start position
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        defaultParent = transform.parent;
        dropped = false;

        SetUpIcon(PoisonVialStat.POISON, 3, selectedCanvasLayer.transform);
    }

    //Method to set up ingredient
    public void SetUpIcon(PoisonVialStat stat, int n, Transform selectedLayer)
    {
        startPosition = GetComponent<RectTransform>().anchoredPosition;
        selectedParent = selectedLayer;
        representedStat = stat;

        count = n;
        countText.text = "" + n;
        icon.color = (n > 0) ? filledColor : emptyColor;
    }

    //Method to clear icon
    public void ClearIcon()
    {
        if (rectTransform == null && canvasGroup == null) {
            rectTransform = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        icon.color = emptyColor;
        countText.text = "0";

        rectTransform.anchoredPosition = startPosition;
        startPosition = rectTransform.anchoredPosition;
    }

    //Set this ingredient for crafting. THIS DOES NOT USE UP AN INGREDIENT AND DOESN'T CHANGE INVENTORY'S DICTIONARY
    public void SetIngredientForCrafting()
    {
        if (count > 0)
        {
            count--;
            countText.text = "" + count;
            
            if (count == 0)
            {
                icon.color = emptyColor;
                rectTransform.anchoredPosition = startPosition;
            }
        }
    }

    //Method for ingredient icon to get ingredient back from crafting
    public void ReturnIngredient()
    {
        if (count == 0)
        {
            icon.color = filledColor;
        }

        count++;
        countText.text = "" + count;
    }

    //Accessor method to ingredient
    public PoisonVialStat GetRepresentedStat()
    {
        return representedStat;
    }

    //Event handler when clicking down on icon
    public void OnPointerDown(PointerEventData eventData)
    {
        // if (ingredient != null && count > 0) {
        //     OnIngredientSelect.Invoke(ingredient);
        // }
    }

    //Event handler when beginning to drag
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (count > 0)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.6f;
            transform.SetAsLastSibling();
            transform.SetParent(selectedParent);
        }
    }

    //Event handler when dragging icon
    public void OnDrag(PointerEventData eventData)
    {
        if (count > 0)
        {
            rectTransform.anchoredPosition += (eventData.delta / canvas.scaleFactor);
        }
    }

    //Event handler when dropping icon
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dropped)
        { 
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
            StartCoroutine(BackToStart());
        }

        dropped = false;
    }

    //Private IEnumerator to go back to start.
    private IEnumerator BackToStart()
    {
        // Get accurate start position
        transform.SetParent(defaultParent);

        // Set up timer
        Vector3 curPos = rectTransform.anchoredPosition;
        float timer = 0.0f;
        float delta = 0.02f;

        while(timer < ICON_SNAPBACK_TIME)
        {
            yield return new WaitForSecondsRealtime(delta);
            timer += delta;
            float percent = timer / ICON_SNAPBACK_TIME;
            rectTransform.anchoredPosition = Vector3.Lerp(curPos, startPosition, percent);
        }
    }

    //Public method to check if ingredient even exist
    public bool IngredientExists()
    {
        return count > 0;
    }


    // Main function to handle when the inventory closes
    //  Pre: inventory is about to close
    //  Post: sets it to this automatically
    public void onInventoryClose() {
        transform.SetParent(defaultParent);
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        rectTransform.anchoredPosition = startPosition;
        dropped = false;
    }
}
