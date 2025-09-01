using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;


[CreateAssetMenu(fileName = "L-System Empty", menuName = "L-Systems/Empty")]
public class LSystem : ScriptableObject
{
    [Serializable] public struct Grammar
    {
        [field: SerializeField] public char Axiom { private set; get; }
        [field: SerializeField] public string Rule { private set; get; }
        [field: SerializeField] public UnityEvent Meaning { private set; get; }
        public Grammar(char axiom, string rule)
        {
            Axiom = axiom;
            Rule = rule;
            Meaning = new UnityEvent();
        }
    }

    [SerializeField] protected List<Grammar> grammars = new List<Grammar>();

    private string Generate(char axiom) =>  grammars.First((g) => g.Axiom == axiom).Rule;
    public string Generate(string grammar)
    {
        string newGrammar = string.Empty;
        foreach (char c in grammar)
            newGrammar += Generate(c);
        return newGrammar;
    }

    public void Interprete(string grammar)
    {
        foreach (char c in grammar)
            grammars.First((g) => g.Axiom == c).Meaning.Invoke();
    }

#if UNITY_EDITOR
    public class MyEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
#endif
}

