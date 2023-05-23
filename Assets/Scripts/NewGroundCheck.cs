using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewGroundCheck : MonoBehaviour
{
    public static bool _isPlayerGrounded;
    public GameObject _player;
    //here we use a layer instead of a tag because it will give us more controll over thins like pataforms(which we want to jump on but will have the tag "plataform");
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 7) _isPlayerGrounded = true;

        if (collision.CompareTag("Death")) _player.GetComponent<NewCharacterController>().Die();
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
       
        if (collision.gameObject.layer == 7) _isPlayerGrounded = false;

    }
}
