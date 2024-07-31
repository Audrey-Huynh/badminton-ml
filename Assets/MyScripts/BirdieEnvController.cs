using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Team
{
    Blue = 0,
    Purple = 1,
    Default = 2
}

public enum Event
{
    HitPurpleGoal = 0,
    HitBlueGoal = 1,
    HitOutOfBounds = 2,
    HitIntoBlueArea = 3,
    HitIntoPurpleArea = 4
}

public class BirdieEnvController : MonoBehaviour
{
    int birdieSpawnSide;

    BadmintonSettings badmintonSettings;

    public BadmintonAgent blueAgent;
    public BadmintonAgent purpleAgent;

    public List<BadmintonAgent> AgentsList = new List<BadmintonAgent>();
    List<Renderer> RenderersList = new List<Renderer>();

    Rigidbody blueAgentRb;
    Rigidbody purpleAgentRb;

    public GameObject birdie;
    Rigidbody birdieRb;

    public GameObject BlueGoal;
    public GameObject PurpleGoal;

    Renderer blueGoalRenderer;

    Renderer purpleGoalRenderer;

    Team lastHitter;

    private int resetTimer;
    public int MaxEnvironmentSteps;

    void Start()
    {

        // Used to control agent & birdie starting positions
        blueAgentRb = blueAgent.GetComponent<Rigidbody>();
        purpleAgentRb = purpleAgent.GetComponent<Rigidbody>();
        birdieRb = birdie.GetComponent<Rigidbody>();

        // Starting birdie spawn side
        // -1 = spawn blue side, 1 = spawn purple side
        var spawnSideList = new List<int> { -1, 1 };
        birdieSpawnSide = spawnSideList[Random.Range(0, 2)];

        // Render ground to visualise which agent scored
        blueGoalRenderer = BlueGoal.GetComponent<Renderer>();
        purpleGoalRenderer = PurpleGoal.GetComponent<Renderer>();
        RenderersList.Add(blueGoalRenderer);
        RenderersList.Add(purpleGoalRenderer);

        badmintonSettings = FindObjectOfType<BadmintonSettings>();

        ResetScene();
    }

    /// <summary>
    /// Tracks which agent last had control of the birdie
    /// </summary>
    public void UpdateLastHitter(Team team)
    {
        lastHitter = team;
    }

    /// <summary>
    /// Resolves scenarios when birdie enters a trigger and assigns rewards
    /// </summary>
    public void ResolveEvent(Event triggerEvent)
    {
        switch (triggerEvent)
        {
            case Event.HitOutOfBounds:
                if (lastHitter == Team.Blue)
                {
                    // apply penalty to blue agent
                }
                else if (lastHitter == Team.Purple)
                {
                    // apply penalty to purple agent
                }

                // end episode
                blueAgent.EndEpisode();
                purpleAgent.EndEpisode();
                ResetScene();
                break;

            case Event.HitBlueGoal:
                // blue wins

                // turn floor blue
                StartCoroutine(GoalScoredSwapGroundMaterial(badmintonSettings.blueGoalMaterial, RenderersList, .5f));

                // end episode
                blueAgent.EndEpisode();
                purpleAgent.EndEpisode();
                ResetScene();
                break;

            case Event.HitPurpleGoal:
                // purple wins

                // turn floor purple
                StartCoroutine(GoalScoredSwapGroundMaterial(badmintonSettings.purpleGoalMaterial, RenderersList, .5f));

                // end episode
                blueAgent.EndEpisode();
                purpleAgent.EndEpisode();
                ResetScene();
                break;

            case Event.HitIntoBlueArea:
                if (lastHitter == Team.Purple)
                {
                    purpleAgent.AddReward(1);
                }
                break;

            case Event.HitIntoPurpleArea:
                if (lastHitter == Team.Blue)
                {
                    blueAgent.AddReward(1);
                }
                break;
        }
    }

    /// <summary>
    /// Changes the color of the ground for a moment.
    /// </summary>
    /// <returns>The Enumerator to be used in a Coroutine.</returns>
    /// <param name="mat">The material to be swapped.</param>
    /// <param name="time">The time the material will remain.</param>
    IEnumerator GoalScoredSwapGroundMaterial(Material mat, List<Renderer> rendererList, float time)
    {
        foreach (var renderer in rendererList)
        {
            renderer.material = mat;
        }

        yield return new WaitForSeconds(time); // wait for 2 sec

        foreach (var renderer in rendererList)
        {
            renderer.material = badmintonSettings.defaultMaterial;
        }

    }

    /// <summary>
    /// Called every step. Control max env steps.
    /// </summary>
    void FixedUpdate()
    {
        resetTimer += 1;
        if (resetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            blueAgent.EpisodeInterrupted();
            purpleAgent.EpisodeInterrupted();
            ResetScene();
        }
    }

    /// <summary>
    /// Reset agent and birdie spawn conditions.
    /// </summary>
    public void ResetScene()
    {
        resetTimer = 0;

        lastHitter = Team.Default; // reset last hitter

        foreach (var agent in AgentsList)
        {
            // randomise starting positions and rotations
            var randomPosX = Random.Range(-2f, 2f);
            var randomPosZ = Random.Range(-2f, 2f);
            var randomPosY = Random.Range(0.5f, 3.75f); // depends on jump height
            var randomRot = Random.Range(-45f, 45f);

            agent.transform.localPosition = new Vector3(randomPosX, randomPosY, randomPosZ);
            agent.transform.eulerAngles = new Vector3(0, randomRot, 0);

            agent.GetComponent<Rigidbody>().velocity = default(Vector3);
        }

        // reset birdie to starting conditions
        ResetBirdie();
    }

    /// <summary>
    /// Reset birdie spawn conditions
    /// </summary>
    void ResetBirdie()
    {
        var randomPosX = Random.Range(-2f, 2f);
        var randomPosZ = Random.Range(6f, 10f);
        var randomPosY = Random.Range(6f, 8f);

        // alternate birdie spawn side
        // -1 = spawn blue side, 1 = spawn purple side
        birdieSpawnSide = -1 * birdieSpawnSide;

        if (birdieSpawnSide == -1)
        {
            birdie.transform.localPosition = new Vector3(randomPosX, randomPosY, randomPosZ);
        }
        else if (birdieSpawnSide == 1)
        {
            birdie.transform.localPosition = new Vector3(randomPosX, randomPosY, -1 * randomPosZ);
        }

        birdieRb.angularVelocity = Vector3.zero;
        birdieRb.velocity = Vector3.zero;
    }
}