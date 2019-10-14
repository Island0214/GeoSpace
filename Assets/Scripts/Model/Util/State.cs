using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class State
{
    public Tool tool;

    public Action OnClickDelete;

    public Action OnElementHighlight; //需求一

    public Action DoubleClick;


    public State(Tool tool)
    {
        this.tool = tool;
    }

    public abstract int[] DependVertices();
    public abstract FormInput Title();
}
