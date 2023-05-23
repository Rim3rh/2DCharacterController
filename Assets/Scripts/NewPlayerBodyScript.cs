using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewPlayerBodyScript : MonoBehaviour
{
    private NewCharacterController _player;
    void Start()
    {
        _player = GetComponentInParent<NewCharacterController>();
    }
    public void PlayerDie()
    {
        _player.Die();
    }
    public void PlayerToInvisible()
    {
        this.gameObject.SetActive(false);
    }
    public void PlayerRevive()
    {
        _player._dead = false;
    }

}
