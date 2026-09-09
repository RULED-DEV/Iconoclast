using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CONTROL : MonoBehaviour
{

    public pelvis p;
    public torso t;
    public head h;
    bool rg;
    float rg_time;

    public GameObject long_hold;
    public GameObject short_hold;

    public GameObject aimer;

    public Camera cam;

    public Rigidbody2D rb;

    public List<weapon> weapons = new List<weapon>(); // list of unequipped weapons

    void Awake(){INIT();}

    void INIT(){
        p = GetComponentInChildren<pelvis>();
        t = GetComponentInChildren<torso>();
        p.rb = rb;
        p.INIT();
        t.INIT();
        h.INIT();
        Transform[] te = GetComponentsInChildren<Transform>();
        foreach(Transform tae in te){
            tae.gameObject.layer = 6; // layer of enemy limbs
        }
        foreach(weapon w in weapons){
            w.INIT(8); // feeds in layer of player weapons
            if(w.can_dual){
                w.equip_to_body(long_hold);
            }
            else{
                w.equip_to_body(short_hold);
            }
            
        }

        ragdoll_cont();
        ragdoll_cont();
    }

    // controls
    // movement - A-D
    // control right - lmb
    // control left - rmb
    // equip - q
    // dequip - e
    // cast - f
    // kick - r
    // stab - space
    // translate - x
    // swap grips - z

    void Update()
    {
        if(!rg){
            COMM_KICK();
            COMM_move();
            COMM_flip();
            COMM_equip();
            COMM_deequip(); 
            COMM_translate();
            COMM_grip();
            COMM_aim();
            COMM_thrust();
            // might be good to consider making these more dynamic if lmb/rmb are not pressed
        }
        COMM_camera();
        COMM_RAG();
    }

    void COMM_thrust(){
        Vector2 offset = Vector2.zero;
        if(Input.GetMouseButton(0)){offset.x = 1;}
        if(Input.GetMouseButton(1)){offset.y = 1;}
        if(offset != Vector2.zero){
            if(Input.GetKey(KeyCode.Space)){
                Vector3 mousePosition = Input.mousePosition;
                mousePosition = cam.ScreenToWorldPoint(mousePosition);
                t.thruster(offset,mousePosition);
            }
        }
    }

    void COMM_translate(){
        // swaps weapon angle
        Vector2 inp = Vector2.zero;
        if(Input.GetMouseButton(0)){inp.x = 1;}
        if(Input.GetMouseButton(1)){inp.y = 1;}
        if(inp != Vector2.zero){
            if(Input.GetKeyDown(KeyCode.X)){
                List<weapon> weps = new List<weapon>(); 
                foreach(arm a in t.a){
                    if(a.equipped != null && !weps.Contains(a.equipped)){
                        weps.Add(a.equipped);
                    }
                }
                foreach(weapon w in weps){
                    w.flip(inp,transform.localScale.x);
                }
            }
        }
    }

    void COMM_grip(){
        Vector2 inp = Vector2.zero;
        if(Input.GetMouseButton(0)){inp.x = 1;}
        if(Input.GetMouseButton(1)){inp.y = 1;}
        if(inp != Vector2.zero){
            if(Input.GetKeyDown(KeyCode.Z)){
                List<weapon> weps = new List<weapon>(); 
                for(int i = 0; i < t.a.Length; i++){
                    if((inp.x != 0 && i%2 == 0) || (inp.y != 0 && i%2 == 1)){
                        if(t.a[i].equipped != null && !weps.Contains(t.a[i].equipped)){
                            weps.Add(t.a[i].equipped);
                        }
                    }
                }
                foreach(weapon w in weps){
                    w.swap_grip();
                }
            }
        }
    }

    void COMM_aim(){
        Vector2 offset = Vector2.zero;
        if(Input.GetMouseButton(0)){offset.y = 1;}
        if(Input.GetMouseButton(1)){offset.x = 1;}
        if(offset != Vector2.zero){
            Vector3 mousePosition = Input.mousePosition;
            mousePosition = cam.ScreenToWorldPoint(mousePosition);
            aimer.transform.up = mousePosition - aimer.transform.position;
            aimer.transform.eulerAngles = new Vector3(0,0,aimer.transform.eulerAngles.z);
            aimer.transform.GetChild(0).localPosition = Vector3.zero; 
            aimer.transform.GetChild(0).localEulerAngles = Vector3.zero;
        }
        
        t.aim_organiser(offset,aimer.transform.GetChild(0).gameObject); // aimers distance from body is assigned here
    }

    void COMM_equip(){
        int offset = -1;
        if(Input.GetMouseButton(0)){offset = 0;}
        if(Input.GetMouseButton(1)){offset = 1;}
        if(Input.GetKeyDown(KeyCode.Q)){
            weapon w = null;
            if(weapons.Count > 0){w = weapons[0];}
            if(w != null){
                bool c = t.equip_checker(offset,w);
                if(c){weapons.RemoveAt(0);}
            }
        }
    }

    void COMM_deequip(){
        int offset = -1;
        if(Input.GetMouseButton(0)){offset = 0;}
        if(Input.GetMouseButton(1)){offset = 1;}
        if(Input.GetKeyDown(KeyCode.E)){
            t.dequip_checker(offset);
        }
    }

    void COMM_KICK(){
        if(Input.GetKey(KeyCode.R)){
            p.kick();
        }
    }

    void COMM_camera(){
        cam.transform.position = Vector3.Lerp(cam.transform.position,transform.position,Time.deltaTime*2);
        cam.transform.position = new Vector3(cam.transform.position.x,cam.transform.position.y,-10);
    }

    void COMM_flip(){
        Vector3 mousePosition = Input.mousePosition;
        mousePosition = cam.ScreenToWorldPoint(mousePosition);
        if(mousePosition.x > transform.position.x && transform.localScale.x != 1){
            transform.localScale = new Vector3(1,1,1);
            leg[] l = GetComponentsInChildren<leg>();
            foreach(leg le in l){
                le.flip();
            }
            foreach(arm a in t.a){
                if(a.equipped != null){
                    a.equipped.flip(Vector2.zero,1);
                }
            }
        }
        if(mousePosition.x < transform.position.x && transform.localScale.x != -1){
            transform.localScale = new Vector3(-1,1,1);
            leg[] l = GetComponentsInChildren<leg>();
            foreach(leg le in l){
                le.flip();
            }
            foreach(arm a in t.a){
                if(a.equipped != null){
                    a.equipped.flip(Vector2.zero,-1);
                }
            }
        }
    }

    void COMM_move(){
        Vector2 inp_vect = Vector2.zero;
        p.sprint = false;
        if(Input.GetKey(KeyCode.LeftShift)){p.sprint = true;}
        if(Input.GetKey(KeyCode.A)){
            inp_vect = -Vector2.right;
            if(transform.localScale.x > 0){
                p.mode = "backward";
            }
            else{
                p.mode = "foreward";
            }
        }
        if(Input.GetKey(KeyCode.D)){
            inp_vect = Vector2.right;
            if(transform.localScale.x > 0){
                p.mode = "foreward";
            }
            else{
                p.mode = "backward";
            }
        }
        if(inp_vect != Vector2.zero){
            p.velocitiser(inp_vect);
        }
    }

    void COMM_RAG(){
        if(!rg){
            if(Input.GetKeyDown(KeyCode.K)){
                ragdoll_cont();
            }
        }
        else{
            if(Input.anyKeyDown && rg_time < Time.time){
                ragdoll_cont();
            }
        }
        
        if(t.rgdoll_flag){
            t.rgdoll_flag = false;
            ragdoll_cont();
        }
    }

    void ragdoll_cont(){
        rg_time = Time.time + 3;
        rg = !rg;
        p.rgdoll();
        t.rgdoll();
        h.rgdoll();
    }
}
