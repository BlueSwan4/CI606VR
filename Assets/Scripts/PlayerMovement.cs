using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 3.0f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;


    public float turnSpeed = 60f;


    [SerializeField]private CharacterController controller;
    private Transform head;

    private Vector3 velocity;

    void Start()
    {
        head = Camera.main.transform;
    }

    void Update()
    {

        Movement();
        if (controller.isGrounded && OVRInput.GetDown(OVRInput.Button.One))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }
    void LateUpdate()
    {
        Vector3 eyeOffset = head.localPosition;
        eyeOffset.y = 0;
        head.parent.localPosition = -eyeOffset;
    }
    void Movement()
    {
        float turnInput = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x;

        float rotationAmount = turnInput * turnSpeed * Time.deltaTime;
        Vector3 curEuler = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(curEuler.x, curEuler.y + rotationAmount, curEuler.z);


        Vector2 input = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);

        Vector3 headY = new Vector3(0, head.eulerAngles.y, 0);

        Vector3 moveDirection = Quaternion.Euler(headY) * new Vector3(input.x, 0, input.y);
       
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);

        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -1f;

        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
    
}
