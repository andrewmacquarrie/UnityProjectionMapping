using UnityEngine;
using System.Collections;

public class ProjectionController : MonoBehaviour {
    void Start () {
	
	}
	
	void Update () {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Projector p = GetComponent<Projector>();
            p.enabled = !p.enabled;
        }
	}
}
