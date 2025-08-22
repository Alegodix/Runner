using UnityEngine;
using UnityEngine.SocialPlatforms.GameCenter;

public class LedgeDetection : MonoBehaviour
{
    [SerializeField] private float radius;
    [SerializeField] private Player player;
    [SerializeField] private LayerMask whatIsGround;

    private bool canDetect;

    private BoxCollider2D BoxCd => GetComponent<BoxCollider2D>();

    private void Update()
    {
        if (canDetect)
            player.ledgeDetected = Physics2D.OverlapCircle(transform.position, radius, whatIsGround);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            canDetect = false;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(BoxCd.bounds.center, BoxCd.bounds.size, 0, whatIsGround);

        foreach (var hit in colliders)
        {
            if (hit.gameObject.GetComponent<PlatformController1>() != null);
                return;
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            canDetect = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
