using UnityEngine;
using Mirror;

public static class ObjectMover
{
    public static void MoveTo(Transform obj, Vector3 targetPosition)
    {
        if (obj == null) return;

        var net = obj.GetComponent<NetworkIdentity>();
        var sync = obj.GetComponent<NetworkSyncMover>();

        // если сетевого компонента нет — просто двигаем локально
        if (net == null || sync == null)
        {
            
            obj.position = targetPosition;
            return;
        }

        // клиент запрашивает движение у сервера
        if (NetworkClient.active && !NetworkServer.active)
        {
            sync.CmdMove(targetPosition);
            return;
        }

        // сервер двигает и всем рассылает
        if (NetworkServer.active)
        {
            obj.position = targetPosition;
            sync.RpcMove(targetPosition);
            return;
        }

        // fallback
        //obj.position = targetPosition;
    }
}
