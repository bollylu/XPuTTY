using System;
using System.Collections.Generic;
using System.Text;

namespace libxputty {
  public static class LocalExtensions {

    public const string EMPTY = "<empty>";

    public static bool IsEmpty(this string source) {
      if ( string.IsNullOrEmpty(source) ) {
        return true;
      }
      if ( source == EMPTY ) {
        return true;
      }
      return false;
    }
  }
}
