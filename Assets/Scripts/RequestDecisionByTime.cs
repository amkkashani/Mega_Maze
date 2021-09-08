using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class RequestDecisionByTime : MonoBehaviour
{
    [SerializeField] private float stopTime;
    private Agent myAgent;
    
    // Start is called before the first frame update
    void Awake()
    {
        myAgent = this.GetComponent<Agent>();
    }

    void Start()
    {
        
    }


    private IEnumerator loopRequest()
    {
        while (true)
        {
            yield return new WaitForSeconds(stopTime);
            myAgent.RequestAction();
        }
    }
}
