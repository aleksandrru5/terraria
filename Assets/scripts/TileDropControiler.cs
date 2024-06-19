using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileDropControiler : MonoBehaviour
{
    public ItemClass item;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (other.GetComponent<Inventory>().Add(item))
            {
                Destroy(gameObject);
            }
        }
    }
}
