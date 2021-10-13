using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using BLTools;
using libxputty.Interfaces;

namespace libxputty {
  public class TPuttySessionGroup : APuttyBase {

    public APuttySessionSource SessionSource { get; private set; }

    public List<APuttyBase> Items { get; private set; } = new();

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public TPuttySessionGroup() : base() { }
    public TPuttySessionGroup(string name) : base(name) {

    }
    protected override void _Initialize() {

    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    #region --- Converters -------------------------------------------------------------------------------------
    public override string ToString() {
      StringBuilder RetVal = new StringBuilder();
      RetVal.Append($"Group {Name}");
      return RetVal.ToString();
    }
    
    #endregion --- Converters ----------------------------------------------------------------------------------

  }
}
