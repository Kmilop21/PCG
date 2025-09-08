using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LSystemReader : MonoBehaviour
{
    public LSystem LSystem;
    public string axiom;
    public int Iteration = 1;

    public bool InitializeOnStart = true;

    public bool Initialized { private set; get; }
    // Start is called before the first frame update
    public void Initialize()
    {
        Initialized = false;
        LSystem.Initialize();
        string currentGrammar = axiom;

        for(int i = 0; i < Iteration; i++)
        {
            Debug.Log(currentGrammar);
            currentGrammar = LSystem.Generate(currentGrammar);
        }
        Debug.Log(currentGrammar);
        LSystem.Interprete(currentGrammar);
        Initialized = true;
    }

    public virtual void Start()
    {
        if (InitializeOnStart)
            Initialize();
    }
    public void Restart()
    {
        foreach(var go in LSHolder.instance.list)
        {
            Destroy(go);
        }

        string currentGrammar = axiom;

        for (int i = 0; i < Iteration; i++)
        {
            Debug.Log(currentGrammar);
            currentGrammar = LSystem.Generate(currentGrammar);
        }
        Debug.Log(currentGrammar);
        LSystem.Interprete(currentGrammar);
    }

    public void changeIterations(string s)
    {
        if (float.TryParse(s, out float result))
        {
            Iteration = (int)result;
        }
    }
}
