using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public GameObject Player;
    public GameObject Camera;
    public GameObject HookPoint;
    public GameObject GrappleAnimSystem;

    public Transform GrappleLocation;


    public GameObject DeathScreen;
    public GameObject Crosshair;
    public TMP_Text Text;


    public Rigidbody rb;
    public SpringJoint sj;


    public LayerMask ExcludePlayer;

    public float Speed = 1.0f;
    public float Gravity = 100f;
    public float JumpForce = 5.0f;
    public float PullForce = 15f;

    float ForwardInput;
    float RightInput;
    Vector3 MovementInput;

    public float Drag = 0.98f;
    public float HookDrag = 0.99f;


    bool IsHooked = false;

    bool IsGrounded = false;

    bool GoingToJump = false;

    bool GoingToBoostJump = false;

    bool Died = false;

    

    GameObject CurrentHookPoint;
    GameObject CurrentGrappleAnim;


    public float HookRange = 10f;


    public Material[] SkyBoxMaterials;

    private void Awake()
    {
        //setup random skybox system!
        RenderSettings.skybox = SkyBoxMaterials[Random.Range(0, SkyBoxMaterials.Length - 1)];

    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        //RenderSettings.skybox = SkyBoxMaterials[Random.Range(0, SkyBoxMaterials.Length - 1)];


    }



    private void Update()
    {




        if (Died)
        {


            if (Input.GetKeyDown(KeyCode.Space))
            {

                Death();

            }

            return;

        }

        RightInput = Input.GetAxis("Horizontal");
        ForwardInput = Input.GetAxis("Vertical");
        MovementInput = transform.TransformDirection(Vector3.ClampMagnitude(new Vector3(RightInput, 0, ForwardInput), 1));


        if (Input.GetMouseButtonDown(1) && !IsHooked)
        {

            //Initiate grapple!!!

            RaycastHit Hit;

            if (Physics.SphereCast(transform.position, 0.75f, Camera.transform.forward, out Hit, HookRange, ExcludePlayer))
            {
                //we hit a point within our hook range
                CurrentHookPoint = Instantiate(HookPoint, Hit.point, Quaternion.identity);
                CurrentGrappleAnim = Instantiate(GrappleAnimSystem, GrappleLocation.position, Quaternion.identity);
                CurrentGrappleAnim.GetComponent<GrappleAnim>().HookPosition = Hit.point;
                CurrentGrappleAnim.GetComponent<GrappleAnim>().PlayerGrapplePoint = GrappleLocation;

                sj = Player.AddComponent<SpringJoint>();
                sj.autoConfigureConnectedAnchor = false;
                sj.connectedAnchor = Vector3.zero;
                sj.maxDistance = Hit.distance * 0.75f;
                sj.minDistance = Hit.distance * 0.15f;

                sj.spring = 4.5f;
                sj.damper = 7f;
                sj.massScale = 4.5f;

                sj.connectedBody = CurrentHookPoint.GetComponent<Rigidbody>();
                IsHooked = true;

            }
            else
            {
                //hook missed!


            }



        }



        



        if (Input.GetMouseButtonUp(1) && IsHooked)
        {

            Destroy(CurrentHookPoint);
            //CurrentHookPoint = null;
            Destroy(CurrentGrappleAnim);
            Destroy(Player.GetComponent<SpringJoint>());
            IsHooked = false;

        }





        if (Input.GetKey(KeyCode.Space) && !GoingToJump)
        {

            if (AmGrounded())
            {
                GoingToJump = true;
            }
            else if (!GoingToBoostJump && IsHooked)
            {

                GoingToBoostJump = true;

            }

        }


    }


    public void Death()
    {

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    }


    private void FixedUpdate()
    {

        if (Player.transform.position.y <= -50f && !Died)
        {

            Died = true;
            Camera.GetComponentInParent<CameraController>().Death();
            rb.velocity = Vector3.zero;


            Text.text = "You reached " + transform.position.z.ToString("F2") + " meters!             Press the space bar to try again";

            DeathScreen.SetActive(true);
            Crosshair.SetActive(false);

        }

        if (Died) { return; }


        IsGrounded = AmGrounded();


        if (IsGrounded)
        {
            //on the ground!

            rb.AddForce(MovementInput * Speed, ForceMode.Force);

            if (GoingToJump)
            {
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                rb.AddForce(transform.up * JumpForce, ForceMode.Impulse);

                GoingToJump = false;
            }

            //rb.AddForce(-transform.up * Gravity, ForceMode.Acceleration);


            rb.velocity = new Vector3(rb.velocity.x * Drag, rb.velocity.y, rb.velocity.z * Drag);

        }
        else if (!IsGrounded && !IsHooked)
        {
            // mid air

            rb.AddForce(MovementInput * Speed/7, ForceMode.Force);

            rb.AddForce(-transform.up * Gravity, ForceMode.Acceleration);


            rb.velocity = new Vector3(rb.velocity.x*0.9975f, rb.velocity.y, rb.velocity.z*0.9975f);



        } 
        else if (!IsGrounded && IsHooked)
        {

            // on hook!



            if(sj != null)
            {
                if (sj.maxDistance > sj.minDistance + 0.5f)
                {
                    sj.maxDistance = sj.maxDistance * 0.985f;
                }
            }

            rb.AddForce(MovementInput * Speed, ForceMode.Force);

            rb.AddForce(-transform.up * Gravity, ForceMode.Acceleration);


            rb.velocity = new Vector3(rb.velocity.x * HookDrag, rb.velocity.y, rb.velocity.z * HookDrag);

            if (GoingToBoostJump)
            {
                Destroy(CurrentHookPoint);
                //CurrentHookPoint = null;
                Destroy(CurrentGrappleAnim);
                Destroy(Player.GetComponent<SpringJoint>());
                IsHooked = false;

                GoingToBoostJump = false;

                rb.AddForce(transform.up * JumpForce * 1.2f, ForceMode.Impulse);

            }


        }




    }



    bool AmGrounded()
    {

        RaycastHit Hit;

        if (Physics.SphereCast(Player.transform.position, 0.499f, -transform.up, out Hit, 0.505f, ExcludePlayer))
        {
            return true;
        }
        else
        {
            return false;
        }


    }

   

}
