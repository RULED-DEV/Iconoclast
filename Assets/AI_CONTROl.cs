using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_CONTROl : MonoBehaviour
{
    public GameObject player;
    public pelvis p;
    public torso t;
    public head h;
    bool rg;
    float rg_time;

    public GameObject long_hold;
    public GameObject short_hold;

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
            tae.gameObject.layer = 7; // layer of enemy limbs
        }
        foreach(weapon w in weapons){
            w.INIT(9); // feeds in layer for enemy waepons
            if(w.can_dual){
                w.equip_to_body(long_hold);
            }
            else{
                w.equip_to_body(short_hold);
            }
            
        }

        ragdoll_cont();
    }

    void Update() // significant lag during collisions, most likely ik managers trying to oppose rigidbody physics
    {
        if(!rg){
            COMM_flip();
        }
        COMM_RAG();
    }

    void COMM_flip(){
        Vector3 mousePosition = player.transform.position;
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

    void COMM_RAG(){
        if(rg){
            if(rg_time < Time.time){
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
