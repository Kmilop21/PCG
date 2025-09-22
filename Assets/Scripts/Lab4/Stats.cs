using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public struct Stats
{
    public int HP;
    public int Str;
    public int Def;

    public Stats(int hp, int str, int def)
    {
        HP = hp;
        Str = str;
        Def = def;
    }

    public int GetEffectiveStr(Stats other)
    {
        int str = Str - other.Def;

        if (str < 0)
            return 0;

        return str;
    }

    public override string ToString()
    {
        return "HP: " + HP + " | Str: " + Str + " | Def: " + Def;
    }
}
