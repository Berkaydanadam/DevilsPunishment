﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Medkit", menuName = "Item/Medkit")]
public class MedkitItem : Item
{

    public float healAmount = 40.0f;
    public float timeToApply = 2.0f;


    public override bool Use()
    {
        Player player = Player.instance;
        Health health = player.GetComponent<Health>();
        if (health.curHealth < 100)
        {
            Debug.Log("Medkit used! Player healed by " + healAmount + " health points.");

            //Player player = Player.instance;
            //Health health = player.GetComponent<Health>();
            health.IncreaseHealth(healAmount, timeToApply);

            return true; //Medkits are consumed upon usage
        }
        Debug.Log("Attempted to use item, health was already full. -No TGM HERE :P ");
        return false;
    }

}
