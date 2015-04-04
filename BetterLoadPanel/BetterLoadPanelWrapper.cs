using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using ICities;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;
using UnityEngine.UI;

namespace BetterLoadPanel
{
   class BetterLoadPanelWrapper : UIPanel
   {
      // sub panels
      public UITitleSubPanel TitleSubPanel;
      public UISortedSavedGamesPanel ListPanel;
      public UILoadPanelDetails DetailsPanel;
      private UIResizeHandle ResizeHandle;

      public void Initialize()
      {
         // first, set up our panel stuff
         UIView uiv = UIView.GetAView();

         if (uiv == null)
         {
            // log error
            return;
         }

         this.name = "BetterLoadPanelWrapper";
         this.cachedName = "BetterLoadPanelWrapper";
         this.stringUserData = "BetterLoadPanelWrapper";

         this.backgroundSprite = "MenuPanel";// or MenuPanel2
         int viewWidth = (int)uiv.GetScreenResolution().x;
         int viewHeight = (int)uiv.GetScreenResolution().y;
         this.clipChildren = false;
         
         this.canFocus = true;
         this.isInteractive = true;
         //this.autoLayout = true;
         //this.autoLayoutDirection = LayoutDirection.Vertical;
         //this.autoLayoutPadding = new RectOffset(0, 0, 1, 1);
         //this.autoLayoutStart = LayoutStart.TopLeft;

         //this.position = new Vector3(0, 0, 0);//test// new Vector3((-viewWidth / 2), (viewHeight / 2));
         this.useCenter = true;
         this.CenterToParent();
         this.useCenter = false;
         //testing tempsize

         this.minimumSize = new Vector2(1170, 450);

         this.width = viewWidth * 0.5f;
         this.height = viewHeight * 0.5f;

         
         //this.SendToBack();

         float inset = 5;

         TitleSubPanel = AddUIComponent<UITitleSubPanel>();
         TitleSubPanel.ParentBetterLoadPanelWrapper = this;
         TitleSubPanel.relativePosition = Vector3.zero;
         TitleSubPanel.width = this.width;
         TitleSubPanel.height = 40;

         ListPanel = AddUIComponent<UISortedSavedGamesPanel>();
         ListPanel.ParentBetterLoadPanelWrapper = this;
         ListPanel.relativePosition = new Vector3(inset, TitleSubPanel.relativePosition.y + TitleSubPanel.height + inset);
         ListPanel.width = (this.width / 1.75f) - 2f*inset; // was width/2
         ListPanel.height = this.height - ListPanel.relativePosition.y - inset;

         float detailinset = 30;

         DetailsPanel = AddUIComponent<UILoadPanelDetails>();
         DetailsPanel.ParentBetterLoadPanelWrapper = this;
         DetailsPanel.relativePosition = new Vector3(ListPanel.relativePosition.x + ListPanel.width + detailinset, ListPanel.relativePosition.y + inset, 0);
         DetailsPanel.width = this.width - ListPanel.width - 2 * detailinset;
         DetailsPanel.height = ListPanel.height - 2 * detailinset;

         ResizeHandle = AddUIComponent<UIResizeHandle>();
         ResizeHandle.AlignTo(this, UIAlignAnchor.BottomRight);
         ResizeHandle.isInteractive = true;
         ResizeHandle.edges = UIResizeHandle.ResizeEdge.Right | UIResizeHandle.ResizeEdge.Bottom;
         ResizeHandle.size = new Vector2(16, 16);
         ResizeHandle.backgroundSprite = "buttonresize";
         ResizeHandle.relativePosition = new Vector3(ResizeHandle.relativePosition.x - 16, ResizeHandle.relativePosition.y - 16, 0);
         
      }

      protected override void OnSizeChanged()
      {
         base.OnSizeChanged();

         if (ResizeHandle == null || !ResizeHandle.containsMouse)
         {
            return;
         }

         //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "current size = " + this.size.ToString());

         // resize children
         float inset = 5;
         TitleSubPanel.width = this.width;
         ListPanel.relativePosition = new Vector3(inset, TitleSubPanel.relativePosition.y + TitleSubPanel.height + inset);
         ListPanel.width = (this.width / 1.75f) - 2f * inset; // was width/2
         ListPanel.height = this.height - ListPanel.relativePosition.y - inset;
         float detailinset = 30;
         DetailsPanel.relativePosition = new Vector3(ListPanel.relativePosition.x + ListPanel.width + detailinset, ListPanel.relativePosition.y + inset, 0);
         DetailsPanel.width = this.width - ListPanel.width - 2 * detailinset;
         DetailsPanel.height = ListPanel.height - 2 * detailinset;

         TitleSubPanel.Resize();         
         ListPanel.Resize();
         DetailsPanel.Resize();

         //Invalidate();
      }
      public void LocaleChange()
      {
         TitleSubPanel.LocaleChange();
         ListPanel.LocaleChange();
         DetailsPanel.LocaleChange();
      }

      public void Refresh()
      {
         ListPanel.Refresh();
      }
      //public void ShowPanel(Vector2 pos)
      //{
      //   // alternate show/hid
      //   if (this.isVisible)
      //   {
      //      this.isVisible = false;
      //      return;
      //   }


      //   this.isVisible = true;

      //}

      // Unity methods
      public override void OnDestroy()
      {
         base.OnDestroy();
      }

      public void OnGUI()
      {
      }
   }
}
