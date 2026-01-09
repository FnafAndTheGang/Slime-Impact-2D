using UnityEngine;

public class GrassRustle : MonoBehaviour
{
    public Animator animator;
    public string rustleAnimationName = "GrassRustle";

    private bool isPlaying = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isPlaying && collision.CompareTag("Player"))
        {
            StartCoroutine(PlayRustle());
        }
    }

    private System.Collections.IEnumerator PlayRustle()
    {
        isPlaying = true;

        animator.Play(rustleAnimationName);

        // Wait for animation to finish
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        isPlaying = false;
    }
}
