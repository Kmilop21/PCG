using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LSystemReader : MonoBehaviour
{
    public LSystem LSystem;
    public string Grammar;
    public int Iteration = 1;
    // Start is called before the first frame update
    public virtual void Start()
    {
        string currentGrammar = Grammar;

        for(int i = 0; i < Iteration; i++)
        {
            Debug.Log(currentGrammar);
            currentGrammar = LSystem.Generate(currentGrammar);
        }

        LSystem.Interprete(currentGrammar);
    }
}
