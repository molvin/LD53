using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectInitiator : MonoBehaviour
{
    // Start is called before the first frame update
    public BoxCollider PlayerCollider;
    public AudioSource OneShotPlayer;
    public AudioClip[] OneShotsToBePlayed;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.rigidbody.velocity.magnitude > 10)
        {
            OneShotPlayer.PlayOneShot(OneShotsToBePlayed[0]);
        }
    }
}
