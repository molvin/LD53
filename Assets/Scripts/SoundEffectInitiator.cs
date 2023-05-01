using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectInitiator : MonoBehaviour
{
    // Start is called before the first frame update
    public BoxCollider PlayerCollider;
    public AudioSource OneShotPlayer;
    public AudioClip[] OneShotsToBePlayed;
    private float CollisionSoundMaxCooldown, CollisionSoundCurrentCooldown;
    private Rigidbody PlayerRigidbody;
    void Start()
    {
        CollisionSoundMaxCooldown = 5f;
        CollisionSoundCurrentCooldown = 1f;
        PlayerRigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        CollisionSoundCurrentCooldown = CollisionSoundCurrentCooldown - Time.deltaTime;
        // Debug.Log("  WHOMST ARE NOT NULL??  " + PlayerCollider + "  or  " + OneShotPlayer);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision == null)
        {
            return;
        }
        Debug.Log("   POWER :   " + PlayerRigidbody.velocity.magnitude);
        //if(collision.rigidbody.velocity.magnitude > 10 && CollisionSoundCurrentCooldown <= 0)
        //{
        //    OneShotPlayer.PlayOneShot(OneShotsToBePlayed[0], 3f);
        //    CollisionSoundCurrentCooldown = CollisionSoundMaxCooldown;
        //}
    }
}
