﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libxputty {
  public interface IName {
    string Name { get; set; }
    string Description { get; set; }
    string Comment { get; set; }
  }
}
