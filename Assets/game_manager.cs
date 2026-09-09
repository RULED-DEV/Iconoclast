using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class game_manager : MonoBehaviour
{
    public GameObject player;

    public GameObject[] enemy;
    public GameObject test_pref_ene;

    public Camera cam;

    void Update(){
        if(Input.GetKeyDown(KeyCode.Alpha1)){
            Vector3 mousePosition = Input.mousePosition;
            mousePosition = cam.ScreenToWorldPoint(mousePosition);
            mousePosition = new Vector3(mousePosition.x,mousePosition.y,0);
            Instantiate(test_pref_ene,mousePosition,transform.rotation).GetComponent<AI_CONTROl>().player = player;
        } 
        if(Input.GetKey(KeyCode.Escape)){
            Application.Quit();
        }
    }
}
