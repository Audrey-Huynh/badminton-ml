using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;

public class BadmintonAgent : Agent
{
    public GameObject area;
    Rigidbody agentRb;
    BehaviorParameters behaviorParameters;
    public Team teamId;

    // To get birdie's location for observations
    public GameObject birdie;
    Rigidbody birdieRb;

    BadmintonSettings badmintonSettings;
    BirdieEnvController envController;

    // Controls jump behavior
    float jumpingTime;
    Vector3 jumpTargetPos;
    Vector3 jumpStartingPos;
    float agentRot;

    public Collider[] hitGroundColliders = new Collider[3];
    EnvironmentParameters resetParams;

    void Start()
    {
        envController = area.GetComponent<BirdieEnvController>();
    }

    public override void Initialize()
    {
        badmintonSettings = FindObjectOfType<BadmintonSettings>();
        behaviorParameters = gameObject.GetComponent<BehaviorParameters>();

        agentRb = GetComponent<Rigidbody>();
        birdieRb = birdie.GetComponent<Rigidbody>();

        // for symmetry between player side
        if (teamId == Team.Blue)
        {
            agentRot = -1;
        }
        else
        {
            agentRot = 1;
        }
    }

    /// <summary>
    /// Moves  a rigidbody towards a position smoothly.
    /// </summary>
    /// <param name="targetPos">Target position.</param>
    /// <param name="rb">The rigidbody to be moved.</param>
    /// <param name="targetVel">The velocity to target during the
    ///  motion.</param>
    /// <param name="maxVel">The maximum velocity posible.</param>
    void MoveTowards(
        Vector3 targetPos, Rigidbody rb, float targetVel, float maxVel)
    {
        var moveToPos = targetPos - rb.worldCenterOfMass;
        var velocityTarget = Time.fixedDeltaTime * targetVel * moveToPos;
        if (float.IsNaN(velocityTarget.x) == false)
        {
            rb.velocity = Vector3.MoveTowards(
                rb.velocity, velocityTarget, maxVel);
        }
    }

    /// <summary>
    /// Check if agent is on the ground to enable/disable jumping
    /// </summary>
    public bool CheckIfGrounded()
    {
        hitGroundColliders = new Collider[3];
        var o = gameObject;
        Physics.OverlapBoxNonAlloc(
            o.transform.localPosition + new Vector3(0, -0.05f, 0),
            new Vector3(0.95f / 2f, 0.5f, 0.95f / 2f),
            hitGroundColliders,
            o.transform.rotation);
        var grounded = false;
        foreach (var col in hitGroundColliders)
        {
            if (col != null && col.transform != transform &&
                (col.CompareTag("walkableSurface") ||
                 col.CompareTag("purpleGoal") ||
                 col.CompareTag("blueGoal")))
            {
                grounded = true; //then we're grounded
                break;
            }
        }
        return grounded;
    }

    /// <summary>
    /// Called when agent collides with the birdie
    /// </summary>
    void OnCollisionEnter(Collision c)
    {
        if (c.gameObject.CompareTag("birdie"))
        {
            envController.UpdateLastHitter(teamId);
        }
    }

    /// <summary>
    /// Starts the jump sequence
    /// </summary>
    public void Jump()
    {
        jumpingTime = 0.2f;
        jumpStartingPos = agentRb.position;
    }

    /// <summary>
    /// Resolves the agent movement
    /// </summary>
    public void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var grounded = CheckIfGrounded();

        var dirToGoForwardAction = act[0];
        var rotateDirAction = act[1];
        var dirToGoSideAction = act[2];
        var jumpAction = act[3];

        if (dirToGoForwardAction == 1)
            dirToGo = (grounded ? 1f : 0.5f) * transform.forward * 1f;
        else if (dirToGoForwardAction == 2)
            dirToGo = (grounded ? 1f : 0.5f) * transform.forward * badmintonSettings.speedReductionFactor * -1f;

        if (rotateDirAction == 1)
            rotateDir = transform.up * -1f;
        else if (rotateDirAction == 2)
            rotateDir = transform.up * 1f;

        if (dirToGoSideAction == 1)
            dirToGo = (grounded ? 1f : 0.5f) * transform.right * badmintonSettings.speedReductionFactor * -1f;
        else if (dirToGoSideAction == 2)
            dirToGo = (grounded ? 1f : 0.5f) * transform.right * badmintonSettings.speedReductionFactor;

        if (jumpAction == 1)
        {
            if (((jumpingTime <= 0f) && grounded))
            {
                Jump();
            }
        }

        transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f);
        agentRb.AddForce(agentRot * dirToGo * badmintonSettings.agentRunSpeed,
            ForceMode.VelocityChange);

        // makes the agent physically "jump"
        if (jumpingTime > 0f)
        {
            jumpTargetPos =
                new Vector3(agentRb.position.x,
                    jumpStartingPos.y + badmintonSettings.agentJumpHeight,
                    agentRb.position.z) + agentRot * dirToGo;

            MoveTowards(jumpTargetPos, agentRb, badmintonSettings.agentJumpVelocity,
                badmintonSettings.agentJumpVelocityMaxChange);
        }

        // provides a downward force to end the jump
        if (!(jumpingTime > 0f) && !grounded)
        {
            agentRb.AddForce(
                Vector3.down * badmintonSettings.fallingForce, ForceMode.Acceleration);
        }

        // controls the jump sequence
        if (jumpingTime > 0f)
        {
            jumpingTime -= Time.fixedDeltaTime;
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MoveAgent(actionBuffers.DiscreteActions);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
    }

    // For human controller
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.D))
        {
            // rotate right
            discreteActionsOut[1] = 2;
        }
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            // forward
            discreteActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            // rotate left
            discreteActionsOut[1] = 1;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            // backward
            discreteActionsOut[0] = 2;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            // move left
            discreteActionsOut[2] = 1;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            // move right
            discreteActionsOut[2] = 2;
        }
        discreteActionsOut[3] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }
}