﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLTools.MVVM;
using EasyPutty.ViewModels;
using EasyPutty.Views;
using libxputty_std20;
using libxputty_std20.Interfaces;

namespace EasyPutty.ViewModels {
  public class TVMSupportContacts : TVMEasyPuttyBase {

    public List<TVMSupportContact> Items { get; } = new List<TVMSupportContact>();

    public TVMSupportContacts() : base() { }

    public TVMSupportContacts(TSupportContactCollection contacts) : this() {
      if ( contacts == null ) {
        return;
      }

      foreach ( ISupportContact ContactItem in contacts.Items ) {
        Items.Add(new TVMSupportContact(ContactItem));
      }
    }

    public TVMSupportContacts(ISupportContactContainer contactContainer) : this() {
      if ( contactContainer == null ) {
        return;
      }

    }

    public void DisplaySupportMessage() {
      PopupDisplaySupportContact Popup = new PopupDisplaySupportContact {
        DataContext = this
      };
      Popup.ShowDialog();
    }


    public static TVMSupportContacts Demo {
      get {
        if (_Demo==null) {
          TSupportContactCollection SupportContacts = new TSupportContactCollection();
          SupportContacts.Add(TSupportContact.Demo1);
          SupportContacts.Add(TSupportContact.Demo2);
          _Demo = new TVMSupportContacts(SupportContacts);
        }
        return _Demo;
      }
    }
    private static TVMSupportContacts _Demo;
  }
}
