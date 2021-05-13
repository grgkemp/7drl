using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShipAnimator : MonoBehaviour
{
    public Vector3 position0;
    public Vector3 position1;
    public Vector3 position2;
    public float boatSmoothTime;
    public float cameraSmoothTime;
    public GameObject ShopUI;

    public IEnumerator AnimateShip()
    {
        //transform.position = position0;

        int state = 0;
        Vector3 velocity = Vector3.zero;
        Vector3 cameraVelocity = Vector3.zero;

        while (Vector3.Distance(transform.position, position2) > 0.5f)
        {
            if (state == 0)
            {
                transform.position = Vector3.SmoothDamp(transform.position, position1, ref velocity, boatSmoothTime);

                if (Vector3.Distance(transform.position, position1) < 0.5f)
                {
                    state = 1;
                }
            }
            else if (state == 1 || state == 2)
            {
                if (state == 1)
                {
                    //Animate on boarding
                    GameObject bc = GameObject.Find("BattleController");
                    List<Transform> children = new List<Transform>();
                    for (int i = 0; i < bc.transform.childCount; i++)
                    {
                        bc.transform.GetChild(i).localScale = Vector3.zero;
                        children.Add(bc.transform.GetChild(i));
                    }

                    foreach (Transform child in children)
                    {
                        if (!child.GetComponent<Entity>().isDead)
                        {
                            child.parent = null;
                            DontDestroyOnLoad(child.gameObject);
                        }
                    }

                    Destroy(GameObject.Find("CameraRotator"));
                    state = 2;
                    yield return null;
                }

                transform.position = Vector3.SmoothDamp(transform.position, position2, ref velocity, boatSmoothTime);
                Vector3 cameraTarget = new Vector3(transform.position.x, transform.position.y + transform.localScale.y * 3, Camera.main.transform.position.z);
                Camera.main.transform.position = Vector3.SmoothDamp(Camera.main.transform.position, cameraTarget, ref cameraVelocity, cameraSmoothTime);
            }

            yield return null;
        }

        SceneManager.UnloadSceneAsync("Battle");

        //Display UI
        GameObject.Find("GameController").GetComponent<GameController>().setUpShop();
    }

    public void AnimateTransition()
    {
        StartCoroutine(AnimateShip());
    }

    public void AnimateDropTransition(List<GameObject> toDrop)
    {
        StartCoroutine(AnimateDrop(toDrop));
    }

    public IEnumerator AnimateDrop(List<GameObject> toDrop)
    {
        // for each gameobject, jump, then fall, then load battle and unload ship
        string state = "jumping";
        int jumperIndex = 0;
        GameObject lastJumper = toDrop[0];

        while (state != "end")
        {
            if (state == "jumping")
            {
                lastJumper = Instantiate(toDrop[jumperIndex], transform.position, Quaternion.identity);
                //lastJumper.transform.parent = transform;
                lastJumper.transform.localScale = new Vector3(1, 1, 1);
                lastJumper.GetComponent<Rigidbody>().isKinematic = false;
                jumperIndex += 1;

                if (jumperIndex >= toDrop.Count)
                {
                    state = "falling";
                }
                yield return new WaitForSeconds(0.3f);
            }
            else if (state == "falling")
            {
                if (Vector3.Distance(lastJumper.transform.position, transform.position) > 10)
                {
                    state = "end";
                }
                yield return null;
            }
        }

        SceneManager.LoadSceneAsync("Battle");
        Invoke("unloadScene", 0.5f);
    }

    private void unloadScene()
    {
        SceneManager.UnloadSceneAsync("Ship Rescue");
    }
}