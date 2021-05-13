using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class BattleController : MonoBehaviour
{
    private List<GameObject> friends = new List<GameObject>();
    private List<GameObject> enemies = new List<GameObject>();
    public List<GameObject> turnOrder = new List<GameObject>();

    private GameController gameController;

    public GameObject reticle;
    public GameObject platform;

    public float turnDelay;
    public float turnLength;

    bool BattleRunning = true;

    // Start is called before the first frame update
    void Start()
    {
        gameController = GameObject.Find("GameController").GetComponent<GameController>();
        gameController.lowerSection.SetActive(true);

        for (int i = 0; i < (gameController.progress - gameController.progress/3) & i < 5; i++)
        {
            GameObject enemy = gameController.enemyTypes[Random.Range(0, Math.Min(gameController.progress - gameController.progress / 3, gameController.enemyTypes.Count))];
            GameObject enemy1 = Instantiate(enemy, new Vector3(Random.Range(0, Random.onUnitSphere.x * platform.transform.localScale.x/2.5f),
                                                            0, Random.Range(0, Random.onUnitSphere.z * platform.transform.localScale.z/2.5f)), Quaternion.identity);
            while (Random.value*250 < gameController.progress)
            {
                enemy1.GetComponent<Entity>().LevelUp();
            }

            enemies.Add(enemy1);
            turnOrder.Add(enemy1);
            enemy1.GetComponent<Entity>().team = 1;
            enemy1.transform.parent = transform;
        }

        foreach (GameObject friend in gameController.friends)
        {
            GameObject friend1 = Instantiate(friend, new Vector3(0, 100, 0), Quaternion.identity);
            friend1.GetComponent<Rigidbody>().isKinematic = true;
            friend1.GetComponent<Entity>().team = 0;
            friends.Add(friend1);
            turnOrder.Add(friend1);
            friend1.transform.Find("sprite").transform.Find("Canvas").transform.Find("Slider").transform.Find("Fill Area").transform.Find("Fill").GetComponent<Image>().color = Color.green;
            friend1.transform.parent = transform;
            friend1.transform.localScale = new Vector3(1,1,1);
        }
        
        nextTime = turnDelay * 5;

        turnOrder = turnOrder.OrderBy(x => Random.value).ToList();
    }

    private float nextTime;
    private bool placementPhase = true;
    private int dropIndex = -1;
    private bool dropPhase = false;

    // Update is called once per frame
    void Update()
    {
        if (placementPhase)
        {
            PlaceDrops();
        }
        else if (dropPhase)
        {
            dropPhase = false;
            foreach (GameObject friend in friends)
            {
                if (friend.transform.position.y > 5) //todo make more efficient
                {
                    dropPhase = true; //silly
                }
            }

            gameController.cardsUpdatedFlag = true;
        }
        else if (BattleRunning & Time.time >= nextTime)
        {
            Turn();
        }
    }

    private void PlaceDrops()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (dropIndex == -1)
        {
            reticle = Instantiate(reticle);
            dropIndex = 0;
        }

        if (Physics.Raycast(ray, out hit))
        {
            Transform objectHit = hit.transform;
            if (objectHit == platform.transform)
            {
                Vector3 newPosition = hit.point;
                newPosition.y = 0.03f;
                reticle.transform.position = newPosition;

                if (Input.GetButtonDown("Fire1"))
                {
                    reticle.GetComponent<ReticleController>().isSpinning = false;

                    Vector3 friendPosition = friends[dropIndex].transform.position;
                    newPosition.y = friendPosition.y + dropIndex*4;
                    friends[dropIndex].transform.position = newPosition;

                    dropIndex += 1;
                    if (dropIndex == friends.Count)
                    {
                        placementPhase = false; // all drop positions entered
                        dropPhase = true; 
                        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("placementPhase"))
                        {
                            Destroy(obj, Random.Range(3f, 4f));
                        }
                        //turn grav back on

                        foreach (GameObject friend in friends)
                        {
                            friend.GetComponent<Rigidbody>().isKinematic = false;
                        }
                    }
                    else
                    {
                        // new reticle
                        reticle = Instantiate(reticle);
                        reticle.GetComponent<ReticleController>().isSpinning = true;
                    }
                }
            }
            else
            {
                reticle.transform.position = new Vector3(-100, -100, -100);
            }
        }
    }

    public GameObject ActiveGameObject;
    private Entity ActiveEntity;

    void Turn()
    {
        if (ActiveGameObject == null)
        {
            NewTurn();
        }

        ActiveEntity.Action();

        if (ActiveEntity.TurnComplete)
        {
            EndTurn();
        }
    }

    void NewTurn()
    {
        ActiveGameObject = turnOrder[0];
        ActiveEntity = (Entity)ActiveGameObject.GetComponent("Entity");

        ActiveEntity.NewTurn(friends, enemies, Time.time + turnLength);
        turnOrder.Remove(ActiveGameObject);

        //print(ActiveEntity.ToString() + " new turn, enemies: " + enemies.Count);
    }

    List<GameObject> theFallen = new List<GameObject>();

    void EndTurn()
    {
        //print(ActiveEntity.ToString() + " end turn");
        turnOrder.Add(ActiveGameObject);
        ActiveGameObject = null;
        ActiveEntity = null;

        //add small delay before next turn
        nextTime = Time.time + turnDelay;

        //check deaths and remove
        List<GameObject> toRemove = new List<GameObject>();

        foreach (GameObject combatant in turnOrder)
        {
            if (combatant.GetComponent<Entity>().isDead)
            {
                toRemove.Add(combatant);
                if (combatant.GetComponent<Entity>().team==1)
                {
                    gameController.wealth += 1;
                }
            } else if (combatant.transform.position.y < -1.0f)
            {
                toRemove.Add(combatant);
                theFallen.Add(combatant);
            }
        }

        foreach (GameObject deadCombatant in toRemove)
        {
            turnOrder.Remove(deadCombatant);
            enemies.Remove(deadCombatant);
            friends.Remove(deadCombatant);
        }

        if (friends.Count == 0)
        {
            //you lose :(
            BattleRunning = false;
            Destroy(gameController.gameObject);
            Destroy(GameObject.Find("Menu UI"));
            Invoke("ReturnToMenu", 3);
        }
        else if (enemies.Count == 0)
        {
            //you win!
            gameController.friends = friends;
            BattleRunning = false;
            gameController.lowerSection.SetActive(false);
            gameController.setShopContents(theFallen);
            SceneManager.LoadSceneAsync("Ship Rescue", LoadSceneMode.Additive);
        }

        gameController.cardsUpdatedFlag = true;
    }

    private void ReturnToMenu()
    {
        SceneManager.LoadSceneAsync("Menu", LoadSceneMode.Single); //TODO gameover screen
    }
}
