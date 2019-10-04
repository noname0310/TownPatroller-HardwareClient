using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjAdder : MonoBehaviour
{
    public GameObject NetworkManager;

    void Start()
    {

        if (GameObject.Find("NetworkManager(Clone)") == null)
        {
            Instantiate(NetworkManager);
        }
    }
}
