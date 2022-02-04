using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WalkingAnimation))]
public class WalkingAnimationEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        WalkingAnimation walkingAnimationScript = (WalkingAnimation)target;

        // Update the fields of the AreaGizmos for each foot when the inner and outer radius is updated
        List<WalkingAnimation.WalkingTarget> targets = walkingAnimationScript.getAllWalkingTargets();
        foreach (WalkingAnimation.WalkingTarget target in targets)
        {
            AreaGizmo gizmo = target.targetArea.GetComponent<AreaGizmo>();

            // If the GameObject doesn't have an AreaGizmo, add one
            if (gizmo == null)
            {
                target.targetArea.AddComponent(typeof(AreaGizmo));
                gizmo = target.targetArea.GetComponent<AreaGizmo>();
            }

            gizmo.setInnerRadius(walkingAnimationScript.standRadius);
            gizmo.setRadius(walkingAnimationScript.stepRadius);
        }
    }
}
