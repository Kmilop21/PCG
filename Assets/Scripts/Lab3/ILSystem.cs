using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public interface ILSystem
{
    string Generate(char axiom);
    string Interprete(string grammar);
}
