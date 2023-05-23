using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewCharacterController : MonoBehaviour
{
    private PlayerInputActions _playerInputActions;
    private Rigidbody2D _rb;
    public GameObject _playerBody;
    private Animator _anim;
    private float _acceleration, _airAcceleration, _deceleration,_maxSpeed, _jumpForce, _airPull, _hMovement, _fallMultiplier;
    private float _coyoteTime, _coyoteTimer, _bufferTime, _bufferTimer;
    private bool _facinRight, _isJumpingUp, _isJumpingDown, _canDash, _isDashing;
    private float _dashDuration, _dashingCd, _dashForce;
    //we make _dead public so we can acces from the playerBodyScript(ADITIONAL)
    public bool _dead;
    //_changin direcction will be true if your are moviung backwards, but your input is moving you straigth or if the opposite is happening.
    private bool _changingDirection => (_rb.velocity.x < 0 && _hMovement > 0) || (_rb.velocity.x > 0 && _hMovement < 0);
    void Start()
    {
        //New Input System Shit:)
        _playerInputActions = new PlayerInputActions();
        _playerInputActions.Player.Enable();
        _playerInputActions.Player.Dash.started += Dash_started;
        _playerInputActions.Player.Jump.started += Jump_started;
        //Getting components
        _rb = GetComponent<Rigidbody2D>();
        _anim = _playerBody.GetComponent<Animator>();
        //Movement variables(adjustable values)
        _facinRight = true;
        _canDash = true;
        _dead = false;
        _acceleration = 50f;
        _airAcceleration = 20f;
        _maxSpeed = 12f;
        _deceleration = 20f;
        _jumpForce = 20f;
        _airPull = 2.5f;
        _fallMultiplier = 5f;
        _dashForce = 40f;
        _dashingCd = 2f;
        _dashDuration = 0.3f;
        //Jump anticipation/delay settings
        _coyoteTime = 0.15f;
        _bufferTime = 0.15f;
    }
    void Update()
    {
        if (!_isDashing && !_dead)
        {
            //Getting horizontal Input from the new input system
            _hMovement = _playerInputActions.Player.Movement.ReadValue<Vector2>().x;
            //Calling the differrent methods
            MoveCharacter();
            WhereAmIFacing();
            JumpUpgrades();
            FallMiltiplier();
            Animations();
        }
    }
    private void Jump_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        //This will execute whenever you hit the jump button
        Jump();
    }
    private void Dash_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        //This will execute whenever you hit the dash button
        StartCoroutine(Dash());
    }
    private void MoveCharacter()
    {
        //MOVEMENT
        //Add the force so the player has an acceleration instaid of getting to full speed directly
        if(NewGroundCheck._isPlayerGrounded) _rb.AddForce(new Vector2(_hMovement * _acceleration, 0f));
        else _rb.AddForce(new Vector2(_hMovement * _airAcceleration, 0f));

        // Since addForce does not limit the speed, we need to limit it.
        if (Mathf.Abs(_rb.velocity.x) > _maxSpeed) _rb.velocity = new Vector2(Mathf.Sign(_rb.velocity.x) * _maxSpeed, _rb.velocity.y);

        //DRAG
        if (NewGroundCheck._isPlayerGrounded)
        {
            //if the player stops moving we want the drag to be= to the deceleration.
            if (_hMovement == 0) _rb.drag = _deceleration;
            //we also want it to aply if we are changing direcctions.
            if (Mathf.Abs(_hMovement) < 0.4 || _changingDirection) _rb.drag = _deceleration;
            else _rb.drag = 0f;
        }
        else _rb.drag = _airPull;
    }
    private void WhereAmIFacing()
    {
        if (_hMovement < 0)
        {
            transform.eulerAngles = new Vector3(0, 180);
            _facinRight = false;
        }
        if (_hMovement > 0)
        {
            transform.eulerAngles = new Vector3(0, 0);
            _facinRight = true;
        }
    }
    private void Jump()
    {
        //missing buffer time
        if (_coyoteTimer > 0)
        {
            _rb.velocity = new Vector2(_rb.velocity.x, 0); 
            _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
        }
    }
    private void JumpUpgrades()
    {
        //what we want here is for the player to be able to jump even if he has left the floor recently,
        //to do that we check if he is grounded, and when he stops beeing grounded he has 0.15" to jump before he goes back to beeing grounded.
        if (NewGroundCheck._isPlayerGrounded) _coyoteTimer = _coyoteTime;
        else _coyoteTimer -= Time.deltaTime;
        //we do the opposite here, we c
        if (Input.GetButtonDown("Jump")) _bufferTimer = _bufferTime;
        else _bufferTimer -= Time.deltaTime;
    }
    private void FallMiltiplier()
    {
        //when you are falling aply the fallMultiplier, else dont.
        if (!_isDashing)
        {
            if (_rb.velocity.y < 0) _rb.gravityScale = _fallMultiplier;
            else _rb.gravityScale = 1f;
        }
    }
    //DASH
    private IEnumerator Dash()
    {
        if (_canDash)
        {
            _isDashing = true;
            _anim.Play("PlayerDash");
            _canDash = false;
            _rb.gravityScale = 0;
            if(Input.GetAxisRaw("Horizontal") != 0) _rb.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * _dashForce, 0);
            else
            {
                int _var;
                _var = (_facinRight ? 1 : -1);
                _rb.velocity = new Vector2(_var * _dashForce, 0);
            }
            yield return new WaitForSeconds(_dashDuration);
            _isDashing = false;
            _rb.gravityScale = 1;
            yield return new WaitForSeconds(_dashingCd);
            _canDash = true;
        }
    }
    //ANIMATIONS(ADITIONAL)
    private void Animations()
    {
        CheckJumpState();
        if (_canDash)
        {
            if (_isJumpingUp && !NewGroundCheck._isPlayerGrounded) _anim.Play("PlayerJumpUp");
            else if (_isJumpingDown && !NewGroundCheck._isPlayerGrounded) _anim.Play("PlayerJumpDown");
            else if (Input.GetAxisRaw("Horizontal") == 0) _anim.Play("PlayerIdle");
            else if (Mathf.Abs(_rb.velocity.x) > 0 && Mathf.Abs(_rb.velocity.x) < 10) _anim.Play("PlayerPreRun");
            if (Mathf.Abs(_rb.velocity.x) > 10 && _isJumpingUp == false && _isJumpingDown == false) _anim.Play("PlayerRun");
        }
        else
        {
            if (_isJumpingUp && !NewGroundCheck._isPlayerGrounded) _anim.Play("NoPlayerJumpUp");
            else if (_isJumpingDown && !NewGroundCheck._isPlayerGrounded) _anim.Play("NoPlayerJumpDown");
            else if (Input.GetAxisRaw("Horizontal") == 0) _anim.Play("NoPlayerIdle");
            else if (Mathf.Abs(_rb.velocity.x) > 0 && Mathf.Abs(_rb.velocity.x) < 10) _anim.Play("NoPlayerPreRun");
            if (Mathf.Abs(_rb.velocity.x) > 10 && _isJumpingUp == false && _isJumpingDown == false) _anim.Play("NoPlayerRun");
        }
    }
    private void CheckJumpState()
    {
        if (_rb.velocity.y == 0)
        {
            _isJumpingUp = false;
            _isJumpingDown = false;
        }
        else if (_rb.velocity.y > 0)
        {
            _isJumpingUp = true;
        }
        else if (_rb.velocity.y < 0)
        {
            _isJumpingUp = false;
            _isJumpingDown = true;
        }
    }
    //DEATH/Respawn (ADITIONAL)
    public void Die()
    {
        _dead = true;
        _rb.velocity = Vector2.zero;
        if(_canDash) _anim.Play("PlayerDie");
        else _anim.Play("NoPlayerDie");
        StartCoroutine(Respawn());
    }
    public void PlayerToVisible()
    {
        _playerBody.gameObject.SetActive(true);
    }
    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(2);
        this.gameObject.transform.position = new Vector3(-22.37f, 6, 0);
        PlayerToVisible();
        _anim.Play("PlayerRevive");
        yield return new WaitForSeconds(1);
    }
}
