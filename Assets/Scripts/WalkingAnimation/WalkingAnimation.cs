using System.Collections.Generic;
using UnityEngine;

public class WalkingAnimation : MonoBehaviour
{
    #region Public

    public enum WalkState { Standing, Stretched, OutOfBounds, Stepping }

    /// <summary>
    /// A class to track the placement target, IKTarget, and state machine of each leg
    /// </summary>
    [System.Serializable]
    public class WalkingTarget
    {
        /// <summary>
        /// The center of the area on the ground that the leg tries to set on. A AreaGizmo will automatically be applied.
        /// </summary>
        [Tooltip("The center of the area on the ground that the leg tries to set on. A AreaGizmo will automatically be applied.")]
        public GameObject targetArea;
        /// <summary>
        /// 
        /// </summary>
        public GameObject IKTarget;
        /// <summary>
        /// The current state of the state machine
        /// </summary>
        public WalkState state { get { return _state; }
            set
            {
                if (_state == WalkState.Standing)
                    stretchStarted = Time.time;
                if (value == WalkState.Stepping)
                {
                    stepStart = Time.time;
                    stepStartPosition.Set(legPosition.x, legPosition.y, legPosition.z);
                }

                _state = value;
            }
        }
        [SerializeField] [Tooltip("The current state of the state machine. It's recommended that you keep this at 'Standing' to begin with.")]
        private WalkState _state = WalkState.Standing;

        [HideInInspector]
        public float lerpState = 0;

        [HideInInspector]
        public Vector3 legPosition = Vector3.zero;

        [HideInInspector]
        public Vector3 stepTargetPosition = Vector3.zero;

        [HideInInspector]
        public Vector3 stepStartPosition = Vector3.zero;

        [HideInInspector]
        public float stepStart = 0;

        [HideInInspector]
        public Vector3 prevAreaPosition;

        /// <summary>
        /// The amount of time since the leg exited the standing state
        /// </summary>
        public float idleTime { get { return Time.time - stretchStarted; } }
        private float stretchStarted = 0;

        /// <summary>
        /// Initialize values that could not be initialized during serialization
        /// </summary>
        public void initialize()
        {
            stretchStarted = Time.time;
            legPosition = targetArea.transform.position;
            prevAreaPosition = targetArea.transform.position;
        }

        /// <summary>
        /// Projects the given point to the target area plane of the target area and raycasts from there to check where the ground is.
        /// </summary>
        /// <param name="point">The point to project to the target area plane</param>
        /// <returns>The point at which the ground was found. If the ground wasn't found, then the projected point is returned.</returns>
        public Vector3 getGroundAt(Vector3 point)
        {
            // Get the transform of the targetArea and define how far up and down the raycast will check
            Transform areaTransform = targetArea.transform;
            float rayCastDown = 3;
            float rayCastUp = 4;

            // Project from the given point to the targetArea plane and racast to find the ground from there
            //targetPos = Vector3.ProjectOnPlane(target, transform.up) + Vector3.Dot(transform.position, transform.up) * transform.up;
            Vector3 projectedPosition = WalkingAnimation.projectToPlane(point, areaTransform.position, areaTransform.up);
            Ray ray = new Ray(projectedPosition + (areaTransform.up * rayCastUp), -areaTransform.up);
            RaycastHit rayHit = new RaycastHit();
            bool hasHit = Physics.Raycast(ray, out rayHit, rayCastDown + rayCastUp);

            if (hasHit && rayHit.collider.CompareTag("Environment"))
                return rayHit.point;
            else
                return projectedPosition;
        }
    }

    /// <summary>
    /// A hacky workaround to the fact that you can't display 2d arrays in the unity inspector
    /// </summary>
    [System.Serializable]
    public struct WalkingTargetList
    {
        public WalkingTarget[] targetGroup;
    }

    /// <summary>
    /// This GameObject is moved relative to the average of all the foot IK targets positions to give a bouncing effect
    /// </summary>
    public GameObject body;

    /// <summary>
    /// The distance from the center of the targetArea the leg will stand still
    /// </summary>
    [Min(0)] [Tooltip("The distance from the center of the targetArea the leg will stand still")]
    public float standRadius = 0.1f;

    /// <summary>
    /// The distance from the center of the targetArea the leg will be out of bounds and step forware while in motion
    /// </summary>
    [Min(0)] [Tooltip("The distance from the center of the targetArea the leg will be out of bounds and step forware while in motion")]
    public float stepRadius = 0.5f;

    /// <summary>
    /// The amount of time each leg will wait in the stretched zone before returning the the center in seconds
    /// </summary>
    [Min(0)]
    [Tooltip("The amount of time each leg will wait in the stretched zone before returning the the center in seconds")]
    public float idleTime = 2f;

    /// <summary>
    /// The amount of time the leg takes to take a step in seconds
    /// </summary>
    [Min(0)]
    [Tooltip("The amount of time the leg takes to take a step in seconds")]
    public float stepTime = 0.2f;

