using System.Collections;
using UnityEngine;

public class PlayerSlash3State : PlayerBaseState
{
    private float cooldown = .5f;
    private float attackTime;
    private bool combo;
    private float damageBoost;
    private float critRate;
    private bool isVampire;

    private float playerAttackDamage = 3f;
    public override void EnterState(PlayerStateManager Player)
    {
        Debug.Log("Entering Slash3 State");
        attackTime = cooldown;
        combo = false;
        Player.Coroutine(DashDelay(Player));
        Player.animator.SetTrigger("slash3");
        damageBoost = Player.GetComponent<PlayerController>().getDamageBoost();
        critRate = Player.GetComponent<PlayerController>().getCritRate();
        isVampire = Player.GetComponent<PlayerController>().doesVampire();
    }

    public override void UpdateState(PlayerStateManager Player)
    {
        if(attackTime <= 0 && combo == false)
        {
            Player.animator.SetTrigger("neutral");
            Player.SwitchState(Player.NeutralState);
        }
        else if(attackTime <= .05f && combo == true)
        {
            Player.SwitchState(Player.Slash1State);
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            combo = true;
        }

        attackTime -= Time.deltaTime;
    }

    public override void OnCollisionEnter2D(PlayerStateManager Player, Collision2D other)
    {
        
    }
    
    public override void OnTriggerStay2D(PlayerStateManager Player, Collider2D other) 
    {

    }

    //done in animation events
    public override void EventTrigger(PlayerStateManager Player)
    {
        Vector2 knockbackDirection = (Vector2)(Player.transform.position - Player.attackPoint.position).normalized;
        LayerMask mask = LayerMask.GetMask("Enemy");
        Collider2D[] colliders = Physics2D.OverlapCircleAll(Player.attackPoint.position, Player.attackRange, mask);

        foreach (Collider2D enemy in colliders)
        {
            if (enemy is not BoxCollider2D)
                continue;
            NewEnemy enemyScript = enemy.GetComponent<NewEnemy>();
            
            if (enemyScript != null) {
                float totalDamage =  playerAttackDamage * damageBoost;  // increase total damage by damageBoost
                float crit = Random.Range(1, 100);
                if(crit <= (critRate * 100))
                {
                    totalDamage *= 1.5f;    // use random number to determine if player hits a crit or not
                }
                enemyScript.TakeDamage(totalDamage);
                if(isVampire) {
                     Player.GetComponent<PlayerController>().healPlayer(3f);
                }
                if (enemy.CompareTag("Enemy")){
                    Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
                    if (enemyRb != null) {
                        // Apply knockback
                        enemyRb.AddForce(-knockbackDirection * 5, ForceMode2D.Impulse);
                    }
                }
            }
        }
    }

    public override void TakeDamage(PlayerStateManager Player)
    {
        Player.SwitchState(Player.HitState);
    }

    IEnumerator DashDelay(PlayerStateManager Player)
    {
        yield return new WaitForSeconds(cooldown * (4/5));
        Vector2 inputDirection = new Vector2(Player.findDirectionFromInputs("Left", "Right"), Player.findDirectionFromInputs("Down", "Up")).normalized;
        inputDirection.Normalize();
        Player.rb.AddForce(inputDirection * 500);

        if (inputDirection.x != 0){ //If the player is moving horizontally
            Player.flipped = inputDirection.x < 0; //If the player is moving left, flipped is true, if the player is moving right, flipped is false
        }

        Player.transform.rotation = Quaternion.Euler(new Vector3(0f, Player.flipped ? 180f: 0f, 0f));
    }
}
