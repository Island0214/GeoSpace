using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriangleCondition : ResolvedBodyCondition
{
    //两个直角边长
    public float height;  

    public float width;

    public TriangleCondition(float height, float width)
    {
        this.height = height;
        this.width = width;

    }
}


public class TriangleConditionTool : ResolvedBodyConditionTool
{
    public override FormInput FormInput()
    {
        return null;
    }

    public override bool ValidateInput(Geometry geometry, FormInput formInput)
    {
        return true;
    }

    public override Condition GenerateCondition(Geometry geometry, FormInput formInput)
    {
        bool valid = ValidateInput(geometry, formInput);
        if (!valid)
            return null;

        TriangleCondition condition = new TriangleCondition(4, 8); 

        return condition;
    }
}

public class TriangleConditionState : ConditionState
{
    new TriangleCondition condition;
    ResolvedBody geometry;

    public TriangleConditionState(Tool tool, Condition condition, Geometry geometry) : base(tool, condition)
    {
        if (condition is TriangleCondition)
            this.condition = (TriangleCondition)condition;

        if (geometry is ResolvedBody)
            this.geometry = (ResolvedBody)geometry;
    }

    public override int[] DependVertices()
    {
        return new int[] { };
    }

    public override FormInput Title()
    {
        FormInput formInput = new FormInput(1);

        formInput.inputs[0] = new FormText("旋转图形: 直角三角形");

        return formInput;
    }

}