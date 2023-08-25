using System;
using System.Collections.Generic;
using odl;

namespace amethyst;

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
}
