using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupSlot : MonoBehaviour
{
    [SerializeField] private SpriteRenderer powerupSpriteRenderer;
    [SerializeField] private GameObject spriteMask;

    private float spriteHeight;

    // Start is called before the first frame update
    private void Awake()
    {
        spriteHeight = powerupSpriteRenderer.size.y;        
    }

    public void SetSprite(Sprite sprite)
    {
        powerupSpriteRenderer.sprite = sprite;
    }

    public void SetColor(Color color)
    {
        powerupSpriteRenderer.color = color;
    }

    public void SetScale(float scale)
    {
        spriteMask.gameObject.transform.localScale = new Vector3(1, scale, 1);
        spriteMask.gameObject.transform.localPosition = new Vector3(0, spriteHeight * (scale - 1) / 2f, 0);
    }
}
