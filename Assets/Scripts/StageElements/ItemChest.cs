using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemChest : MonoBehaviour
{
    // chest contents
    [SerializeField]
    private List<LobAction> chestContents = null;
    [SerializeField]
    [Min(0.1f)]
    private float itemDropRange = 1f;
    [SerializeField]
    [Min(0.1f)]
    private float itemDropSpeed = 3f;
    [SerializeField]
    private LayerMask itemCollisionLayerMask;


    private bool opened = false;


    // When player enters this trigger box, open the trigger box
    private void OnTriggerEnter(Collider collider) {
        PlayerStatus player = collider.GetComponent<PlayerStatus>();

        if (player != null && !opened) {
            opened = true;

            foreach (LobAction item in chestContents) {
                Debug.Assert(item != null);

                LobAction curItemInstance = Object.Instantiate(item, transform.position, Quaternion.identity);
                curItemInstance.lobAroundRadius(transform.position, itemDropRange, itemDropSpeed, itemCollisionLayerMask);
            }

            // Do open animation or something
            Object.Destroy(gameObject);
        }
    }


    // Main function to add an item to the chest
    //  Pre: item != null
    //  Post: item is now added as part of the chest's content
    public void addItem(LobAction item) {
        Debug.Assert(item != null);
        chestContents.Add(item);
    }

}
