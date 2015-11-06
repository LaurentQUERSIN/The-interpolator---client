using UnityEngine;
using System.Collections;

public class PlayerShip : MonoBehaviour
{
    public float movementPrower = 10;
    public float rotationPrower = 10;

    public bool connected = false;
    public bool pause = false;

    private Rigidbody rb;

    public void Start()
    {
        rb = this.GetComponent<Rigidbody>();
    }

    public void Update()
    {
        //if (Input.GetKeyDown(KeyCode.End) == true && connected == true)
        //    pause = !pause;
        if (connected == false || pause == true)
            return;

        if (Input.GetKey("z") == true)
            rb.AddForce(transform.forward * movementPrower);
        if (Input.GetKey("s") == true)
            rb.AddForce(-1 * transform.forward * movementPrower);
        if (Input.GetKey("q") == true)
            rb.AddForce(-1 * transform.right * movementPrower);
        if (Input.GetKey("d") == true)
            rb.AddForce(transform.right * movementPrower);
        if (Input.GetKey("space") == true)
            rb.AddForce(transform.up * movementPrower);
        if (Input.GetKey(KeyCode.LeftControl) == true)
            rb.AddForce(-1 * transform.up * movementPrower);

        if (Input.GetMouseButton(1) == true)
        {
            float x = Input.GetAxis("Mouse X");
            float y = Input.GetAxis("Mouse Y");

            transform.RotateAround(Vector3.up, x * rotationPrower);
            transform.RotateAround(transform.right, y * rotationPrower);
        }
    }

}
