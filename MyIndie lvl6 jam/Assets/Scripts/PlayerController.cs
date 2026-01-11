using Mirror;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private GameObject typeA;
    [SerializeField] private GameObject typeB;
    [SerializeField] private GameObject[] cameras;

    private void Start()
    {
        typeA.SetActive(false);
        typeB.SetActive(false);
        if (!isLocalPlayer)
        {
            for (int i = 0; i < cameras.Length; i++)
            {
                cameras[i].gameObject.SetActive(false);
            }
        }
    }

    public void OnSelectTypeA()
    {
        if (!isLocalPlayer) return;
        CmdSetPlayerType("A");
    }

    public void OnSelectTypeB()
    {
        if (!isLocalPlayer) return;
        CmdSetPlayerType("B");
    }

    [Command]
    private void CmdSetPlayerType(string type)
    {
        RpcSetPlayerType(type);
    }

    [ClientRpc]
    private void RpcSetPlayerType(string type)
    {
        typeA.SetActive(type == "A");
        typeB.SetActive(type == "B");
    }
}
