using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using ICities;
using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace BetterLoadPanel
{
   // title bar with descriptive lable (non editable) and close button
   // holds draggable handle for moving window
   class UITitleSubPanel : UIPanel
   {
      public BetterLoadPanelWrapper ParentBetterLoadPanelWrapper;

      public UILabel Title;
      public UIButton CloseButton;
      private UIDragHandle DragHandle;
      

      // runs only once, do internal init here
      public override void Awake()
      {
         base.Awake();

         // Note - parent may not be set yet

         Title = AddUIComponent<UILabel>();
         CloseButton = AddUIComponent<UIButton>();
         DragHandle = AddUIComponent<UIDragHandle>();

         //// some defaults
         //this.height = 40;
         //this.width = 400;
      }

      // runs only once, do internal connections and setup here
      public override void Start()
      {
         base.Start();

         if (ParentBetterLoadPanelWrapper == null)
         {
            return;
         }

         // setup now that we are in parent
         //this.width = ParentBetterLoadPanelWrapper.width;
         this.isVisible = true;
         this.isEnabled = true;
         this.canFocus = true;
         this.isInteractive = true;
         this.relativePosition = Vector3.zero;

         int inset = 5;

         Title.relativePosition = new Vector3(inset, inset);
         Title.text = string.Format("{0}++", Locale.Get(LocaleID.LOADGAME_TITLE)); // "Better Load";

         CloseButton.normalBgSprite = "buttonclose";
         CloseButton.hoveredBgSprite = "buttonclosehover";
         CloseButton.pressedBgSprite = "buttonclosepressed";
         CloseButton.relativePosition = new Vector3(this.width - CloseButton.width - inset, inset);
         CloseButton.eventClick += (component, param) => { ParentBetterLoadPanelWrapper.Hide(); };

         DragHandle.target = ParentBetterLoadPanelWrapper;
         DragHandle.autoSize = true;
         DragHandle.height = this.height;
         DragHandle.width = this.width - 50;
         DragHandle.relativePosition = Vector3.zero;
      }

      public void Resize()
      {
         int inset = 5;

         Title.relativePosition = new Vector3(inset, inset);
         CloseButton.relativePosition = new Vector3(this.width - CloseButton.width - inset, inset);
         DragHandle.height = this.height;
         DragHandle.width = this.width - 50;

         //Invalidate();
      }

      public void LocaleChange()
      {
         Title.text = string.Format("{0}++", Locale.Get(LocaleID.LOADGAME_TITLE)); // "Better Load";
      }
   }
}
