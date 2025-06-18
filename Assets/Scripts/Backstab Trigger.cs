using UnityEngine;

public class BackstabTrigger : MonoBehaviour
{

    public bool canBeBackstabbed { get; private set; }
    [SerializeField] private PlayerController playerController;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            canBeBackstabbed = true;
            GetComponent<SpriteRenderer>().enabled = true;
            playerController.canBackstab = true;
            GameObject parentObject = transform.parent.gameObject;
            playerController.enemyInRange = parentObject;
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            canBeBackstabbed = false;
            GetComponent<SpriteRenderer>().enabled = false;
            playerController.canBackstab = false;
            playerController.enemyInRange = null;
        }
    }
}

