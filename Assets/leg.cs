using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.IK;

public class leg : MonoBehaviour
{

    public pelvis l;
    public LayerMask mask;
    public GameObject target;
    public GameObject foot;
    public Vector3 foot_pos = Vector3.zero; // where we want the foot to go
    public string mode; // "foreward","backwar","idle"
    
    [Header("step stats")] 

    public float idle_pos_x;
    public float step_pos_x;
    public float backstep_pos_x;
    public float step_dist;

    public float step_pos_y;

    [Header("leg stats")] 

    public float max_length;
    public float desired_hieght;
    
    public float limb_length;

    int dir = -1; // this is where we face
    int orientation = 1; // this is what side the limb is on

    public bool kicking;

    public float kick_mult;
    bool raise_check = false;
    float raise_dist;
    
    public bool middair;
    public bool STEP = false;

    float leg_speed_mult = 1;

    public leg paired;
    bool rg;



    public void INIT(leg p)
    {
        paired = p;
        mode = "idle";
        target.transform.parent = null;
        step(false);
    }

    void Update()
    {
        step_manager();
    }

    void step_manager(){
        // move target to target pos 
        if(raise_check){
            if(raise_dist+step_pos_y < target.transform.position.y){
                raise_check = false;
                foot_pos.y -= step_pos_y;
            }
        }
        else{
            if(Vector3.Distance(target.transform.position,foot_pos) < 0.05f){
                STEP = true;
            }
        }

        if(Vector3.Distance(target.transform.position,foot_pos) > max_length*2){
            leg_speed_mult = Mathf.Lerp(leg_speed_mult,2,Time.deltaTime); // doubles speed
        }
        else{
            leg_speed_mult = Mathf.Lerp(leg_speed_mult,1,Time.deltaTime);
        }

        float v_mult = Mathf.Abs((l.rb.velocity.x*l.velocity_scale))+1;
        float speed = (l.leg_speed*leg_speed_mult);
        if(kicking){
            STEP = false;
            middair = true;
            Vector3 kick_vect = new Vector3((max_length)*orientation,0,0) + transform.position;
            target.transform.position = Vector3.Lerp(target.transform.position,kick_vect,(Time.deltaTime*speed*kick_mult)*v_mult);
            if(Vector3.Distance(target.transform.position,kick_vect) < 0.05f){
                kicking = false;
            }
        }
        else{
            target.transform.position = Vector3.Lerp(target.transform.position,foot_pos,(Time.deltaTime*speed)*v_mult);
        }
        
    }

    float get_step_offset()
    {
        switch (mode)
        {
            case "foreward": return step_pos_x;
            case "backward": return backstep_pos_x;
            default:         return 0f; // idle
        }
    }
 
    public void step(bool inst) // still fucked, FIX
    {
        STEP = inst;
        Rigidbody2D rb = l.rb;

        float world_target_x = transform.position.x + ((idle_pos_x + get_step_offset()) * -dir);

        float world_body_x = transform.position.x; // our position

        float decayFactor = ((rb.velocity / rb.drag) * l.velocity_scale).x;
        world_target_x += decayFactor; // supposedly this should predict us
        
        float range_min = world_body_x;
        float range_max = world_target_x;

        const int RAY_COUNT = 11;

        Vector3 fallback = Vector3.zero;
 
        for (int i = 0; i < RAY_COUNT; i++)
        {
            float d = 1;
            if(mode == "backward"){d = -1;}
            // Fan rays outward in dir, sweeping downward as i increases
            float ray_x = (orientation*d) - ((orientation*d) * 0.1f * i);
            float ray_y = -0.1f * i;
  
            RaycastHit2D hit = Physics2D.Raycast(transform.position, new Vector2(ray_x, ray_y), max_length, mask);

            if(hit.collider != null){
                bool in_range = false;
                //in_range = hit.point.x >= range_min && hit.point.x <= range_max;
                float dist_1 = Mathf.Abs(range_min-range_max); // distance between body and max range
                float dist_2 = Mathf.Abs(range_min-hit.point.x); // distance between body and hit point
                in_range = dist_2 <= dist_1;
                if (!in_range) // it is in the valid range
                    continue;

                float normal_angle = Vector2.Angle(hit.normal, Vector2.up);
                bool  is_floor     = normal_angle < 90f;
                if (is_floor)
                {
                    place_foot(hit.point, liftFoot: !inst);
                    middair = false;
                    return;
                }
        
                // Wall — only place foot here if climbing is allowed and it's closer to the body
                else{
                    fallback = hit.point;
                }
            }
        }
        if(fallback != Vector3.zero){ // if no valid floor pos is found it will try to grab onto the fall before the floor
            STEP = true;
            middair = false;
            place_foot(fallback,liftFoot: false);
        }
        else{
            STEP = false;
            middair = true;
            place_foot(new Vector2(transform.position.x,transform.position.y-max_length), liftFoot:false);
        }
    }
 
