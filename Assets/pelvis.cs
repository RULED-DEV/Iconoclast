using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pelvis : MonoBehaviour
{

    [Header("movement stats")] // all the movement controls
    
    public float MASS;
    public float speed;
    public float sprint_mult_speed;
    public bool sprint;
    public float leg_speed = 6;
    public float velocity_scale = 0.5f;

    [Header("hieght stats")]

    public float add_hieght = 0.5f; // 0% - 50% - 100%
    public float grav = 10;

    public float springStrength = 5;
    public float damping = 2;

    float MtS;
    float kick_time;

    leg[] legser;

    public string mode; // foreward,backward,idle
    
    public Rigidbody2D rb;

    bool rg = false;

    public void INIT(){
        legser = gameObject.GetComponentsInChildren<leg>();
        for(int i = 0; i < legser.Length; i++){
            legser[i].l = gameObject.GetComponent<pelvis>();
            if(i%2 != 1){
                legser[i].INIT(legser[i+1]); // initialises the legs
                
            }
            else{
                legser[i].INIT(legser[i-1]);
            }
        }
        mode = "idle";
        transform.localPosition = Vector3.zero;
    }

    void Update()
    {
        if(!rg){
            check_legs();
            COMM_hieght();
        }
        grav_manager();
    }

    void check_legs(){
        // moves legs when far away
        for(int i = 0; i < legser.Length; i++){
            legser[i].mode = mode;
            if(legser[i].check()){ // if we have stepped and need to step again
                if(legser[i].paired.STEP){ // || (legser[i].paired.middair && legser[i].middair) // needs middair check
                    leg l = legser[i];
                    l.mode = mode;
                    l.step(false);
                }
            }
        }
    }

    void grav_manager(){
        rb.AddForce(new Vector2(0,-1)*grav*rb.mass); // applies gravity
    }

    void COMM_hieght(){ // more work needed
        float num = 0;
        float avg_hieght = 0;

        float point = 0;

        foreach(leg l in legser){
            if(l.STEP){
                num++;
                avg_hieght += l.desired_hieght;
                point += l.target.transform.position.y;
            }
        }
        if(num > 0){

            point = point / num;
            avg_hieght = avg_hieght / num; // takes mean

            MtS = 0.5f+(num/legser.Length*0.5f); // goes from 50% to 100% depending on feet
            MtS = MtS * add_hieght;

            avg_hieght = avg_hieght*MtS;
            avg_hieght += point;

            float correction = springStrength*(avg_hieght-rb.position.y) - damping*rb.velocity.y;
            if(correction+(grav*rb.mass) > 0){
                rb.AddForce(new Vector2(0,correction+(grav*rb.mass)), ForceMode2D.Force);
            }
        }
    }

    public void kick(){
        if(kick_time < Time.time){
            for(int i = 0; i < legser.Length; i++){
                if(i%2 != 1){
                    // every even num
                    if(legser[i].STEP){
                        legser[i+1].kicking = true;
                        kick_time = Time.time + 1;
                        break;
                    }
                    else if(legser[i+1].STEP){
                        legser[i].kicking = true;
                        kick_time = Time.time + 1;
                        break;
                    }
                }
            }
        }
    }

    public void velocitiser(Vector2 inp){ // moves ya
        // manages velocity
        float pract_speed = 0;
        float num = 0;
        foreach(leg l in legser){
            if(l.STEP){
                num++;
                pract_speed += speed;
            }
        }
        inp = inp*(pract_speed/num);
        if(sprint){inp = inp*sprint_mult_speed;}
        if(float.IsNaN(inp.x)){
            inp = Vector2.zero;
        }
        if(rb != null){
            rb.AddForce(inp);
        }
    }

    public void rgdoll(){
        HingeJoint2D hinge = transform.GetComponent<HingeJoint2D>();
        Rigidbody2D rbs = gameObject.GetComponent<Rigidbody2D>();
        foreach(leg le in legser){le.rgdoll();}
        if(rg){
            // stops being ragdoll
            transform.localPosition = Vector3.zero;
            transform.localEulerAngles = Vector3.zero;
            rg = false;
            mode = "idle";
            rbs.isKinematic = true;
            rbs.angularVelocity = 0;
            hinge.enabled = false;
        }
        else{
            // starts being ragdoll
            rg = true;
            rbs.isKinematic = false;
            rbs.velocity = rb.velocity;
            hinge.enabled = true;
        }
    }

    public void remove_limb(GameObject br){
        br.transform.parent = null;
        HingeJoint2D hinge = br.GetComponent<HingeJoint2D>();
        hinge.enabled = false;
    }
}