    /// <summary>
    /// The curve the foot IK follows as it takes a step
    /// </summary>
    [Tooltip("The curve the foot IK follows as it takes a step")]
    public AnimationCurve stepCurve = new AnimationCurve(new Keyframe(0, 0, 0, 0, 0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0, 0, 0, 0, 0));

    public WalkingTargetList[] legTargets;

    #endregion


    #region Private

    /// <summary>
    /// Stores relevant data for a line to be visualized by a gizmo
    /// </summary>
    private struct LineDrawRequest
    {
        /// <summary>
        /// The color of the line
        /// </summary>
        public Color color;

        /// <summary>
        /// Whether or not there is a cap at the beginning of the line
        /// </summary>
        public bool startCap;

        /// <summary>
        /// Whether or not there is a cap at the end of the line
        /// </summary>
        public bool endCap;

        /// <summary>
        /// The global coordinates of the start point
        /// </summary>
        public Vector3 p1;

        /// <summary>
        /// The global coordinates of the end point
        /// </summary>
        public Vector3 p2;
    }

    /// <summary>
    /// A separate list maintained at the same length as the WalkingTargetList for reference to draw gizmos of raycasts on each leg
    /// </summary>
    private List<LineDrawRequest> lineDrawRequests = new List<LineDrawRequest>();

    /// <summary>
    /// An int representing which group is allowed to step. This ensures only one group of legs takes a step at a time for alternating steps.
    /// </summary>
    private int activeLegGroup = 0;

    /// <summary>
    /// The offset of the body from the legs in local coordinates
    /// </summary>
    private Vector3 bodyOffset;

    #endregion


    #region Core Functions

    void Awake()
    {
        // Perform initialization of WalkingTarget fields that could not be initialized during serialization
        foreach (WalkingTarget target in getAllWalkingTargets()) target.initialize();

        // If a body is given set its initial offset from the average foot position in local coordinates
        if (body != null)
            bodyOffset = (body.transform.rotation * body.transform.position) - (body.transform.rotation * getAverageTargetAreaPosition());
    }

    // Update is called once per frame
    void Update()
    {
        lineDrawRequests.Clear();

        // Iterate through each walkingtarget and handle their state machines
        for (int group = 0; group < legTargets.Length; group++)
        {
            List<WalkingTarget> currentTargetGroup = new List<WalkingTarget>();
            currentTargetGroup.AddRange(legTargets[group].targetGroup);

            bool legsStepping = false;

            foreach (WalkingTarget target in currentTargetGroup)
            {
                // Parameters and useful information for raycasting
                Transform areaTransform = target.targetArea.transform;
                Vector3 areaVelocity = (areaTransform.position - target.prevAreaPosition) * (1/Time.deltaTime);
                Vector3 projectedLegPosition = projectToPlane(target.legPosition, areaTransform.position, areaTransform.up);

                switch (target.state)
                {
                    case WalkState.Standing:
                        // If the distance from the center of the TargetArea is greater than the stand radius, switch to the Stretched state
                        if (Vector3.Distance(projectedLegPosition, target.targetArea.transform.position) > standRadius)
                            target.state = WalkState.Stretched;
                        break;

                    case WalkState.Stretched:
                        // If the distance from the center of the TargetArea is greater than the step radius, switch to the OutOfBounds state to start stepping
                        if (Vector3.Distance(projectedLegPosition, target.targetArea.transform.position) > stepRadius)
                            target.state = WalkState.OutOfBounds;

                        // Step back to the center if waiting too long
                        if (target.idleTime > idleTime && group == activeLegGroup)
                        {
                            target.stepTargetPosition = target.getGroundAt(areaTransform.position);

                            target.state = WalkState.Stepping;
                            legsStepping = true;
                        }
                        else if (Vector3.Distance(projectedLegPosition, target.targetArea.transform.position) <= standRadius)
                            target.state = WalkState.Standing;

                        break;

                    case WalkState.OutOfBounds:
                        // If the leg group is active, set the target point to the opposite direction
                        if (group == activeLegGroup)
                        {
                            // Step to the opposite end of the stepping circle
                            Vector3 stepDirection = areaVelocity.normalized;//(areaTransform.position - target.legPosition).normalized;
                            Vector3 newTarget = areaTransform.position + (stepDirection * ((areaVelocity.magnitude*stepTime) + standRadius));
                            Vector3 newTargetOnGround = target.getGroundAt(newTarget);

                            target.stepTargetPosition.Set(newTargetOnGround.x, newTargetOnGround.y, newTargetOnGround.z);
                            target.state = WalkState.Stepping;
                            legsStepping = true;
                        }
                        else if (Vector3.Distance(projectedLegPosition, target.targetArea.transform.position) <= stepRadius)
                            target.state = WalkState.Stretched;
                        break;

                    case WalkState.Stepping:
                        legsStepping = true;

                        // End the step once the step time has been exceeded
                        if (Time.time - target.stepStart <= stepTime)
                        {
                            // Raycast to the point on the group the IKTarget is currently hovering over
                            // If there was a hit, measure the distance relative to the floor, otherwise measure it relative to the targetArea plane
                            // Lerp from the current position to the target position using the animation curve for distance from the ground
                            float stepProgress = (Time.time - target.stepStart) / stepTime;
                            Vector3 lerpedPosition = Vector3.Lerp(target.stepStartPosition, target.stepTargetPosition, stepProgress);
                            Vector3 finalPosition = target.getGroundAt(lerpedPosition) + (areaTransform.up * stepCurve.Evaluate(stepProgress));

                            target.legPosition.Set(finalPosition.x, finalPosition.y, finalPosition.z);
                        }
                        else
                        {
                            // Snap the leg position to the target position
                            target.legPosition = target.stepTargetPosition;

                            // Switch to the standing state
                            target.state = WalkState.Standing;
                        }

                        // Draw a gizmo from the legPosition to the target
                        drawLine(target.legPosition, target.stepTargetPosition, Color.green, endcap: true);
                        break;

                    default:
                        target.state = WalkState.Standing;
                        break;
                }

                // Move the IKTarget to the appropriate global position
                target.IKTarget.transform.position = target.legPosition;

                target.prevAreaPosition = areaTransform.position;
            }

            if (!legsStepping)
                setActiveLegGroup(group + 1);
        }

        // Set the body to the appropriate position relative to the average foot position
        if (body != null)
        {
            Vector3 averageFootPosition = getAverageFootPosition();
            Vector3 projectedPosition = projectToPlane(body.transform.position, averageFootPosition, body.transform.up);
            Vector3 newBodyPosition = projectedPosition + Vector3.Project(body.transform.rotation * bodyOffset, body.transform.up);
            //body.transform.position.Set(body.transform.position.x, newBodyPosition.y, body.transform.position.z);
            body.transform.position = newBodyPosition;
        }
    }

