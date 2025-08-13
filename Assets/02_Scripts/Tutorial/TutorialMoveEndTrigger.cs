using UnityEngine;

public class TutorialMoveEndTrigger : MonoBehaviour
{
    [SerializeField] TutorialManager tutorialManager;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            tutorialManager.OnReachMoveDirectionEnd();
        }
    }
}