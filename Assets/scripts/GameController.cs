using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public List<GameObject> friends = new List<GameObject>();
    private List<GameObject> cards = new List<GameObject>();
    public int progress = 1;
    public int wealth = 0;

    [Header("Objects")]
    public GameObject cardObject;
    public GameObject lowerSection;
    public GameObject shop;
    public GameObject nextAreaButton;
    public TextMeshProUGUI wealthUI;
    public TextMeshProUGUI progressUI;

    public List<GameObject> enemyTypes;

    public int cardStart;
    public int cardOffset;
    public int cardHeight;
    public int shopHeight;

    private Rect rect;
    private Canvas canvas;

    private void Awake()
    {
        rect = GetComponent<RectTransform>().rect;
        canvas = GetComponent<Canvas>();
        DontDestroyOnLoad(gameObject);

        foreach (GameObject friend in friends)
        {
            GameObject card = CardFromEntity(friend);
            card.transform.SetParent(lowerSection.transform, false);
            cards.Add(card);
        }

        nextAreaButton.SetActive(false);
        shop.SetActive(false);
        lowerSection.SetActive(true);

        SceneManager.LoadScene("Battle");
    }

    public bool cardsUpdatedFlag = true;

    private void Update()
    {
        canvas.worldCamera = Camera.main;
        if (cardsUpdatedFlag)
        {
            GameObject battleController = GameObject.Find("BattleController");
            if (battleController != null)
            {
                cards = emptyCards(cards);
                foreach (GameObject representation in battleController.GetComponent<BattleController>().turnOrder)
                {
                    GameObject card = CardFromEntity(representation);
                    card.transform.SetParent(lowerSection.transform, false);
                    cards.Add(card);
                }
            }
            else
            {
                cards = emptyCards(cards);
                foreach (GameObject friend in friends)
                {
                    GameObject card = CardFromEntity(friend);
                    card.transform.SetParent(lowerSection.transform, false);
                    cards.Add(card);
                }

            }
            cardsUpdatedFlag = false;
        }

        rearrangeCards();
        displayShop();
        moveLowerSection();

        wealthUI.text = "§" + wealth;
        progressUI.text = "Island " + progress;
    }

    private Vector3 lowerSectionVel;

    private void moveLowerSection()
    {
        if (Input.mousePosition.y < 200)
        {
            lowerSection.transform.localPosition = Vector3.SmoothDamp(lowerSection.transform.localPosition, new Vector3(0, -rect.height/2, 0), ref lowerSectionVel, 0.25f);
        }
        else
        {
            lowerSection.transform.localPosition = Vector3.SmoothDamp(lowerSection.transform.localPosition, new Vector3(0, -200-rect.height / 2, 0), ref lowerSectionVel, 0.25f);
        }
    }

    private GameObject CardFromEntity(GameObject representation)
    {
        GameObject card = Instantiate(cardObject);
        CardUIController cardControl = card.GetComponent<CardUIController>();
        cardControl.representation = representation;

        return card;
    }

    private void rearrangeCards()
    {
        float x = cardStart -rect.width / 2;
        foreach (GameObject card in cards)
        {
            card.transform.localPosition = new Vector3(x, cardHeight, 0);
            x += cardOffset;
        }
    }

    private List<GameObject> emptyCards(List<GameObject> theseCards)
    {
        if (theseCards != null)
        {
            for (int i = 0; i < theseCards.Count; i++)
            {
                Destroy(theseCards[i]);
            }
        }
        return new List<GameObject>();
    }

    private List<GameObject> shopContents = new List<GameObject>();
    private List<GameObject> shopCards = new List<GameObject>();

    internal void setShopContents(List<GameObject> theFallen)
    {
        shopContents = new List<GameObject>();
        for (int i = 0; i < 3; i++)
        {
            if (i < theFallen.Count)
            {
                GameObject forSale = theFallen[i];
                forSale.GetComponent<Entity>().cost /= 2;
                shopContents.Add(forSale);
            } else
            {
                GameObject forSale = Instantiate(enemyTypes[UnityEngine.Random.Range(0, Math.Min(progress - progress / 3, enemyTypes.Count))], shop.transform);
                print(forSale.GetComponent<Entity>().Name + " " + forSale.GetComponent<Entity>().cost);
                while (UnityEngine.Random.value * 250 < progress)
                {
                    forSale.GetComponent<Entity>().LevelUp();
                }
                shopContents.Add(forSale);
            }
        }
    }

    bool shopSetUp = false;

    internal void setUpShop()
    {
        shopSetUp = true;
        shop.SetActive(true);
        shopCards = emptyCards(shopCards);
        foreach (GameObject obj in shopContents)
        {
            GameObject card = CardFromEntity(obj);
            card.transform.SetParent(shop.transform, false);
            card.GetComponent<CardUIController>().costPanel.SetActive(true);
            shopCards.Add(card);
        }

        nextAreaButton.SetActive(true);
        lowerSection.SetActive(true);
    }

    private void displayShop()
    {
        if (shopSetUp)
        {

            float x = -1.5f * cardOffset;

            List<GameObject> boughtCards = new List<GameObject>();
            foreach (GameObject card in shopCards)
            {
                card.transform.localPosition = new Vector3(x, shopHeight, 0);
                x += cardOffset*1.5f;

                if (Input.GetButtonDown("Fire1") & card.GetComponent<CardUIController>().IsPointerOverGameObject())
                {
                    GameObject representation = card.GetComponent<CardUIController>().representation;
                    Entity entity = representation.GetComponent<Entity>();

                    if (wealth >= entity.cost) { 
                        print("bought this card");
                        GameObject newFriend = Instantiate(representation);
                        DontDestroyOnLoad(newFriend);
                        friends.Add(newFriend);
                        entity.team = 0;
                        newFriend.transform.Find("sprite").transform.Find("Canvas").transform.Find("Slider").transform.Find("Fill Area").transform.Find("Fill").GetComponent<Image>().color = Color.green;
                        cardsUpdatedFlag = true;
                        boughtCards.Add(card);
                        wealth -= entity.cost;

                        // check for triple
                        CheckForTriple();
                    }
                }
            }

            for (int i = 0; i < boughtCards.Count; i++)
            {
                shopCards.Remove(boughtCards[i]);
                Destroy(boughtCards[i]);
            }
        }
    }

    private void CheckForTriple()
    {
        List<GameObject> toRemove = new List<GameObject>();
        for (int i = 0; i < friends.Count; i++)
        {
            GameObject friend1 = friends[i];
            Entity entity1 = friend1.GetComponent<Entity>();
            for (int j = 0; j < friends.Count; j++)
            {
                if (i != j)
                {
                    GameObject friend2 = friends[j];
                    Entity entity2 = friend2.GetComponent<Entity>();

                    if (entity1.Name == entity2.Name)
                    {
                        for (int k = 0; k < friends.Count; k++)
                        {
                            if (k != i && k != j)
                            {
                                GameObject friend3 = friends[k];
                                Entity entity3 = friend3.GetComponent<Entity>();

                                if (entity3.Name == entity2.Name)
                                {
                                    //triple found
                                    toRemove.Add(friend1);
                                    toRemove.Add(friend2);

                                    entity3.LevelUp();
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        foreach (GameObject friend in toRemove)
        {
            friends.Remove(friend);
        }
    }

    public void nextArea()
    {
        nextAreaButton.SetActive(false);
        shopSetUp = false;
        shop.SetActive(false);
        lowerSection.SetActive(false);
        progress += 1;
        GameObject.Find("Ship").GetComponent<ShipAnimator>().AnimateDropTransition(friends);
    }
}
