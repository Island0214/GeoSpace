using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ResolvedBodyCondition : Condition 
{

}

public abstract class ResolvedBodyConditionTool : ConditionTool
{

}

public class ResolvedBodyConstructor : Constructor
{
    private RectangleCondition rectangleCondition;

    new ResolvedBody geometry;

    public ResolvedBodyConstructor(Geometry geometry) : base(geometry)
    {
        if (geometry is ResolvedBody)
            this.geometry = (ResolvedBody)geometry;
    }

    public override bool AddCondition(Condition condition) {
        if (!(condition is ResolvedBodyCondition))
            return false;

        if (CheckAddCondition((ResolvedBodyCondition)condition))
        {
            Resolve();
            return true;
        }

        return false;
    }

    public override bool RemoveCondition(Condition condition) {
        if (!(condition is ResolvedBodyCondition))
            return false;
        if (condition is RectangleCondition)
        {
            if (rectangleCondition != (RectangleCondition)condition)
                return false;
            rectangleCondition = null;
            return true;
        }
        return false;
    }

    public override void ClearConditions() {
        rectangleCondition = null;
    }

    private bool CheckAddCondition(ResolvedBodyCondition condition)
    {

        if (condition is ResolvedBodyCondition)
        {
            if (rectangleCondition != null)
                return false;
            rectangleCondition = (RectangleCondition)condition;
            return true;
        }

        return false;
    }

    private void Resolve()
    {
        GeometryBehaviour geometryBehaviour = GameObject.Find("/3D/Geometry").GetComponent<GeometryBehaviour>();
        if (rectangleCondition != null){
            Vector2 position = new Vector2(rectangleCondition.height, rectangleCondition.width);
            geometry.SetRectangle(position);
            geometryBehaviour.InitGeometry(geometry);
            geometry.isSpinned = true;
        }
    }

}