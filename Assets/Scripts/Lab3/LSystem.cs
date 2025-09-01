using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Events;


[CreateAssetMenu(fileName = "L-System")]
public class LSystem : ScriptableObject
{
    [SerializeField] private List<char> alphabet = new List<char>();
    [SerializeField] private List<string> productionRules = new List<string>();
    [SerializeField] private List<UnityEvent> events = new List<UnityEvent>();

    private string Generate(char axiom)
    {
        return productionRules[alphabet.IndexOf(axiom)];
    }

    public string Generate(string grammar)
    {
        string newGrammar = string.Empty;
        foreach (char c in grammar)
            newGrammar += Generate(c);
        return newGrammar;
    }

    public void Interprete(string grammar)
    {
        for (int i = 0; i < grammar.Length; i++)
            events[i].Invoke();
    }

    public UnityEvent GetEvent(int index) => events[index];

    public UnityEvent GetEvent(char axiom) => events[alphabet.IndexOf(axiom)];

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

//public UnityEvent this[char key]
//{
//    set
//    {
//        if (value == null)
//            return;

//        events[alphabet.IndexOf(key)] = value;
//    }
//    get => events[alphabet.IndexOf(key)];
//}

//ICollection<char> IDictionary<char, UnityEvent>.Keys => throw new NotImplementedException();

//ICollection<UnityEvent> IDictionary<char, UnityEvent>.Values => throw new NotImplementedException();

//public int Count => throw new NotImplementedException();

//bool ICollection<KeyValuePair<char, UnityEvent>>.IsReadOnly => ((IList<char>)alphabet).IsReadOnly;

//public void Add(char key, UnityEvent value)
//{
//    alphabet.Add(key);
//    events.Add(value ?? new UnityEvent());
//}

//public void Add(KeyValuePair<char, UnityEvent> item)
//{
//    Add(item.Key, item.Value);
//}

//void ICollection<KeyValuePair< char, UnityEvent >>.Clear()
//    {
//    alphabet.Clear();
//    events.Clear();
//}

//public bool Contains(KeyValuePair<char, UnityEvent> item)
//{
//    return alphabet.Contains(item.Key) && events.Contains(item.Value);
//}

//public bool ContainsKey(char key)
//{
//    return alphabet.Contains(key);
//}

//public void CopyTo(KeyValuePair<char, UnityEvent>[] array, int arrayIndex)
//{
//    int count = 0;

//    foreach (KeyValuePair<char, UnityEvent> item in this)
//    {
//        if (count >= arrayIndex)
//            array[count] = item;
//        count++;
//    }
//}

//public IEnumerator<KeyValuePair<char, UnityEvent>> GetEnumerator()
//{
//    List<KeyValuePair<char, UnityEvent>> items = new List<KeyValuePair<char, UnityEvent>>();

//    for (int i = 0; i < alphabet.Count; i++)
//        items.Add(new KeyValuePair<char, UnityEvent>(alphabet[i], events[i]));

//    return items.GetEnumerator();
//}

//bool IDictionary<char, UnityEvent>.Remove(char key)
//    {
//    int index = alphabet.IndexOf(key);

//    if (index >= 0)
//    {
//        alphabet.RemoveAt(index);
//        events.RemoveAt(index);
//        return true;
//    }

//    return false;
//}

//public bool Remove(KeyValuePair<char, UnityEvent> item)
//{
//    if (Contains(item))
//        return ((IDictionary<char, UnityEvent>)this).Remove(item.Key);

//    return false;
//}

//public bool TryGetValue(char key, out UnityEvent value)
//{
//    if (ContainsKey(key))
//    {
//        value = this[key];
//        return true;
//    }
//    value = new UnityEvent();
//    return false;
//}

//IEnumerator IEnumerable.GetEnumerator () => GetEnumerator();