    private void OnDrawGizmos()
    {
        // Iterate through each walkingtarget and draw lines for the raycasts
        foreach (LineDrawRequest request in lineDrawRequests)
        {
            Gizmos.color = request.color;
            Gizmos.DrawLine(request.p1, request.p2);

            if (request.startCap)
                Gizmos.DrawSphere(request.p1, 0.05f);
            if (request.endCap)
                Gizmos.DrawSphere(request.p2, 0.05f);
        }
    }

    #endregion


    #region Public Functions

    /// <summary>
    /// Converts the list of walkingTarget Groups into one big list of WalkingTargets.
    /// </summary>
    /// <returns>A list of all WalkingTargets</returns>
    public List<WalkingTarget> getAllWalkingTargets()
    {
        List<WalkingTarget> allTargets = new List<WalkingTarget>();

        foreach (WalkingTargetList targetList in legTargets)
        {
            allTargets.AddRange(targetList.targetGroup);
        }

        return allTargets;
    }

    /// <summary>
    /// Gets the average position of all the feet.
    /// </summary>
    /// <returns>The average global position of all the foot targets.</returns>
    public Vector3 getAverageFootPosition()
    {
        List<WalkingTarget> walkingTargets = getAllWalkingTargets();
        Vector3 averagePosition = Vector3.zero;

        foreach (WalkingTarget target in walkingTargets)
        {
            averagePosition += target.legPosition;
        }

        averagePosition /= walkingTargets.Count;

        return averagePosition;
    }

    /// <summary>
    /// Gets the average position of all the target areas.
    /// </summary>
    /// <returns>The average global position of all the foot targets.</returns>
    public Vector3 getAverageTargetAreaPosition()
    {
        List<WalkingTarget> walkingTargets = getAllWalkingTargets();
        Vector3 averagePosition = Vector3.zero;

        foreach (WalkingTarget target in walkingTargets)
        {
            averagePosition += target.targetArea.transform.position;
        }

        averagePosition /= walkingTargets.Count;

        return averagePosition;
    }

    public static Vector3 projectToPlane(Vector3 point, Vector3 planePosition, Vector3 planeNormal)
    {
        return Vector3.ProjectOnPlane(point, planeNormal.normalized) + (planeNormal.normalized * Vector3.Dot(planePosition, planeNormal.normalized));
    }

    #endregion


    #region Private Functions

    /// <summary>
    /// Increments the activeLegGroup. If the maximum value is reached, the value loops over to 0.
    /// </summary>
    private void setActiveLegGroup(int newGroup)
    {
        activeLegGroup = newGroup;

        if (activeLegGroup >= legTargets.Length)
            activeLegGroup = 0;
        if (activeLegGroup < 0)
            activeLegGroup = legTargets.Length;
    }

    private void drawLine(Vector3 start, Vector3 end, Color color, bool startcap = false, bool endcap = false)
    {
        lineDrawRequests.Add(new LineDrawRequest() { color = color, p1 = start, p2 = end, startCap = startcap, endCap = endcap });
    }

    #endregion
}
