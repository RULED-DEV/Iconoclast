using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.IK;

public class arm : MonoBehaviour
{

    bool rg;

    public GameObject target;
    public GameObject hand;

    public weapon equipped;

    public bool equip_check;
    public GameObject hold_point;

    public float speed = 5;
    public float MOVE_force = 5;
    public float positionDamper;
    public float ROT_force = 5;
    public float counter_force = 0.5f;

    public float thrust_str = 5;

    public float arm_len;
    public bool arming;

    public void INIT()
    {
        target.transform.parent = null;
    }

    void Update(){
        if(!rg){
            if(equip_check){
                equip();
            }
            else{
                if(equipped != null){
                    //eq_check();
                }
                // arm fighty stuff
            }
        }
    }

    void eq_check(){
        if(equipped != null && !equip_check){
            Vector3 p = equipped.GetComponentInChildren<Collider2D>().ClosestPoint(hand.transform.position);
            if(Vector3.Distance(hand.transform.position,p) > 0.1f){ // checks if hand is near any point of the haft
                Invoke("eq",Time.deltaTime*17); // triggers after 17 frames
            }
        }
    }

    void eq(){
        if(equipped != null && !equip_check){
            if(Vector3.Distance(hand.transform.position,hold_point.transform.position) > 0.1f){
                // dequip
                equipped.grabbed.RemoveAt(equipped.grabbed.IndexOf(gameObject.GetComponent<arm>())); // remove arm from weapon
                equipped = null; // remove weapon from arm
                hold_point = null;
                target.transform.parent = null;
            }
        }
    }

    void equip(){
        if(Vector3.Distance(target.transform.position,hold_point.transform.position) < 0.1f){
            // final dequip
            target.transform.parent = hold_point.transform; // sets position
            target.transform.localPosition = Vector3.zero;
            target.transform.localEulerAngles = Vector3.zero;
            if(!equipped.rb.simulated){
                equipped.brought_to_hand();
            }
            equip_check = false;
        }
        else{
            // move toward
            target.transform.position = Vector3.Lerp(target.transform.position,hold_point.transform.position,Time.deltaTime*speed);
        }
    }

    public void thrust_cont(Vector3 tar){
        Vector2 vect = (Vector2)tar-(Vector2)equipped.transform.position;
        vect = new Vector2(Mathf.Clamp(vect.x,-1,1),Mathf.Clamp(vect.y,-1,1));
        equipped.rb.AddForce(vect*thrust_str,ForceMode2D.Impulse);
    }

    public void aimer(Vector3 pos, GameObject orientation, GameObject refe, Vector3 point,float str_mult){
        // pos is where we want it
        // point is where we are

        // orientation is our angle
        // refe is out target angle

        // orientation is used with ref so that they line up
        Rigidbody2D rb = equipped.rb;
        // position nonsense
            Vector2 positionError = (Vector2)pos - (Vector2)point;
            Vector2 velocityError = Vector2.zero - rb.velocity;

            Vector2 forcepos = (positionError * (MOVE_force*rb.mass)*str_mult) + (velocityError * positionDamper);
            forcepos = Vector2.ClampMagnitude(forcepos, 1000);

            // Counteract gravity so the PD controller isn't fighting a constant offset
            Vector2 gravityCompensation = -Physics2D.gravity * rb.gravityScale * rb.mass;
            forcepos += gravityCompensation;

            rb.AddForce(forcepos, ForceMode2D.Force);

        // rotation nonsense // needs to be weightier, chnge back pls
            float wep_ang = orientation.transform.eulerAngles.z;
            float tar_ang = refe.transform.eulerAngles.z;
            if(Mathf.Abs(wep_ang-tar_ang) > 180){ // if they are not on the same side
                if(wep_ang < 180 && tar_ang > 180){
                    tar_ang -= 360;
                }
                else{
                    wep_ang -= 360;
                }
            }
            int dir = 1; // direction we must rotate in
            if(tar_ang < wep_ang){dir = -1;}

            float dist = Vector2.Angle(orientation.transform.up,refe.transform.up);
            float vel = Mathf.Abs(rb.angularVelocity);

            float rf = ROT_force*str_mult;

            // max_rot is our max rot speed currently
            float max_rot = 4500;
            float predict_speed = max_rot/((rf*(1/Time.deltaTime))+vel); // time until cruise is met
            float predict_time = dist/vel; // est time to end ang
            float predict_slow = vel/(rf*(1/Time.deltaTime)); // predicted time to 0

            float force = 0;

            if(predict_time <= predict_slow){ // slowdown
                force -= rf*dir;
            }
            else{
                if(vel+rf < max_rot){ // accelerate
                    force += rf*dir;
                }
                else if(vel > max_rot){ // meet speed
                    force += ((vel+rf)-max_rot)*dir;
                }
            }

            rb.AddTorque(force);

            // float angleDiff = Mathf.DeltaAngle(orientation.transform.eulerAngles.z,refe.transform.eulerAngles.z);

            // // Proportional term: pushes toward the target angle
            // float springTorque = angleDiff * (ROT_force);

            // // Derivative term: resists current angular velocity to prevent overshoot/oscillation
            // float dampingTorque = -rb.angularVelocity * counter_force;

            // float torque = springTorque + dampingTorque;
            // torque = Mathf.Clamp(torque, -max_rot0, max_rot0);

            // rb.AddTorque(torque);
    }

    public void tar_pos(Vector3 pos){
        target.transform.position = Vector3.Lerp(target.transform.position,pos,Time.deltaTime*speed);
    }

    public void rgdoll(){ // arms break when ragdolling
        IKManager2D[] man = GetComponentsInChildren<IKManager2D>();
        HingeJoint2D[] hinge = GetComponentsInChildren<HingeJoint2D>();
        Collider2D[] col = GetComponentsInChildren<Collider2D>();
        Rigidbody2D[] rbs = GetComponentsInChildren<Rigidbody2D>();
        if(rg){
            // stops being ragdoll
            transform.localPosition = Vector3.zero;
            transform.localEulerAngles = Vector3.zero;
            rg = false;
            for(int i = 0; i < rbs.Length; i++){
                if(rbs[i].tag != "coll"){
                    rbs[i].bodyType = RigidbodyType2D.Kinematic;
                    rbs[i].angularVelocity = 0;
                }
            }
            for(int i = 0; i < man.Length; i++){
                man[i].enabled = true;
            }
            for(int i = 0; i < hinge.Length; i++){
                hinge[i].enabled = false;
            }
            for(int i = 0; i < col.Length; i++){
                col[i].isTrigger = true;
            }
        }
        else{
            // starts being ragdoll
            rg = true;
            for(int i = 0; i < rbs.Length; i++){
                rbs[i].bodyType = RigidbodyType2D.Dynamic;
            }
            for(int i = 0; i < man.Length; i++){
                man[i].enabled = false;
            }
            for(int i = 0; i < hinge.Length; i++){
                hinge[i].enabled = true;
            }
            for(int i = 0; i < col.Length; i++){
                col[i].isTrigger = false;
            }
            // deequips weapons
        }
    }
}
