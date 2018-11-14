using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using BLTools;
using libxputty_std20.Interfaces;

namespace libxputty_std20 {
  public class TPuttySessionGroup : TPuttyBase {

    public TPuttySessionSource SessionSource { get; private set; }

    public List<TPuttyBase> Items { get; private set; } = new List<TPuttyBase>();

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
