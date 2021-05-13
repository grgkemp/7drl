using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardUIController : MonoBehaviour
{
    public GameObject representation;
    private Entity entity;

    [Header("UI Fields")]
    public Image glow;
    public Slider healthBar;
    public TextMeshProUGUI repName;
    public GameObject sprite;
    public TextMeshProUGUI atk;
    public TextMeshProUGUI typ;
    public TextMeshProUGUI spd;
    public TextMeshProUGUI unknown;
    public GameObject costPanel;
    public TextMeshProUGUI costField;

    [Header("Stats")]
    public bool isSelected = false;

    private void Start()
    {
        if (representation == null)
        {
            print("card passed null gameobject");
        }
        entity = representation.GetComponent<Entity>();

        healthBar.maxValue = entity.maxHealth;
        healthBar.value = entity.currentHealth;

        repName.text = entity.Name;

        sprite.GetComponent<SpriteRenderer>().sprite = representation.GetComponentInChildren<SpriteRenderer>().sprite;
        sprite.GetComponent<PaletteSwapper>().entity = representation.GetComponentInChildren<PaletteSwapper>().entity;
        sprite.GetComponent<PaletteSwapper>().e_primaries = representation.GetComponentInChildren<PaletteSwapper>().e_primaries;
        sprite.GetComponent<PaletteSwapper>().f_primaries = representation.GetComponentInChildren<PaletteSwapper>().f_primaries;
        sprite.GetComponent<PaletteSwapper>().secondaries = representation.GetComponentInChildren<PaletteSwapper>().secondaries;

        atk.text = "" + entity.damage;
        typ.text = "" + entity.type;
        spd.text = "" + entity.speed;
        unknown.text = "" + entity.description;

        if (entity.team == 0)
        {
            healthBar.transform.Find("Fill Area").transform.Find("Fill").GetComponent<Image>().color = Color.green;
        }

        costField.text = "§"+entity.cost;
    }

    private void Update()
    {
        glow.enabled = IsPointerOverGameObject();
        healthBar.value = entity.currentHealth;
    }

    public bool IsPointerOverGameObject()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults.Any(x => x.gameObject == gameObject);
    }
}
