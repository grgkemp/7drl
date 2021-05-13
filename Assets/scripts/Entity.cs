using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Entity : MonoBehaviour
{
    [Header("Stats")]
    public float radius;
    public float min_range;
    public float max_range;
    public int maxHealth;
    public int currentHealth;
    public float agility;
    public int damage;
    public int speed;
    public bool isDead = false;
    public new string name;
    public string type = "";
    public string description = "";
    public int cost;
    [Header("Data")]
    public float x;
    public float y;
    public int team;
    public int level = 0;
    [Header("Defaults")]
    public float smoothTime;
    private Vector3 velocity = Vector3.zero;
    [Header("SFX")]
    public AudioClip hurtSFX;

    public bool TurnComplete { get; internal set; }
    public string Name { get => level>0 ? name + " " + level : name; set => name = value; }

    private void Awake()
    {
        cost = (level + 1) * (damage + speed + maxHealth/4);
    }

    private void Start()
    {
        currentHealth = maxHealth;
        GetComponentInChildren<Slider>().maxValue = maxHealth;
        GetComponentInChildren<Slider>().value = currentHealth;
        GetComponentInChildren<Rigidbody>().mass = maxHealth;
    }

    private void Update()
    {
        if (hasTarget)
        {
            gameObject.transform.position = Vector3.SmoothDamp(gameObject.transform.position, targetPosition, ref velocity, smoothTime);
        }

        if (transform.position.y < -25f)
        {
            GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    override public string ToString()
    {
        return gameObject.name;
    }

    private List<GameObject> friends;
    private List<GameObject> enemies;
    public float endTime;

    internal void NewTurn(List<GameObject> goodguys, List<GameObject> badguys, float endTime)
    {
        if (team == 0)
        { // I'm a good guy :)
            friends = goodguys;
            enemies = badguys;
        } else
        { // I'm a bad guy >:)
            friends = badguys;
            enemies = goodguys;
        }

        //remove me from my team
        //friends.Remove(gameObject);
        TurnComplete = false;
        this.endTime = endTime;
    }

    private void EndTurn()
    {
        TurnComplete = true;
        hasTarget = false;
        enemies = null;
        friends = null;
    }

    private Vector3 targetPosition;
    private bool hasTarget;

    internal void Action()
    {
        //print(ToString() + "action");
        //is an enemy in range?
        GameObject closestEnemy = null;
        float closestDist = 6500000.0f;

        foreach (GameObject enemy in enemies)
        {//find minimum distance enemy
            float dist = Vector3.Distance(gameObject.transform.position, enemy.transform.position);
            if (dist < closestDist) {
                closestDist = dist;
                closestEnemy = enemy;
            }
        }

        if (closestDist >= min_range & closestDist <= max_range) //if closest enemy in range
        {
            //print(ToString() + "attack");
            //ATTACK
            Attack(closestEnemy);
        } else
        {
            if (!hasTarget)
            {
                Vector3 enemyPosition = closestEnemy.transform.position;
                targetPosition = Vector3.MoveTowards(gameObject.transform.position, enemyPosition, agility);
                hasTarget = true;
                //print("target found");
            }
            //print("move");

            //If we reach the target or timeout
            if (Vector3.Distance(gameObject.transform.position, targetPosition) < Vector3.kEpsilon || Time.time > endTime)
            {
                EndTurn();
            }
        }
    }

    private int swing = 0;
    private Vector3 returnPosition;

    private void Attack(GameObject enemy)
    {
        //animate
        if (swing == 0) // hit enemy
        {
            swing = 1;
            returnPosition = gameObject.transform.position;
            targetPosition = enemy.transform.position;
            hasTarget = true;
        }
        else if (swing == 1) // return to position
        {
            if(Vector3.Distance(gameObject.transform.position, targetPosition) < enemy.GetComponent<Entity>().radius)
            {
                swing = 2;
                targetPosition = returnPosition;
                //deal damage
                enemy.GetComponent<Entity>().DealDamage(gameObject, damage);
            } else if (Time.time > endTime)
            { //Timeout
                swing = 0;
                hasTarget = false;
                //End turn
                EndTurn();
            }
        }
        else if (swing == 2) // deal damage and end turn or Timeout
        {
            if (Vector3.Distance(gameObject.transform.position, targetPosition) < Vector3.kEpsilon || Time.time > endTime)
            {
                swing = 0;
                hasTarget = false;

                //End turn
                EndTurn();
            }
        }
    }

    private void DealDamage(GameObject source, int damage)
    {
        GetComponentInChildren<AudioSource>().PlayOneShot(hurtSFX, 0.8f*((float)damage / (float)currentHealth));
        currentHealth -= damage;
        GetComponent<AnimationHandler>().DamageResponse(source.transform.position, damage, 0.2f);
        GetComponentInChildren<Slider>().value = currentHealth;

        if (currentHealth <= 0)
        {   // DIE
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        transform.Find("sprite").Rotate(new Vector3(45,0,0));
        transform.Find("sprite").position += new Vector3(0, 0.1f, 0);
        Destroy(transform.Find("Shadow Caster").gameObject);
        Destroy(GetComponentInChildren<Billboard>());
        GetComponentInChildren<Slider>().transform.localScale = Vector3.zero;
    }

    internal void LevelUp()
    {
        if (level < 2)
        {
            level += 1;
        }

        damage += Random.Range(0, 1+ damage * 2);
        maxHealth += Random.Range(0, 1 + maxHealth * 2);
        speed += Random.Range(0, 1 + speed * 2);
        agility += Random.Range(0, 1 + agility * 2);
        GetComponentInChildren<Rigidbody>().mass = maxHealth;

        print("level up!");
    }
}