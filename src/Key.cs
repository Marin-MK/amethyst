using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using odl;

namespace amethyst;

[DebuggerDisplay("{ToString()}")]
public class Key
{
    static Random Random = new Random();

    public Keycode MainKey;
    public List<Keycode> Modifiers;
    public string ID;

    public Key(Keycode MainKey, params Keycode[] Modifiers)
    {
        this.MainKey = MainKey;
        this.Modifiers = new List<Keycode>(Modifiers);
        ID = Random.Next(0, 999999).ToString();
    }

	public override string ToString()
	{
        if (Modifiers.Count > 0) return Modifiers.Select(x => x.ToString()).Aggregate((a, b) => a + " + " + b) + " + " + this.MainKey.ToString();
        return this.MainKey.ToString();
	}
}
