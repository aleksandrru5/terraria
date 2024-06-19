using System.Collections;
using UnityEngine;

public class groundCheck : MonoBehaviour
{
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("ground"))
        {
            transform.parent.GetComponent<PlayerController>().onGround = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("ground"))
        {
            transform.parent.GetComponent<PlayerController>().onGround = false;
        }
    }
}
