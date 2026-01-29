using Mirror;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [Header("Player Types")]
    [SerializeField] private GameObject typeA;
    [SerializeField] private GameObject typeB;

    [Header("Animators")]
    [SerializeField] private Animator typeAHandAnimator;
    [SerializeField] private Animator typeBHandAnimator;

    [Header("Cameras")]
    [SerializeField] private GameObject[] cameras;

    private string currentType = "";

    // =============================
    // LOCAL PLAYER INIT
    // =============================
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        GameManager.Instance.RegisterLocalPlayer(this);
    }



    private void Start()
    {
        typeA.SetActive(false);
        typeB.SetActive(false);

        if (!isLocalPlayer)
        {
            foreach (var cam in cameras)
                cam.SetActive(false);
        }
    }

    // =============================
    // TYPE SELECTION
    // =============================
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
        currentType = type;

        typeA.SetActive(type == "A");
        typeB.SetActive(type == "B");
    }

    // =============================
    // HAND ANIMATION
    // =============================
    public void PlayHandAnimation()
    {
        if (!isLocalPlayer) return;

        Animator anim = GetCurrentAnimator();

        if (anim == null) return;

        anim.SetTrigger("Press");
    }

    private Animator GetCurrentAnimator()
    {
        switch (currentType)
        {
            case "A":
                return typeAHandAnimator;

            case "B":
                return typeBHandAnimator;

            default:
                return null;
        }
    }

    // =============================
    // NETWORK SYNC (OPTIONAL)
    // =============================
    [Command]
    public void CmdPlayHandAnim()
    {
        RpcPlayHandAnim();
    }

    [ClientRpc]
    private void RpcPlayHandAnim()
    {
        Animator anim = GetCurrentAnimator();

        if (anim != null)
            anim.SetTrigger("Press");
    }
}
