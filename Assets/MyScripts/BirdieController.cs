using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdieController : MonoBehaviour
{
    [HideInInspector]
    public BirdieEnvController envController;

    public GameObject purpleGround;
    public GameObject blueGround;
    Collider purpleGoalCollider;
    Collider blueGoalCollider;
    // Start is called before the first frame update
    void Start()
    {
        envController = GetComponentInParent<BirdieEnvController>();
        purpleGoalCollider = purpleGoal.GetComponent<Collider>();
        blueGoalCollider = blueGoal.GetComponent<Collider>();
    }

    /// <summary>
    /// Detects whether the birdie lands in the blue, purple, or out of bounds area
    /// </summary>
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("boundary"))
        {
            // birdie went out of bounds
            envController.ResolveEvent(Event.HitOutOfBounds);
        }
        else if (other.gameObject.CompareTag("blueBoundary"))
        {
            // birdie hit into blue side
            envController.ResolveEvent(Event.HitIntoBlueArea);
        }
        else if (other.gameObject.CompareTag("purpleBoundary"))
        {
            // birdie hit into purple side
            envController.ResolveEvent(Event.HitIntoPurpleArea);
        }
        else if (other.gameObject.CompareTag("purpleGoal"))
        {
            // birdie hit purple goal (blue side court)
            envController.ResolveEvent(Event.HitPurpleGoal);
        }
        else if (other.gameObject.CompareTag("blueGoal"))
        {
            // birdie hit blue goal (purple side court)
            envController.ResolveEvent(Event.HitBlueGoal);
        }
    }


}