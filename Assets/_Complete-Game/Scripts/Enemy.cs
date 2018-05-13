using UnityEngine;
using System.Collections;
using System;

namespace Completed
{
    //Enemy inherits from MovingObject, our base class for objects that can move, Player also inherits from this.
    public class Enemy : MovingObject
    {
        public int playerDamage;                            //The amount of food points to subtract from the player when attacking.
        public AudioClip attackSound1;                      //First of two audio clips to play when attacking the player.
        public AudioClip attackSound2;                      //Second of two audio clips to play when attacking the player.
        public AudioClip chopSound1;                //1 of 2 audio clips that play when the wall is attacked by the player.
        public AudioClip chopSound2;

        private Animator animator;                          //Variable of type Animator to store a reference to the enemy's Animator component.
        private Transform target;                           //Transform to attempt to move toward each turn.
        private bool skipMove;                              //Boolean to determine whether or not enemy should skip a turn or move this turn.
        public int hp = 2;
        private bool bounceBack;
        private Color originalColor;

        //Start overrides the virtual Start function of the base class.
        protected override void Start()
        {
            //Register this enemy with our instance of GameManager by adding it to a list of Enemy objects. 
            //This allows the GameManager to issue movement commands.
            GameManager.instance.AddEnemyToList(this);

            //Get and store a reference to the attached Animator component.
            animator = GetComponent<Animator>();

            //Find the Player GameObject using it's tag and store a reference to its transform component.
            target = GameObject.FindGameObjectWithTag("Player").transform;
            originalColor = GetComponent<SpriteRenderer>().color;
            //Call the start function of our base class MovingObject.
            base.Start();
        }


        //Override the AttemptMove function of MovingObject to include functionality needed for Enemy to skip turns.
        //See comments in MovingObject for more on how base AttemptMove function works.
        protected override void AttemptMove<T>(int xDir, int yDir)
        {
            //Check if skipMove is true, if so set it to false and skip this turn.
            if (skipMove)
            {
                skipMove = false;
                return;

            }

            //Call the AttemptMove function from MovingObject.
            base.AttemptMove<T>(xDir, yDir);

            //Now that Enemy has moved, set skipMove to true to skip next move.
            skipMove = true;
        }

        //MoveEnemy is called by the GameManger each turn to tell each Enemy to try to move towards the player.
        public void MoveEnemy()
        {
            //Declare variables for X and Y axis move directions, these range from -1 to 1.
            //These values allow us to choose between the cardinal directions: up, down, left and right.
            int xDir = 0;
            int yDir = 0;

            //If the difference in positions is approximately zero (Epsilon) do the following:
            if (Mathf.Abs(target.position.x - transform.position.x) < float.Epsilon)
            {
                //If the y coordinate of the target's (player) position is greater than the y coordinate of this enemy's position set y direction 1 (to move up). If not, set it to -1 (to move down).
                yDir = target.position.y > transform.position.y ? 1 : -1;

            }

            //If the difference in positions is not approximately zero (Epsilon) do the following:
            else
            {
                //Check if target x position is greater than enemy's x position, if so set x direction to 1 (move right), if not set to -1 (move left).
                xDir = target.position.x > transform.position.x ? 1 : -1;

            }
            //Call the AttemptMove function and pass in the generic parameter Player, because Enemy is moving and expecting to potentially encounter a Player
            AttemptMove<Player>(xDir, yDir);
        }

        //OnCantMove is called if Enemy attempts to move into a space occupied by a Player, it overrides the OnCantMove function of MovingObject 
        //and takes a generic parameter T which we use to pass in the component we expect to encounter, in this case Player
        protected override void OnCantMove<T>(T component)
        {
            //Declare hitPlayer and set it to equal the encountered component.
            Player hitPlayer = component as Player;

            //Call the LoseFood function of hitPlayer passing it playerDamage, the amount of foodpoints to be subtracted.
            hitPlayer.LoseFood(playerDamage);

            //Set the attack trigger of animator to trigger Enemy attack animation.
            animator.SetTrigger("enemyAttack");

            //Call the RandomizeSfx function of SoundManager passing in the two audio clips to choose randomly between.
            SoundManager.instance.RandomizeSfx(attackSound1, attackSound2);
        }

        public void DamageEnemy(int loss, Transform playerTransform)
        {

            //Call the RandomizeSfx function of SoundManager to play one of two chop sounds.
            SoundManager.instance.RandomizeSfx(chopSound1, chopSound2);

            //Set spriteRenderer to the damaged wall sprite.
            Vector2 start = transform.position;

            float originalX = transform.position.x;
            float originalY = transform.position.y;
            //Subtract loss from hit point total.
            hp -= loss;
            if (transform.position.x > playerTransform.position.x)
            {
                StartCoroutine(MoveBackAndForth(new Vector3(originalX + 0.5f, originalY)));

            }
            else if (transform.position.x < playerTransform.position.x)
            {
                StartCoroutine(MoveBackAndForth(new Vector3(originalX - 0.5f, originalY)));

            }
            else if (transform.position.y < playerTransform.position.y)
            {
                StartCoroutine(MoveBackAndForth(new Vector3(originalX, originalY - 0.5f)));

            }
            else if (transform.position.y > playerTransform.position.y)
            {
                StartCoroutine(MoveBackAndForth(new Vector3(originalX, originalY + 0.5f)));
            }


            //If hit points are less than or equal to zero:
            if (hp <= 0)
            {
                //Disable the gameObject.
                gameObject.SetActive(false);
                Destroy(gameObject);
            }
        }

        private IEnumerator MoveBackAndForth(Vector3 end)
        {
            Vector3 start = transform.position;
            float sqrRemainingDistance = (start - end).sqrMagnitude;
            
            yield return null;
            while (sqrRemainingDistance > float.Epsilon)
            {
                Vector3 newPostion = Vector3.MoveTowards(start, end, 2f / moveTime * Time.deltaTime);
                GetRigidbody2D().MovePosition(newPostion);
                sqrRemainingDistance = (transform.position - end).sqrMagnitude;
                GetComponent<SpriteRenderer>().material.color = Color.red;
                
                yield return new WaitForEndOfFrame();
            }

            while (sqrRemainingDistance == 0)
            {
                Vector3 newPostion = Vector3.MoveTowards(end, start, 2f / moveTime * Time.deltaTime);
                GetRigidbody2D().MovePosition(newPostion);
                GetComponent<SpriteRenderer>().material.color = originalColor;
                
                
                yield return null;
            }


        }

    }


}
