using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinPoint : MonoBehaviour
{
    public float Radius;
    public LayerMask PlayerLayer;
    public RunManager Manager;
    public bool won;

    private void Update()
    {
        if (won)
            return;
        Collider[] colls = Physics.OverlapSphere(transform.position, Radius, PlayerLayer);
        foreach(Collider coll in colls)
        {
            var player = coll.GetComponent<HoverController>();
            if(player)
            {
                Manager.Win();
                won = true;
            }
        }
    }
}
