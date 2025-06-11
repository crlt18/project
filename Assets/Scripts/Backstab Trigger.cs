using UnityEngine;

public class BackstabTrigger : MonoBehaviour
{

    public bool canBeBackstabbed { get; private set; }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            canBeBackstabbed = true;
            GetComponent<SpriteRenderer>().enabled = true;
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            canBeBackstabbed = false;
            GetComponent<SpriteRenderer>().enabled = false;
        }
    }
}

