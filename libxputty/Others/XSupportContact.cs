using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace libxputty {
  public class XSupportContact : ASessionBase, ISupportContact {
    public string SupportMessage { get; private set; }
    public string Email { get; private set; }
    public string Phone { get; private set; }
    public string HelpUri { get; private set; }
    public string Message { get; private set; }

    private static Random _Generator = new Random(DateTime.Now.Millisecond);

    #region --- Constructor(s) ---------------------------------------------------------------------------------
    public XSupportContact() {
      _Initialize();
    }

    protected override void _Initialize() {
      int RandomValue = _Generator.Next(1, 10);
      switch (RandomValue) {
        case >= 1 and <= 5:
          Name = $"Demo{RandomValue}";
          Message = "Please contact your responsible";
          Comment = "When the system is going slow";
          Description = "Malfunction";
          Email = "user.boss@domain.net";
          Phone = "+32-456-789321951";
          break;
        case >= 6 and <= 10:
          Name = $"Demo{RandomValue}";
          Message = "Please contact technical support if you are low on RAM";
          Comment = "Do not abuse, RAM is expensive";
          Description = "Technical support";
          Email = "user.technical@domain.net";
          Phone = "+32-456-789321952";
          break;
      }
    }
    #endregion --- Constructor(s) ------------------------------------------------------------------------------

    public void DisplaySupportMessage() {
      throw new NotImplementedException();
    }

    
  }
}
