using Mirror;
using UnityEngine;

public class RoleSelectionUI : NetworkBehaviour
{
    PlayerController player;
    public void OnClickStartAsExecutioner()
    {
        
            print("sasasa");
            player = FindAnyObjectByType<PlayerController>();
            player.OnSelectTypeA();
        
    }

    public void OnClickStartAsPrisoner()
    {
        
            player = FindAnyObjectByType<PlayerController>();
            player.OnSelectTypeB();
        
    }
}
