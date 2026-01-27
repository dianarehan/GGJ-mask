using UnityEngine;

public class Mask : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform mask;
    [SerializeField] private float moveRate = .3f;
    [SerializeField] Animator animator;
    public static Mask Instance { get; private set;}

    bool IsMaskOn = true;

    Vector2 currentVelocity;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Instance == null)
            Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        mask.position = Vector2.SmoothDamp(mask.position, player.position, ref currentVelocity, moveRate);
        mask.rotation = player.rotation;
    }

    public void MaskOn(float maskTime)
    {
        animator.Play("TakeOnTheMask");
        Invoke(nameof (MaskOff), maskTime);

    }

    void MaskOff() 
    {
        animator.Play("TakeOffTheMask");
    }


}
