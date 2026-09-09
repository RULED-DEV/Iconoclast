using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class weapon : MonoBehaviour
{
    public List<arm> grabbed = new List<arm>();

    public GameObject[] grab_points_main;
    public GameObject[] grab_points_alt;

    public GameObject base_point; 

    public bool alt;

    Vector2 flip_vals = new Vector2(1,1);

    public bool can_dual;

    public Rigidbody2D rb;

    bool can_thrust = true;
    float thr_time;

    [Header("hold pos Settings")] // this adds a header :D

    public float depression;

    public float rest_dist;
    public float rest_depression; // these dont work, wix pls
    public float rest_ang;

    public void INIT(int inp){
        rb = gameObject.GetComponent<Rigidbody2D>();
        Collider2D[] col = gameObject.GetComponentsInChildren<Collider2D>();
        foreach(Collider2D c in col){
            c.gameObject.layer = inp;
        }
    }

    public void thrust_man(Vector3 t){
        if(thr_time < Time.time){
            can_thrust = true;
        }
        if(can_thrust){
            foreach(arm a in grabbed){
                a.thrust_cont(t);
            }
            can_thrust = false;
            thr_time = Time.time + 1;
        }
    }

    public void swap_grip(){
        alt = !alt;
        foreach(arm a in grabbed){
            a.equipped = null; // remove weapon from arm
            a.hold_point = null;
            equip_to_wep(a);
        }
    }

    public void flip(Vector2 inp,float x){ // 1 keeps it the same -1 flips it
        if(inp.x != 0){flip_vals.x = -flip_vals.x;}
        if(inp.y != 0){flip_vals.y = -flip_vals.y;}
        transform.localScale = new Vector3(x*flip_vals.x,1*flip_vals.y,1);
    }

    public void wepcontrol(GameObject t, string type){
        Vector3 point = Vector3.zero;
        Vector3 phys_point = Vector3.zero;
        float arm_len = 0;
        float tot = 0;
        float d = 0;
        float avg = 0;
        foreach(arm g in grabbed){
            arm_len = g.arm_len;
            if(g.arming){tot += g.hold_point.transform.localEulerAngles.z;d++;}
            g.arming = false;

            if(g.hold_point != null){
                point += g.hold_point.transform.position;
                phys_point += g.hold_point.transform.localPosition;
            }
        }

        point = point/grabbed.Count;
        phys_point = phys_point/grabbed.Count;

        tot = tot / d; // average of all active arm angles
        rb.centerOfMass = base_point.transform.localPosition;
        if(type == "att"){
            base_point.transform.localEulerAngles = new Vector3(0,0,tot);
            t.transform.localPosition = new Vector3(depression,arm_len/2,0); // subtract dist from hold points
        }
        else{
            base_point.transform.localEulerAngles = new Vector3(0,0,0);
            t.transform.localPosition = new Vector3(depression,arm_len/2,0); // subtract dist from hold points
        }

        grabbed[0].aimer(t.transform.position,base_point,t,point,grabbed.Count);
    }

    // public void restwep(GameObject t){
    //     Vector3 point = Vector3.zero;
    //     Vector3 phys_point = Vector3.zero;
    //     float arm_len = 0;
    //     float tot = 0;
    //     float d = 0;
    //     float avg = 0;
    //     foreach(arm g in grabbed){
    //         arm_len = g.arm_len;
    //         if(g.arming){tot += g.hold_point.transform.localEulerAngles.z;d++;}
    //         g.arming = false;

    //         if(g.hold_point != null){
    //             point += g.hold_point.transform.position;
    //             phys_point += g.hold_point.transform.localPosition;
    //         }
    //     }

    //     point = point/grabbed.Count;
    //     phys_point = phys_point/grabbed.Count;

    //     rb.centerOfMass = phys_point;
    //     base_point.transform.localEulerAngles = new Vector3(0,0,0);
    //     t.transform.localPosition = new Vector3(depression,arm_len/2,0); // subtract dist from hold points

    //     grabbed[0].aimer(t.transform.position,base_point,t,point,grabbed.Count);
    // }

    public void equip_to_wep(arm a){
        if(a.equipped != null){
            a.equipped.grabbed.RemoveAt(a.equipped.grabbed.IndexOf(a)); // remove arm from weapon
        }
        a.equipped = gameObject.GetComponent<weapon>(); // adds weapon from arm
        if(!grabbed.Contains(a)){gameObject.GetComponent<weapon>().grabbed.Add(a);}// adds arm to weapon
        GameObject[] grabs = grab_points_main;
        if(alt){grabs = grab_points_alt;}
        a.equip_check = true; // sets arm to be busy and equip weapon
        transform.parent = null;
        foreach(GameObject o in grabs){
            bool ch = true;
            foreach(arm e in grabbed){if(e.hold_point == o){ch = false;}}
            if(ch){ // doesnt work
                a.hold_point = o;
                break;
            }
        }
    }

    public void brought_to_hand(){
        rb.simulated = true;
        rb.isKinematic = false;
    }

    public void equip_to_body(GameObject g){
        transform.parent = g.transform;
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = Vector3.zero; // change to modifiable variable

        rb.isKinematic = true;
        rb.simulated = false;
    }
}
