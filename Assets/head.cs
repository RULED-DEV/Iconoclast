using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class head : MonoBehaviour
{

    public Rigidbody2D rb;
    bool rg;

    public void INIT(){

    }

    public void rgdoll(){ // arms break when ragdolling
        HingeJoint2D hinge = GetComponent<HingeJoint2D>();
        Collider2D col = GetComponent<Collider2D>();
        if(rg){
            // stops being ragdoll
            transform.localPosition = Vector3.zero;
            transform.localEulerAngles = Vector3.zero;
            rg = false;
            rb.isKinematic = true;
            rb.angularVelocity = 0;
            hinge.enabled = false;
            col.isTrigger = true;
        }
        else{
            // starts being ragdoll
            rg = true;
            rb.isKinematic = false;
            hinge.enabled = true;
            col.isTrigger = false;
        }
    }
}
