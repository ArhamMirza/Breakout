using UnityEngine;

public class NPCController : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayAnimation(string animationTrigger)
    {
        if (animator != null)
        {
            animator.SetTrigger(animationTrigger);
        }
    }
}
