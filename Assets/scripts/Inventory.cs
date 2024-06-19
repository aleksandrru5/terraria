using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using TMPro;

public class Inventory : MonoBehaviour
{
    public ToolClass tool;

    public int stackLimit = 64;

    public Vector2 inventoryOffset;
    public Vector2 hotBarOffset;
    public Vector2 multiplier;

    public GameObject inventoryUI;
    public GameObject hotBarUI;
    public GameObject inventorySlotPrefab;

    public int inventoryWidth;
    public int inventoryHeight;

    public InventorySlot[,] inventorySlots;
    public InventorySlot[] hotbarSlots;

    public GameObject[] hotbarUISlots;
    public GameObject[,] uiSlots;

    private void Start()
    {
        inventorySlots = new InventorySlot[inventoryWidth, inventoryHeight];
        uiSlots = new GameObject[inventoryWidth, inventoryHeight];

        hotbarSlots = new InventorySlot[inventoryWidth];
        hotbarUISlots = new GameObject[inventoryWidth];

        SetupUI();
        UpdateInventoryUI();
        Add(new ItemClass(tool));
        Add(new ItemClass(tool));
        Add(new ItemClass(tool));
    }

    void SetupUI()
    {
        for (int x = 0; x < inventoryWidth; x++)
        {
            for (int y = 0; y < inventoryHeight; y++)
            {
                GameObject inventorySlot = Instantiate(inventorySlotPrefab, inventoryUI.transform);
                inventorySlot.GetComponent<RectTransform>().localPosition = new Vector3((x * multiplier.x) + inventoryOffset.x, (y * multiplier.y) + inventoryOffset.y);
                uiSlots[x, y] = inventorySlot;
                inventorySlots[x, y] = null;
            }
        }

        for (int x = 0; x < inventoryWidth; x++)
        {
            GameObject hotbarSlot = Instantiate(inventorySlotPrefab, hotBarUI.transform);
            hotbarSlot.GetComponent<RectTransform>().localPosition = new Vector3((x * multiplier.x) + hotBarOffset.x, hotBarOffset.y);
            hotbarUISlots[x] = hotbarSlot;
            hotbarSlots[x] = null;
        }
    }
    void UpdateInventoryUI()
    {
        for (int x = 0; x < inventoryWidth; x++)
        {
            for (int y = 0; y < inventoryHeight; y++)
            {
                if (inventorySlots[x, y] == null)
                {
                    uiSlots[x, y].transform.GetChild(0).GetComponent<Image>().sprite = null;
                    uiSlots[x, y].transform.GetChild(0).GetComponent<Image>().enabled = false;

                    uiSlots[x, y].transform.GetChild(1).GetComponent<TMP_Text>().text = "0";
                    uiSlots[x, y].transform.GetChild(1).GetComponent<TMP_Text>().enabled = false;
                }
                else
                {
                    uiSlots[x, y].transform.GetChild(0).GetComponent<Image>().enabled = true;
                    uiSlots[x, y].transform.GetChild(0).GetComponent<Image>().sprite = inventorySlots[x, y].item.spryte;

                    uiSlots[x, y].transform.GetChild(1).GetComponent<TMP_Text>().text = inventorySlots[x, y].quantity.ToString();
                    uiSlots[x, y].transform.GetChild(1).GetComponent<TMP_Text>().enabled = true;
                }
            }
        }

        for (int x = 0; x < inventoryWidth; x++)
        {
            if (inventorySlots[x, inventoryHeight - 1] == null)
            {
                hotbarUISlots[x].transform.GetChild(0).GetComponent<Image>().sprite = null;
                hotbarUISlots[x].transform.GetChild(0).GetComponent<Image>().enabled = false;

                hotbarUISlots[x].transform.GetChild(1).GetComponent<TMP_Text>().text = "0";
                hotbarUISlots[x].transform.GetChild(1).GetComponent<TMP_Text>().enabled = false;
            }
            else
            {
                hotbarUISlots[x].transform.GetChild(0).GetComponent<Image>().enabled = true;
                hotbarUISlots[x].transform.GetChild(0).GetComponent<Image>().sprite = inventorySlots[x, inventoryHeight - 1].item.spryte;

                hotbarUISlots[x].transform.GetChild(1).GetComponent<TMP_Text>().text = inventorySlots[x, inventoryHeight - 1].quantity.ToString();
                hotbarUISlots[x].transform.GetChild(1).GetComponent<TMP_Text>().enabled = true;
            }
        }
    }
    public bool Add(ItemClass item)
    {
        Vector2Int itemPos = Contains(item);
        bool added = false;

        if (itemPos != Vector2Int.one * -1)
        {
            if (inventorySlots[itemPos.x, itemPos.y].quantity < stackLimit)
            {
                inventorySlots[itemPos.x, itemPos.y].quantity += 1;
                added = true;
            }
        }

        if(!added)
        {
            for (int y = inventoryHeight - 1; y >= 0; y--)
            {
                if (added)
                {
                    break;
                }

                for (int x = 0; x < inventoryWidth; x++)
                {
                    if (inventorySlots[x, y] == null)
                    {
                        inventorySlots[x, y] = new InventorySlot { item = item, position = new Vector2Int(x, y), quantity = 1 };
                        added = true;
                        break;
                    }
                }
            }
        }

        UpdateInventoryUI();
        return added;
    }

    public Vector2Int Contains(ItemClass item)
    {
        for (int y = inventoryHeight - 1; y >= 0; y--)
        {
            for (int x = 0; x < inventoryWidth; x++)
            {
                if (inventorySlots[x, y] != null)
                {
                    if (inventorySlots[x, y].item.itemName == item.itemName)
                    {
                        if (item.isStackable)
                        {
                            return new Vector2Int(x, y);
                        }
                    }
                }
            }
        }
        return Vector2Int.one * -1;
    }

    public void Remove(ItemClass item)
    {

    }
}
