using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
    [SerializeField]
    private AudioSource speaker;

    public UnityEvent chestOpenedEvent;


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
            StartCoroutine(openAction());
        }
    }


    // Main function to do open animation
    private IEnumerator openAction() {
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        chestOpenedEvent.Invoke();

        if (speaker != null && speaker.clip != null) {
            speaker.Play();
            yield return new WaitForSeconds(speaker.clip.length);
        } else {
            yield return 0;
        }

        Object.Destroy(gameObject);
    }


    // Main function to add an item to the chest
    //  Pre: item != null
    //  Post: item is now added as part of the chest's content
    public void addItem(LobAction item) {
        Debug.Assert(item != null);
        chestContents.Add(item);
    }


    // Main function to get the number of prizes in this chest
    //  Pre: none
    //  Post: returns a non-negative number representing how much items are in this chest
    public int getNumPrizes() {
        return chestContents.Count;
    }

}
