using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtendedAssetEditor.Detour
{
    interface IDetour
    {
        void Deploy();
        void Revert();
    }
}
