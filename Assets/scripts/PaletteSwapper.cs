using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaletteSwapper : MonoBehaviour
{
    public Texture2D palette;
    public int[] e_primaries;
    public int[] f_primaries;
    public int[] secondaries;
    public Entity entity;
    // Start is called before the first frame update
    void Start()
    {
        int value1;
        if (entity.team == 0)
        {
            value1 = f_primaries[entity.level];
        }
        else
        {
            value1 = e_primaries[entity.level];
        }
        int value2 = secondaries[entity.level];
        MaterialPropertyBlock mat = new MaterialPropertyBlock();
        GetComponent<Renderer>().GetPropertyBlock(mat);
        mat.SetColor("_Primary_Color", palette.GetPixel(value1, 2));
        mat.SetColor("_Secondary_Color", palette.GetPixel(value2, 2));
        GetComponent<Renderer>().SetPropertyBlock(mat);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
