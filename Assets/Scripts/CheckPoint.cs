using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            PlayerAgentDestroyer player = other.GetComponent<PlayerAgentDestroyer>();
            if (player != null)
            {
                player.rechedTheCheckPoint();
                this.gameObject.SetActive(false);
            }
        }
    }
}
