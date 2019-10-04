using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjBtnLinker : MonoBehaviour
{
    public void OnConnectBtnClicked()
    {
        GameObject.Find("NetworkManager(Clone)").GetComponent<SocketObj>().OnConnectBtnClicked();
    }
}
