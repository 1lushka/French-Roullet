using UnityEngine;
using Mirror;

public class NetworkSyncMover : NetworkBehaviour
{
    [Command(requiresAuthority = false)]
    public void CmdMove(Vector3 newPos)
    {
        print("iijnhn");
        // —ервер обновл€ет позицию
        transform.position = newPos;
        // —ообщаем всем клиентам
        RpcMove(newPos);
    }

    [ClientRpc]
    public void RpcMove(Vector3 newPos)
    {
        print("iijnhn");
        if (isServer) return; // сервер уже поставил позицию
        transform.position = newPos;
    }
}
