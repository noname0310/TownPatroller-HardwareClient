using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjBtnLinker : MonoBehaviour
{
    public Button ConnectButton;

    private void Start()
    {
        ConnectButton.onClick.AddListener(OnConnectBtnClicked);
    }

    public void OnConnectBtnClicked()
    {
        StartCoroutine(ConnectButtonCoolDown());
        GameObject.Find("NetworkManager(Clone)").GetComponent<SocketObj>().OnConnectBtnClicked();
    }

    private IEnumerator ConnectButtonCoolDown()
    {
        ConnectButton.interactable = false;
        yield return new WaitForSeconds(1f);
        ConnectButton.interactable = true;
    }
}
