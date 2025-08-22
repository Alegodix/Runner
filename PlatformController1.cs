using UnityEngine;

public class PlatformController1 : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        /*if (collision.GetComponent<Player>() != null)
            sr.color = Color.white;
        */
    }
}
