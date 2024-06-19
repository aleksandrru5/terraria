using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public int selectorSlotIndex = 0;
    public GameObject hotBarSelector;

    public Inventory inventory;
    public bool inventoryShowing = false;

    public ItemClass selectedItem;

    public Vector2Int mousePos;
    public int playerRange = 5;

    public float moveSpeed;
    public float jumpForce;
    public bool onGround;

    private Rigidbody2D rb;
    private Animator anim;

    public float horizontal;
    public bool hit;
    public bool place;


    [HideInInspector]
    public Vector2 spawnPos;
    public TerrainGeneration TerrainGenerator;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        inventory = GetComponent<Inventory>();
        inventory.inventoryUI.SetActive(inventoryShowing);
    }

    public void Spawn()
    {
        GetComponent<Transform>().position = spawnPos;
    }

    private void FixedUpdate()
    {
        float jump = Input.GetAxisRaw("Jump");

        Vector2 movement = new Vector2(horizontal * moveSpeed, rb.velocity.y);

        if (horizontal > 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (horizontal < 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }

        if (jump > 0.1f)
        {
            if (onGround == true)
            {
                movement.y = jumpForce;
            }
        }

        rb.velocity = movement;
    }

    public void Update()
    {
        horizontal = Input.GetAxis("Horizontal");
        hit = Input.GetMouseButtonDown(0);
        place = Input.GetMouseButton(1);



        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (selectorSlotIndex < inventory.inventoryWidth - 1)
            {
                selectorSlotIndex += 1;
            }
        }
        else if(Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            if (selectorSlotIndex > 0)
            {
                selectorSlotIndex -= 1;
            }
        }

        hotBarSelector.transform.position = inventory.hotbarUISlots[selectorSlotIndex].transform.position;

        if (inventory.inventorySlots[selectorSlotIndex, inventory.inventoryHeight - 1].item != null)
        {
            selectedItem = inventory.inventorySlots[selectorSlotIndex, inventory.inventoryHeight - 1].item;
        }
        else
        {
            selectedItem = null;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            inventoryShowing = !inventoryShowing;
        }

        if (Vector2.Distance(transform.position, mousePos) <= playerRange && Vector2.Distance(transform.position, new Vector2(mousePos.x + 0.5f, mousePos.y)) > 0.95f)
        {
            if (place == true)
            {
                if (selectedItem != null)
                {
                    if (selectedItem.itemType == ItemClass.ItemType.block)
                    {
                        TerrainGenerator.CheckTile(selectedItem.tile, mousePos.x, mousePos.y, false);
                    }
                }
            }
        }
        if (Vector2.Distance(transform.position, mousePos) <= playerRange)
        {
            if (hit == true)
            {
                TerrainGenerator.RemoveTile(mousePos.x, mousePos.y);
            }
        }

        mousePos.x = Mathf.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).x - 0.5f);
        mousePos.y = Mathf.RoundToInt(Camera.main.ScreenToWorldPoint(Input.mousePosition).y - 0.5f);

        inventory.inventoryUI.SetActive(inventoryShowing);

        anim.SetFloat("forizontal", horizontal);
        anim.SetBool("hit", hit || place);
    }
}
