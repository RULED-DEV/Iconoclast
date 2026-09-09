using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class torso : MonoBehaviour
{

    public Rigidbody2D rb;

    public float MASS; // mass of the torso

    float springStrength = 2;
    float damping = 5;

    Vector2 des_orientation = Vector2.zero;

    bool rg = false;
    public bool righted = true;
    public bool rgdoll_flag;

    public float ROT_force = 5;
    public float counter_force = 2;

    public arm[] a;

    public void INIT(){
        a = GetComponentsInChildren<arm>();
        foreach(arm arms in a){arms.INIT();}
        rb = gameObject.GetComponent<Rigidbody2D>();
        rb.centerOfMass = Vector2.zero;
    }

    void Update(){
        if(!rg){
            balancer();
        }
    }

    void balancer(){
            des_orientation = new Vector2(0,1);

            float diff = Vector2.Angle(transform.up,des_orientation);
            float meas_Tar_ang = 0; // physics based rather than physics cringe
            float meas_tra_ang = transform.eulerAngles.z;
            if(Mathf.Abs(meas_tra_ang-meas_Tar_ang) > 180){ // if they are not on the same side
                if(meas_tra_ang < 180 && meas_Tar_ang > 180){
                    meas_Tar_ang -= 360;
                }
                else{
                    meas_tra_ang -= 360;
                }
            }
            int dir = 1;
            if(meas_Tar_ang < meas_tra_ang){dir = -1;}

            diff += (rb.angularVelocity*Time.deltaTime); // adds how far we are predicted to move this timestep

            float force = ROT_force*rb.mass;
                
            if(diff > 0){
                // this predicts we will be overshooting
                force = diff;
            }
            if(diff < force){
                // if rot force would cause an overshoot
                force = Mathf.Abs(diff-force);
            }
            force = Mathf.Clamp(force,-ROT_force,ROT_force); // prevents us exceeding the force limit
            rb.AddTorque(force*dir);

            if((dir > 0 && rb.angularVelocity+(force*dir) < 0) || dir < 0 && rb.angularVelocity+(force*dir) > 0){
                // we are travelling in the wrong direction
                float mag = Mathf.Abs(rb.angularVelocity);
                force = Mathf.Clamp(force,-mag,mag);
                // applies counterforce
                rb.AddTorque((force*dir)*counter_force);
            }

            if(1.0f / Time.unscaledDeltaTime < 5){
                // if we are lagging HARD
                transform.up = des_orientation;
            }

            if(Vector3.Angle(transform.up,Vector3.up) > 25){
                if(righted){
                    righted = false;
                    rgdoll_flag = true;
                }
            }
            else{
                righted = true;
            }
    }

    public void thruster(Vector2 des,Vector3 t){
        for(int i = 0; i < a.Length; i++){ // get this to  aim in valid arms held point direction
            if((i%2 == 0 && des.x != 0) || (i%2 != 0 && des.y != 0)){
                if(a[i].equipped != null){
                    a[i].equipped.thrust_man(t);
                }
            }
        }
    }

    public void aim_organiser(Vector2 des, GameObject tar){
        List<weapon> weps = new List<weapon>(); 
        List<weapon> exs = new List<weapon>(); 
        for(int i = 0; i < a.Length; i++){ // get this to  aim in valid arms held point direction
            if((i%2 == 0 && des.x != 0) || (i%2 != 0 && des.y != 0)){
                if(a[i].equipped != null){
                    a[i].arming = true;
                    if(!weps.Contains(a[i].equipped)){
                        weps.Add(a[i].equipped);
                    }
                }
            }
        }
        for(int i = 0; i < a.Length; i++){
            if(a[i].equipped){ // not one of the weapons being controlled
                if(!exs.Contains(a[i].equipped) && !weps.Contains(a[i].equipped)){
                    exs.Add(a[i].equipped);
                }
            }
            else{
                a[i].tar_pos(transform.position);
            }
        }
        foreach(weapon w in weps){
            w.wepcontrol(tar,"att");
        }
        foreach(weapon w in exs){
            w.wepcontrol(tar,"rest");
        }
    }

    public bool equip_checker(int arm_sel, weapon w){
        bool eq = false;
        // bug, when arm_sel = -1 it will equip new weapons over reequipping
        for(int i = 0; i < a.Length; i++){
            if(i%2 == arm_sel || arm_sel == -1){ // only selects odd/even arms dependant on inp
                int offset = 1;
                if(arm_sel == 1){offset = -1;}
                int cum = Mathf.Clamp(offset+i,0,a.Length-1); // clamped to always be in range

                if(a[i].equipped == null){ 
                    // if hand is free
                    if(a[cum].equipped != null && a[cum].equipped.can_dual){
                        // if other hand has a weapon
                        a[cum].equipped.equip_to_wep(a[i]);
                    }
                    else if(w != null){
                        // if there is a weapon to equip
                        w.equip_to_wep(a[i]);
                        eq = true;
                        if(w.can_dual){
                            if(a[cum].equipped == null){
                                // if other hand is free
                                w.equip_to_wep(a[cum]);
                            }
                        }
                    }
                }
                if(arm_sel == -1){break;}
            }
        }
        return eq;
    }

    public void dequip_checker(int arm_sel){
        for(int i = 0; i < a.Length; i++){
            if(i%2 == arm_sel || arm_sel == -1){ // only selects odd/even arms dependant on inp
                if(a[i].equipped != null){
                    a[i].equipped.grabbed.RemoveAt(a[i].equipped.grabbed.IndexOf(a[i])); // remove arm from weapon
                    a[i].equipped = null; // remove weapon from arm
                    a[i].hold_point = null;
                    a[i].target.transform.parent = null;
                }
            }
        }
    }

    public void rgdoll(){ // arms break when ragdolling
        righted = false;
        Rigidbody2D[] rbs = GetComponentsInChildren<Rigidbody2D>();
        rb_test[] rbt = GetComponentsInChildren<rb_test>();
        HingeJoint2D[] hinge = transform.GetComponentsInChildren<HingeJoint2D>();
        if(rg){
            // stops being ragdoll
            rg = false;
            for(int i = 0; i < rbs.Length; i++){
                if(rbs[i] != rb && rbs[i].GetComponent<weapon>() == null && rbs[i].tag != "coll"){
                    rbs[i].isKinematic = true;
                    rbs[i].angularVelocity = 0;
                }
            }
            for(int i = 0; i < hinge.Length; i++){
                hinge[i].enabled = false;
            }
            foreach(rb_test t in rbt){
                t.enabled = true;
            }
        }
        else{
            // starts being ragdoll
            rg = true;
            for(int i = 0; i < rbs.Length; i++){
                if(rbs[i] != rb && rbs[i].tag != "coll"){
                    rbs[i].isKinematic = false;
                    rbs[i].velocity = rb.velocity;
                }
            }
            for(int i = 0; i < hinge.Length; i++){
                hinge[i].enabled = true;
            }
            foreach(rb_test t in rbt){
                t.enabled = false;
                t.transform.localPosition = Vector3.zero;
                t.transform.localEulerAngles = Vector3.zero;
            }
        }
        foreach(arm arms in a){
            arms.rgdoll();
        }
        dequip_checker(0);
        dequip_checker(1); // dequips all held weapons
        rb.isKinematic = false;
    }
}