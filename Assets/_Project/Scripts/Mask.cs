using UnityEngine;

public class Mask : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float rotateSpeed = 20;

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
        // for Test
        if(Input.GetKeyDown(KeyCode.G))
        {
            MaskOn(5);
        }

        transform.position = player.position;
        transform.rotation = Quaternion.Slerp(transform.rotation, player.rotation, rotateSpeed * Time.deltaTime);
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
