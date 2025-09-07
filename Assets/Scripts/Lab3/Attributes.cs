using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[AttributeUsage(AttributeTargets.Method)]
public class RuleMeaningAttribute : Attribute
{
    public string Axiom { private set; get; }

    public RuleMeaningAttribute(string axiom)
    {
        Axiom = axiom;
    }
}
