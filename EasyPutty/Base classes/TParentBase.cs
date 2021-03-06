﻿using System;
using System.Xml.Linq;

namespace EasyPutty.Base {
  public class TParentBase : IParent {

    public TParentBase() { }
    public TParentBase(XElement element)  { }

    #region IParent
    public IParent Parent { get; set; }
    public T GetParent<T>() {
      if (Parent == null) {
        return default(T);
      }
      if (Parent.GetType().Name == typeof(T).Name) {
        return (T)Convert.ChangeType(Parent, typeof(T));
      }
      return Parent.GetParent<T>();
    }
    #endregion IParent
  }
}
