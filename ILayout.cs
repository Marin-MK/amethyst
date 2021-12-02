using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace amethyst;

public interface ILayout
{
    bool NeedUpdate { get; set; }

    void UpdateLayout();
}
