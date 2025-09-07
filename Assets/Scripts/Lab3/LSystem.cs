using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor.Events;
using UnityEditor;
#endif


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

    private Rule[] GetCandidates(string grammar) => rules.Where((r) => r.Axiom == grammar).ToArray();
    private Rule? SelectCandidate(string grammar)
    {
        Rule[] candidate = GetCandidates(grammar);

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
            Rule? candidate = SelectCandidate(word);
            if (candidate != null)
            {
                candidate?.Meaning.Invoke();
                word = string.Empty;
            }
        }
    }

    public bool AddListenerToMeaning(string axiom, UnityAction call, int index = -1)
    {
        Rule[] candidates = GetCandidates(axiom);

        if (candidates.Length > 0)
        {
            if (index == -1)
            {
                foreach (Rule candidate in candidates)
                    candidate.Meaning.AddListener(call);
            }
            else
                candidates[index].Meaning.AddListener(call);

            return true;
        }

        return false;
    }

#if UNITY_EDITOR

    [SerializeField, HideInInspector] private bool addedPersistent;
    private void AddPersistentRuleMeaning(UnityEngine.Object obj)
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

    [CustomEditor(typeof(LSystem), true)]

    public class MyEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            LSystem lSystem = (LSystem)target;

            if(!lSystem.addedPersistent)
            {
                lSystem.addedPersistent = true;
                lSystem.AddPersistentRuleMeaning(lSystem);
            }
        }
    }
#endif
}

