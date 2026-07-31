using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerCharacter : MonoBehaviour
{
    public static WorkerCharacter instance;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float arriveEpsilon = 0.02f;
    public int maxPathExpansions = 3000;
    public int maxNearestCellSearchRadius = 8;

    [Header("Idle Wander")]
    public float idleMinWait = 1.5f;
    public float idleMaxWait = 4f;
    public int wanderPickAttempts = 20;

    [Header("Work Animation")]
    public float slashRetriggerInterval = 0.75f;
    public float itemRetriggerInterval = 1.05f;
    public float resumeWanderSettleDelay = 0.2f;

    private Animator animator;
    private Coroutine wanderRoutine;
    private Coroutine workLoopRoutine;
    private Coroutine resumeWanderRoutine;
    private int pendingTaskCount = 0;
    private bool isWorking = false;

    private static readonly Vector3Int[] Directions =
    {
        Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right
    };

    private static readonly HashSet<string> ToolSwingTasks = new HashSet<string>
    {
        "firstPlow", "secondPlow", "harvestCrop", "burn", "clearDebris", "harvestTree", "harvestAnimal"
    };

    void Start()
    {
        instance = this;
        animator = GetComponent<Animator>();
    }

    // Called synchronously the instant a task is queued (from QueueTaskSystem.SetTask), so
    // idle wandering is interrupted immediately rather than whenever the queue eventually
    // dequeues the corresponding MoveTo coroutine.
    public void NotifyTaskQueued()
    {
        pendingTaskCount++;
        if (resumeWanderRoutine != null)
        {
            StopCoroutine(resumeWanderRoutine);
            resumeWanderRoutine = null;
        }
        StopWander();
    }

    public IEnumerator MoveTo(Vector3 target)
    {
        Vector3 flatTarget = new Vector3(target.x, target.y, 0f);
        Vector3Int targetFine = TileSelector.instance.WorldToFineCell(flatTarget);
        Vector3Int goalFine = FindNearestWalkableCell(targetFine);
        Vector3Int startFine = TileSelector.instance.WorldToFineCell(transform.position);

        List<Vector3Int> path = FindPath(startFine, goalFine);
        if (path == null)
        {
            Debug.LogWarning($"WorkerCharacter: no path found to {flatTarget}, walking straight there instead.");
            yield return MoveTowardsWorld(TileSelector.instance.FineCellToWorld(goalFine));
        }
        else
        {
            foreach (Vector3Int fineCell in path)
                yield return MoveTowardsWorld(TileSelector.instance.FineCellToWorld(fineCell));
        }

        animator.SetBool("Walk", false);
        SetFacingFromDelta(flatTarget - transform.position);
    }

    public void StartWorking(string task)
    {
        isWorking = true;
        string trigger = ToolSwingTasks.Contains(task) ? "Slash" : "Item";
        float interval = trigger == "Slash" ? slashRetriggerInterval : itemRetriggerInterval;
        workLoopRoutine = StartCoroutine(WorkAnimationLoop(trigger, interval));
    }

    public void StopWorking()
    {
        isWorking = false;
        if (workLoopRoutine != null)
        {
            StopCoroutine(workLoopRoutine);
            workLoopRoutine = null;
        }

        pendingTaskCount = Mathf.Max(0, pendingTaskCount - 1);
        if (pendingTaskCount == 0)
        {
            if (resumeWanderRoutine != null) StopCoroutine(resumeWanderRoutine);
            resumeWanderRoutine = StartCoroutine(ResumeWanderAfterSettle());
        }
    }

    public void StartWander()
    {
        if (wanderRoutine != null) return;
        wanderRoutine = StartCoroutine(WanderLoop());
    }

    public void StopWander()
    {
        if (wanderRoutine != null)
        {
            StopCoroutine(wanderRoutine);
            wanderRoutine = null;
        }
        animator.SetBool("Walk", false);
    }

    private IEnumerator WorkAnimationLoop(string trigger, float interval)
    {
        while (isWorking)
        {
            animator.SetTrigger(trigger);
            yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator ResumeWanderAfterSettle()
    {
        yield return new WaitForSeconds(resumeWanderSettleDelay);
        if (pendingTaskCount == 0) StartWander();
    }

    private IEnumerator WanderLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(idleMinWait, idleMaxWait));

            Vector3Int targetFine = PickRandomWanderCell();
            Vector3Int startFine = TileSelector.instance.WorldToFineCell(transform.position);
            List<Vector3Int> path = FindPath(startFine, targetFine);
            if (path != null)
            {
                foreach (Vector3Int fineCell in path)
                    yield return MoveTowardsWorld(TileSelector.instance.FineCellToWorld(fineCell));
            }
            animator.SetBool("Walk", false);
        }
    }

    private Vector3Int PickRandomWanderCell()
    {
        Bounds bounds = MapController.instance.GetBounds();
        for (int i = 0; i < wanderPickAttempts; i++)
        {
            Vector3 point = new Vector3(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.y, bounds.max.y), 0f);
            if (!TileSelector.instance.HasTileAtWorldPos(point)) continue;
            Vector3Int fineCell = TileSelector.instance.WorldToFineCell(point);
            if (TileSelector.instance.IsFineCellWalkable(fineCell)) return fineCell;
        }
        return TileSelector.instance.WorldToFineCell(transform.position);
    }

    private IEnumerator MoveTowardsWorld(Vector3 destination)
    {
        animator.SetBool("Walk", true);
        while ((destination - transform.position).sqrMagnitude > arriveEpsilon * arriveEpsilon)
        {
            SetFacingFromDelta(destination - transform.position);
            transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = destination;
    }

    private void SetFacingFromDelta(Vector3 delta)
    {
        if (delta.sqrMagnitude < 0.0001f) return;
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            animator.SetFloat("x", Mathf.Sign(delta.x));
            animator.SetFloat("y", 0f);
        }
        else
        {
            animator.SetFloat("x", 0f);
            animator.SetFloat("y", Mathf.Sign(delta.y));
        }
    }

    // 4-directional A* over TileSelector's fine-cell occupancy grid, guided by a Manhattan-distance
    // heuristic so it heads toward the goal instead of flooding outward in every direction like a
    // plain BFS would (which was blowing through maxPathExpansions on almost any real cross-map walk,
    // since the fine grid is 4 cells per world unit).
    private List<Vector3Int> FindPath(Vector3Int start, Vector3Int goal)
    {
        if (start == goal) return new List<Vector3Int> { goal };

        var openSet = new List<Vector3Int> { start };
        var inOpenSet = new HashSet<Vector3Int> { start };
        var cameFrom = new Dictionary<Vector3Int, Vector3Int>();
        var gScore = new Dictionary<Vector3Int, int> { [start] = 0 };

        int expansions = 0;
        while (openSet.Count > 0 && expansions < maxPathExpansions)
        {
            expansions++;

            int bestIndex = 0;
            int bestF = gScore[openSet[0]] + Heuristic(openSet[0], goal);
            for (int i = 1; i < openSet.Count; i++)
            {
                int f = gScore[openSet[i]] + Heuristic(openSet[i], goal);
                if (f < bestF) { bestF = f; bestIndex = i; }
            }
            Vector3Int current = openSet[bestIndex];

            if (current == goal) return ReconstructPath(cameFrom, start, goal);

            openSet.RemoveAt(bestIndex);
            inOpenSet.Remove(current);

            foreach (Vector3Int dir in Directions)
            {
                Vector3Int next = current + dir;
                bool isGoal = next == goal;
                if (!isGoal && !TileSelector.instance.IsFineCellWalkable(next)) continue;

                int tentativeG = gScore[current] + 1;
                if (!gScore.TryGetValue(next, out int existingG) || tentativeG < existingG)
                {
                    cameFrom[next] = current;
                    gScore[next] = tentativeG;
                    if (inOpenSet.Add(next)) openSet.Add(next);
                }
            }
        }

        return null;
    }

    private static int Heuristic(Vector3Int a, Vector3Int b) =>
        Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    private static List<Vector3Int> ReconstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int start, Vector3Int goal)
    {
        var path = new List<Vector3Int>();
        Vector3Int node = goal;
        while (node != start)
        {
            path.Add(node);
            node = cameFrom[node];
        }
        path.Reverse();
        return path;
    }

    // Expanding-ring search for the nearest walkable fine cell to a (possibly occupied) target,
    // so the worker stands adjacent to a plot/tree/animal/debris rather than trying to path onto it.
    private Vector3Int FindNearestWalkableCell(Vector3Int from)
    {
        if (TileSelector.instance.IsFineCellWalkable(from)) return from;

        for (int r = 1; r <= maxNearestCellSearchRadius; r++)
        {
            for (int x = -r; x <= r; x++)
            {
                for (int y = -r; y <= r; y++)
                {
                    if (Mathf.Max(Mathf.Abs(x), Mathf.Abs(y)) != r) continue;
                    Vector3Int candidate = new Vector3Int(from.x + x, from.y + y, 0);
                    if (TileSelector.instance.IsFineCellWalkable(candidate)) return candidate;
                }
            }
        }
        return from;
    }
}
