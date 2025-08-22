using Unity.VisualScripting;
using UnityEngine;

public class DeadZone_Trigger : MonoBehaviour
{
    public Transform player;
    private void Update()
    {
        transform.position = new Vector2(player.position.x, transform.position.y);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>() != null)
            GameManager.instance.RestartLevel();
    }
}
