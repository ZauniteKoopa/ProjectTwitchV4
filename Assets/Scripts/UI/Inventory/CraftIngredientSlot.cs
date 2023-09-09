using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class CraftIngredientSlot : MonoBehaviour, IDropHandler, IPointerDownHandler
{
    //UI elements
    [SerializeField]
    private Image icon = null;
    [SerializeField]
    private Color emptyColor = Color.clear;
    private IngredientIcon ingredientIcon = null;
    public UnityEvent ingredientDropEvent;

    //Audio
    //private AudioSource audioFX;

    // Start is called before the first frame update
    void Awake()
    {
        // OnIngredientSelect = new IngredientSelectDelegate();
        //audioFX = GetComponent<AudioSource>();
    }

    //Method to drop Ingredient Icon in this slot
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            IngredientIcon ingIcon = eventData.pointerDrag.GetComponent<IngredientIcon>();
            if (ingIcon != null && ingIcon.IngredientExists())
            {
                //audioFX.Play(0);

                //if there's an ingredient icon already in here, give an ingredient back to that icon
                if (ingredientIcon != null)
                {
                    ingredientIcon.ReturnIngredient();
                }

                ingredientIcon = ingIcon;
                ingIcon.SetIngredientForCrafting();
                icon.color = PoisonVial.poisonVialConstants.getPureColor(ingIcon.GetRepresentedStat(), false);
                ingredientDropEvent.Invoke();
            }
        }
    }

    //Event handler when clicking down on icon
    public void OnPointerDown(PointerEventData eventData)
    {
        // if (ingredientIcon != null && ingredientIcon.GetIngredient() != null)
        //     OnIngredientSelect.Invoke(ingredientIcon.GetIngredient());
    }

    //Accessor method to ingredient
    public bool hasIngredient(out PoisonVialStat ingStat)
    {
        ingStat = (ingredientIcon != null) ? ingredientIcon.GetRepresentedStat() : PoisonVialStat.POTENCY;
        return ingredientIcon != null;
    }
    

    // Accessor method to jsut check if you have an ingredient
    public bool hasIngredient() {
        return ingredientIcon != null;
    }


    //Method to reset all this slot
    public void Reset()
    {
        if (ingredientIcon != null)
            ingredientIcon.ReturnIngredient();
        
        ingredientIcon = null;
        icon.color = emptyColor;
    }

    //Method to clear the ingredient without having any affect on ingredient counts
    public void ClearIngredient()
    {
        ingredientIcon = null;
        Reset();
    }
}
