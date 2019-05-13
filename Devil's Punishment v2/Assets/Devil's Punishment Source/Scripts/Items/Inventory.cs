using System;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Inventory : MonoBehaviour
{

    public enum SortType {
        TYPE,
        ALPHABETICAL
    }

    [System.Serializable]
    public class InventorySlot : IComparable<InventorySlot> {
        public Item item;
        public int stack;

        public InventorySlot(Item _item, int _stack) {
            item = _item;
            stack = _stack;
        }
        public InventorySlot() {
            item = null;
            stack = 0;
        }

        public int CompareTo(InventorySlot other) {

            if(item == null && other.item == null) {return 0;}

            if(item != null && other.item == null) {
                return -1;
            } else if (item == null && other.item != null) {
                return 1;
            } else {
                switch(Inventory.sortType) {
                    case SortType.ALPHABETICAL:
                        return item.CompareToAlphabetical(other.item);
                    default:
                    case SortType.TYPE:
                        return item.CompareTo(other.item);
                }
            }

        }

    }

    public List<InventorySlot> inventory;
    public GunItem equippedGun;
    public static Inventory instance;
    public static SortType sortType;

    private GunController gunController;

    private void Awake() {
        instance = this;
    }

    private void Start() {
        Sort();
        gunController = GunController.instance;
    }

    public Item GetItemFromIndex(int index) {
        if(index == inventory.Count) {
            return equippedGun;
        } else if(index > -1 && inventory[index] != null) {
            return inventory[index].item;
        } else {
            return null;
        }
    }

    public void AddItem(Item item, int stack=1) {

        if(item is GunItem) {
			if (equippedGun != null)
			{
				Debug.Log("Gun in hand," + equippedGun.name +" " + (equippedGun as Item).description + ")" + ", so dropping it first...(");
				DropItem(10); // Drop the "GUN" in hand
			} else
			{
				Debug.Log("Player doesn't appear to be holding a gun, so... ");

			}


			Debug.Log("Equipping picked gun " +item.name);
			equippedGun = item as GunItem;
            gunController.InitGun();

            return;
        }

        for(int k = 0; k < stack; k++) {

            for(int i = 0; i < inventory.Count; i++) {

                if(inventory[i] != null && inventory[i].item != null) {

                    if(item.Equals(inventory[i].item) && inventory[i].stack < inventory[i].item.maxStackSize) {
                        inventory[i].stack ++;
                        break;
                    }


                } else {
                    inventory[i] = new InventorySlot(item, 1);
                    break;
                }

            }

            CompoundInventory();
        }

    }

    public void DropItemAll(string name) {
        int[] indexes = GetIndexesOfItem(name);

        foreach(int i in indexes) {
            DropItemAll(i);
        }
    }

    public void DropItemAll(int index)
	{
		Debug.Log("Root of DropItem func reached");
        if(index>=inventory.Count) {

			GameObject GunPrefab;
			if (equippedGun.clipSize >=32) // This bool is probably not good enough long-term
			{
				GunPrefab = ResourceManager.instance.getResource("Pickup_Assault_Rifle");

			} else
			{
				GunPrefab = ResourceManager.instance.getResource("Pickup_Handgun");
			}

			Debug.Assert(GunPrefab != null, "GunPrefab shouldn't be null. It didn't load correctly as a resource.");

			GameObject DroppedGun = Instantiate(GunPrefab, gameObject.transform.position , gameObject.transform.rotation);

			DroppedGun.SetActive(true);


			equippedGun = null;
			gunController.InitGun();

			Debug.Log("It should have dropped now... Must make sure to create a gameObject for it... Should be handled by the DropItem function calls probably.");

		}
		else if(index > -1)
		{
			if (inventory[index].item != null)
			{

				string name = inventory[index].item.name;
				Debug.Log("Dropping item: " + name);


				GameObject itemPrefab;
				if (name == "Basic Medkit") // This bool is probably not good enough long-term
				{
					itemPrefab = ResourceManager.instance.getResource("Pikcup_Medkit_Basic");

				}
				else
				{ // ToDo: The Rest of them
					itemPrefab = ResourceManager.instance.getResource("Pickup_Handgun");
				}
				Debug.Assert(itemPrefab != null, "itemPrefab shouldn't be null. It didn't load correctly as a resource.");
				GameObject DroppedGun = Instantiate(itemPrefab, gameObject.transform.position, gameObject.transform.rotation);
				DroppedGun.SetActive(true);


				inventory.RemoveAt(index);
                inventory.Add(new InventorySlot());
            }
        }

    }

    public void DropItem(string name, int amount = 1) {
		Debug.Log("Attempting to drop " + name);
        int x = amount;

        while(x > 0) {

            int index = GetIndexOfItem(name);

            if(index > -1) {
				Debug.Log("Item was found to be at index:" + index);
                DropItem(index, 1);
                x--;
            } else {
				Debug.Log("Item index was not found");
                break;
            }

        }

        Sort();

    }

    public void DropItem(int index, int amount = 1) {

        for(int i = 0; i < amount; i++) {

            if(index>=inventory.Count) {
				DropItemAll(index);
                return;
            } else if(index > -1) {
                if(inventory[index].item != null && inventory[index].stack > 1) {
                    inventory[index].stack--;
					Debug.Log("Appears to be dropping part of a stack... Must remember to create the game object for it.");
                } else {
                    DropItemAll(index);
                    return;
                }
            }

        }

    }

    public void UseItem(int index) {

        Item item = GetItemFromIndex(index);
        if(item != null) {
            if(item.Use()) {
                DropItem(index);
            }
        }

    }

    public bool ContainsItem(string name) {
        int index = GetIndexOfItem(name);

        return (index != -1);

    }

    public int GetEquippedGunAmmo() {

        return GetQuantityOfItem(equippedGun.ammunitionType.name);
    }

    public int GetQuantityOfItem(string name) {
        int[] indexes = GetIndexesOfItem(name);

        if(indexes.Length == 0) {
            return 0;
        }

        int quantity = 0;

        for(int i = 0; i < indexes.Length; i++) {
            quantity += inventory[indexes[i]].stack;
        }

        return quantity;
    }

    public int[] GetIndexesOfItem(string name) {
        List<int> indexes = new List<int>();
        for(int i = 0; i < inventory.Count; i++) {
            if(inventory[i].item != null && name.Equals(inventory[i].item.name)) {
                indexes.Add(i);
            }
        }

        return indexes.ToArray();
    }

    public void ChangeSortType(int sortTypeIndex) {
        sortType = (SortType) sortTypeIndex;
        Sort();
    }

    public void Sort() {
        CompoundInventory();
        inventory.Sort();
    }

    public int GetIndexOfItem(string name) {
        for(int i = 0; i < inventory.Count; i++) {
            if(inventory[i].item != null && name.Equals(inventory[i].item.name)) {
                return i;
            }
        }

        return -1;
    }

    private void CompoundInventory() {
        for(int i = 0; i < inventory.Count; i++) {

            if(inventory[i].item != null) {
                int[] indexes = GetIndexesOfItem(inventory[i].item.name);

                if(indexes.Length > 1) {
                    for(int y = 0; y < indexes.Length-1; y++) {
                        while(inventory[indexes[y]].stack < inventory[indexes[y]].item.maxStackSize && inventory[indexes[y+1]].stack>0) {
                            inventory[indexes[y]].stack++;
                            inventory[indexes[y+1]].stack--;
                        }
                        while(inventory[indexes[y]].stack > inventory[indexes[y]].item.maxStackSize) {
                            inventory[indexes[y]].stack--;
                            inventory[indexes[y+1]].stack++;
                        }

                    }
                }

            }
        }

        //If overflow, drag to another slot
        for(int i = 0; i < inventory.Count; i++) {
            if(inventory[i].item != null && inventory[i].stack > 0) {
                if(inventory[i].stack > inventory[i].item.maxStackSize) {
                    int dif = inventory[i].stack - inventory[i].item.maxStackSize;
                    inventory[i].stack = inventory[i].item.maxStackSize;
                    AddItem(inventory[i].item, dif);
                }
            }
        }

        //Cull nulls
        CullNulls();
    }

    private void CullNulls() {
        for(int i = 0; i < inventory.Count; i++) {
            if(!(inventory[i] != null && inventory[i].item != null && inventory[i].stack > 0)) {
                DropItemAll(i);
            }
        }
    }

    private void EmptySlot(int index) {
        inventory[index].item = null;
        inventory[index].stack = 0;
    }

    public void PrintInventory() {

        for(int i = 0; i < inventory.Count; i++) {
            if(inventory[i].item != null) {
                Debug.Log(inventory[i].item.name + " x " + inventory[i].stack + " : " + inventory[i].item.description);
            } else {
                Debug.Log("EMPTY");
            }
        }
    }

}
