using System.Collections;
using UnityEngine;

public class Bumper : MonoBehaviour
{
    [SerializeField] private float baseForce = 10f;
    [SerializeField] private float parryMult = 2.5f;
    [SerializeField] private float hitStopTime = 0.05f;

    private void OnCollisionEnter2D(Collision2D other)
    {
        Rigidbody2D rb = other.rigidbody;
        if (rb == null) return;

        //Calculate where we will bounce based on where we hit the bumper
        Vector2 normal = -other.contacts[0].normal;
        Vector2 bounceDirection = normal.normalized;

        float mult = 1f;
        float force = baseForce;

        if (other.gameObject.TryGetComponent<ParryState>(out var parry) && parry.IsParrying)
        {
            force *= parryMult;
            parry.Consume();

            if (other.gameObject.TryGetComponent<BallControl>(out var ball))
            {
                Vector2 aim = ball.GetParryAim();

                if (aim != Vector2.zero)
                {
                    //Project aim away from the bumper surface
                    bounceDirection = Vector2.Lerp(Vector2.Reflect(rb.linearVelocity.normalized, normal),
                        Vector2.Reflect(-aim, normal).normalized, 
                        0.75f).normalized;
                }
                else
                {
                    bounceDirection = Vector2.Reflect(rb.linearVelocity.normalized, normal);
                }
            }

            StartCoroutine(HitStop());
        }

        bounceDirection.y = Mathf.Max(bounceDirection.y, 0.25f);
        bounceDirection.Normalize();
        
        rb.linearVelocity = Vector2.zero; // Reset to not have leftover forces
        rb.AddForce(bounceDirection * force, ForceMode2D.Impulse);
    }

    private IEnumerator HitStop()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(hitStopTime);
        Time.timeScale = 1f;
    }
}
