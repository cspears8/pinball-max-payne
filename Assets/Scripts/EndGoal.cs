using UnityEngine;

public class EndGoal : MonoBehaviour
{
    private BallControl player;
    
    private void Awake()
    {
        player = FindFirstObjectByType<BallControl>();
    }
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject == player.gameObject)
        {
            Debug.Log("Level Complete!");
        }
    }
}
