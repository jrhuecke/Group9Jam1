using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/*
 NOTE: Some code here for the animator is based on an older sprite I was using that had multiple rotations,
and thus currently does nothing.
But I've left the code here in case we DO want a sprite with rotations - so we can bring it back

I'd also prefer to use a Kinematic body, but I can't get it to collide with any other bodies, even when
useFullKinematicContacts is set to true. Maybe we can make this change later?
 */

public class Player2D : MonoBehaviour
{
    Rigidbody2D rigidbody;
    SpriteRenderer sprite;
    Animator charAnimator;

    public float speedMax = 8.0f;
    Vector2 moveInput = Vector2.zero;
    float fireInput = 0;

    Vector2 mousePos;
    public GameObject crosshair;
    public GameObject weaponRoot;
    public GameObject projectileOrigin;

    [Header("On-Hit stuff")]
    public float iFrames = 1.0f;
    float endiFrames = 0.0f;

    [Header("Machine Gun")]
    public Rigidbody2D bulletPrefab;
    public int bulletDamage = 10;
    public float bulletSpeed = 16.0f;
    public float rateOfFireMG = 0.166f;
    float shootDelay = 0;
    public float randomSpreadMG = 4.0f; //Degrees in which the bullet can randomly offshoot

    void Start()
    {
        Cursor.visible = false;
        rigidbody = this.GetComponent<Rigidbody2D>();
        sprite = this.GetComponent<SpriteRenderer>();
        charAnimator = this.GetComponent<Animator>();
    }

    /*The player input component sends these function calls/"messages" to this script
      The various function calls are listed on the component itself.*/
    void OnFire(InputValue value)
    {
        fireInput = value.Get<float>();
        Cursor.visible = false;
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    /*Alt method of controlling crosshair: Make a second Input Action Asset (since I think each one is a player?)*/
    void OnAim(InputValue value)
    {
        mousePos = value.Get<Vector2>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        Vector3 converion = new Vector3(mousePos.x, mousePos.y, Camera.main.nearClipPlane);
        crosshair.transform.position = Camera.main.ScreenToWorldPoint(converion);

        //Machine gun shooting
        if (fireInput > 0.0f && Time.time >= shootDelay)
        {
            Rigidbody2D bulletInstance = Instantiate(bulletPrefab, projectileOrigin.transform.position, projectileOrigin.transform.rotation);

            //Spread feels a little off...but it does what it's supposed to do
            float randomDegrees = Random.Range(-randomSpreadMG, randomSpreadMG);
            bulletInstance.transform.Rotate(new Vector3(0, 0, randomDegrees));
            Debug.Log(randomDegrees);

            bulletInstance.velocity = bulletInstance.transform.right * bulletSpeed;
            bulletInstance.gameObject.GetComponent<DamageBoss2D>().DamageToBoss = bulletDamage;
            shootDelay = Time.time + rateOfFireMG;
        }

        Vector3 conversion = new Vector3(moveInput.x, moveInput.y);
        conversion = conversion.normalized;
        rigidbody.velocity = conversion * speedMax;

        //Animation stuff
        /*if (crosshair.transform.position.x > gameObject.transform.position.x)
        {
            weaponRoot.transform.localScale = new Vector3(1, 1, 1);
        }
        else
        {
            weaponRoot.transform.localScale = new Vector3(1, -1, 1);
        }*/

        //Gun + projectile rotation
        /*This took a lot of corrections and adjustments to get right...*/
        Vector3 crosshair2Dpos = crosshair.transform.position;
        crosshair2Dpos.z = 0;
        weaponRoot.transform.LookAt(crosshair2Dpos);

        Vector2 rootToCrosshair = crosshair2Dpos - weaponRoot.transform.position;
        Debug.Log("First vector: " + rootToCrosshair);
        Debug.Log("Second vector: " + Vector2.right);
        float aimAngle = Vector2.SignedAngle(rootToCrosshair, Vector2.right);
        Debug.Log(aimAngle);

        /* (Trying to set weaponRoot.right
         * When aiming to the exact left, the weaponRoot changes from using its Z rotation to using its Y rotation, which is why it flips along the Y axis
         * compared to its default rotation.
         */


        //Make player face the direction they're looking
        Vector2 playerLookDir = rootToCrosshair.normalized;
        charAnimator.SetFloat("LookX", playerLookDir.x);
        charAnimator.SetFloat("LookY", playerLookDir.y);
        charAnimator.SetFloat("Speed", rigidbody.velocity.magnitude);

        //Decide if doing a vertical or horizontal animation
        //Horizontal if magnitude of x > magnitude of y, and vice-versa
        if(Mathf.Abs(playerLookDir.x) >= Mathf.Abs(playerLookDir.y)) //Horizontal animation
        {
            if(rigidbody.velocity.magnitude > 0 && playerLookDir.x > 0 && rigidbody.velocity.x < 0) //If velocity != look direction
            {
                //Play animation backwards
                charAnimator.SetBool("PlayAnimBackwards", true);
            }
            else
            {
                //Play animation forwards, as usual
                charAnimator.SetBool("PlayAnimBackwards", false);
            }
        }
        else //Vertical animation
        {
            if (rigidbody.velocity.magnitude > 0 && playerLookDir.y > 0 && rigidbody.velocity.y < 0) //If velocity != look direction
            {
                //Play animation backwards
                charAnimator.SetBool("PlayAnimBackwards", true);
            }
            else
            {
                //Play animation forwards, as usual
                charAnimator.SetBool("PlayAnimBackwards", false);
            }
        }
    }

    //To be called by trigger colliders that are meant to deal damage to the player (bullets, boss melee, etc)
    public void TakeDamage(int damage)
    {
        if(damage > -1 && Time.time >= endiFrames)
        {
            Debug.Log("Player took " + damage + " damage!");
            endiFrames = Time.time + iFrames;
        }
        else if(damage < 0)
        {
            Debug.Log("Player took " + damage + " damage!");
        }
    }
}
