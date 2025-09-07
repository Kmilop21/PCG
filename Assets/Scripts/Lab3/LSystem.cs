using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;


[CreateAssetMenu(fileName = "L-System Empty", menuName = "L-Systems/Empty")]
public class LSystem : ScriptableObject
{
    [System.Serializable] public struct Rule
    {
        [field: SerializeField] public string Axiom { private set; get; }
        [field: SerializeField] public string Grammar { private set; get; }
        [field: SerializeField] public UnityEvent Meaning { private set; get; }
        public Rule(string axiom, string product)
        {
            Axiom = axiom;
            Grammar = product;
            Meaning = new UnityEvent();
        }
    }

    [SerializeField] protected List<Rule> rules = new List<Rule>();
    private Rule? SelectCandidate(string grammar)
    {
        Rule[] candidate = rules.Where((r) => r.Axiom == grammar).ToArray();

        if (candidate.Length == 0)
            return null;

        return candidate[UnityEngine.Random.Range(0, candidate.Length)];
    }
    public string Generate(string grammar)
    {
        string newGrammar = string.Empty;
        string word = string.Empty;
        foreach (char c in grammar)
        {
            word += c;
            Rule? candidate = SelectCandidate(word);
            if (candidate != null)
            {
                newGrammar += candidate?.Grammar;
                word = string.Empty;
            }
        }
        return newGrammar;
    }

    public void Interprete(string grammar)
    {
        string word = string.Empty;
        foreach (char c in grammar)
        {
            word += c;
            Rule? candidate = SelectCandidate(grammar);
            if (candidate != null)
            {
                candidate?.Meaning.Invoke();
                word = string.Empty;
            }
        }
    }

    public void AddPersistentRuleMeaning(UnityEngine.Object obj)
    {
        Type t = obj.GetType();

        foreach (MethodInfo c in t.GetMethods().Where((m) => m.GetCustomAttribute<RuleMeaningAttribute>() != null))
        {
            bool isValidForMethodInfo(Rule rule)
            {
                IEnumerable<RuleMeaningAttribute> attrs = c.GetCustomAttributes<RuleMeaningAttribute>();
                return attrs.Count((attr) => attr.Axiom == rule.Axiom) > 0;
            }
            UnityAction action = Delegate.CreateDelegate(typeof(UnityAction), obj, c.Name) as UnityAction;
            foreach (Rule rule in rules.Where(isValidForMethodInfo))
                UnityEventTools.AddPersistentListener(rule.Meaning, action);
        }
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