    // Sets foot_pos to pos. If liftFoot is true, arcs the foot up by step_pos_y first.
    void place_foot(Vector2 pos, bool liftFoot)
    {
        foot_pos    = pos;
        raise_check = liftFoot;
        if (liftFoot)
        {
            // distance from body
            raise_dist = foot_pos.y;
            foot_pos.y  += step_pos_y * 1.1f;
        }
    }

    public bool check()
    {
        if(kicking){return false;} // stops step when kicking
        if(middair){return true;}
        // Has the foot drifted too far from its idle X position?
        float world_idle_x = (idle_pos_x*-dir) + transform.position.x;
        
        if (Mathf.Abs(world_idle_x - target.transform.position.x) > step_dist){
            return true;
        }

        // Is the foot further than the leg can reach?
        if (Vector2.Distance(transform.position, target.transform.position) > max_length){
            return true;
        }
 
        // Is the line to the foot obstructed (e.g. leg clipping through a wall)?
        RaycastHit2D hit = Physics2D.Raycast(transform.position, foot_pos - transform.position, max_length, mask);
        if (hit.collider == null){
            return true;
        }

        // hit = Physics2D.Raycast(foot.transform.position,-foot.transform.up , 0.1f, mask);
        // if(hit.collider == null){
        //     return true; // foot not actually on ground
        // }
 
        // need a check to see if leg is lower than us to help climbing
        //if(STEP && Mathf.Abs(target.transform.position.y-transform.position.y) > max_length/2)
            //return true;
        if(STEP){
            if(target.transform.position.y > transform.position.y){
                return true; // foot is above head, major social faux pas
            }
        }

        return false;
    }

    public void flip(){
        dir = -dir;
        orientation = -orientation;
        mode = "idle";
        step(true);
    }

    public void rgdoll(){ // arms break when ragdolling
        Rigidbody2D[] rbs = GetComponentsInChildren<Rigidbody2D>();
        IKManager2D[] man = transform.GetComponentsInChildren<IKManager2D>();
        HingeJoint2D[] hinge = transform.GetComponentsInChildren<HingeJoint2D>();
        //Collider2D[] col = GetComponentsInChildren<Collider2D>();
        if(rg){
            // stops being ragdoll
            rg = false;
            mode = "idle";
            for(int i = 0; i < rbs.Length; i++){
                if(rbs[i].tag != "coll"){
                    rbs[i].isKinematic = true;
                    rbs[i].angularVelocity = 0;
                }
            }
            for(int i = 0; i < man.Length; i++){
                man[i].enabled = true;
            }
            for(int i = 0; i < hinge.Length; i++){
                hinge[i].enabled = false;
            }
            step(true);
        }
        else{
            // starts being ragdoll
            STEP = false;
            middair = true;
            rg = true;
            for(int i = 0; i < rbs.Length; i++){
                rbs[i].isKinematic = false;
            }
            for(int i = 0; i < man.Length; i++){
                man[i].enabled = false;
            }
            for(int i = 0; i < hinge.Length; i++){
                hinge[i].enabled = true;
            }
        }
    }

    public void remove_limb(GameObject br){
        br.transform.parent = null;
        HingeJoint2D hinge = br.GetComponent<HingeJoint2D>();
        hinge.enabled = false;
        STEP = false;
    }
}
