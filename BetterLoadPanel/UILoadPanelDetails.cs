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
using ColossalFramework.Plugins;
using ColossalFramework.Packaging;
using ColossalFramework.Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace BetterLoadPanel
{
   class UILoadPanelDetails : UIPanel
   {
      public BetterLoadPanelWrapper ParentBetterLoadPanelWrapper;

      public UIButton LoadButton;
      public UIButton CancelButton;

      public UITextureSprite Snapshot;
      public UISprite AchSprite;
      public UISprite TrophySprite;

      private float SnapshotMaxWidth;
      private float SnapshotMaxHeight;

      // runs only once, do internal init here
      public override void Awake()
      {
         base.Awake();

         // Note - parent may not be set yet
         Snapshot = AddUIComponent<UITextureSprite>();
         TrophySprite = AddUIComponent<UISprite>();
         AchSprite = AddUIComponent<UISprite>();

         LoadButton = AddUIComponent<UIButton>();
         CancelButton = AddUIComponent<UIButton>();

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
         this.backgroundSprite = "GenericPanelLight";
         int inset = 25;

         LoadButton.autoSize = true;
         LoadButton.text = Locale.Get(LocaleID.LOAD);// "LOAD";
         LoadButton.textPadding = new RectOffset(5, 5, 2, 2);
         LoadButton.normalBgSprite = "ButtonMenu";
         LoadButton.hoveredBgSprite = "ButtonMenuHovered";
         LoadButton.pressedBgSprite = "ButtonMenuPressed";

         LoadButton.relativePosition = new Vector3(inset, this.height - inset - LoadButton.height);
         LoadButton.eventClick += (component, param) => { ParentBetterLoadPanelWrapper.ListPanel.LaunchSelectedSaveGame(); };
         LoadButton.isVisible = true;
         LoadButton.enabled = true;
         LoadButton.isInteractive = true;

         CancelButton.autoSize = true;
         CancelButton.text = Locale.Get(LocaleID.CANCEL);// "CANCEL";
         CancelButton.textPadding = new RectOffset(5, 5, 2, 2);
         CancelButton.normalBgSprite = "ButtonMenu";
         CancelButton.hoveredBgSprite = "ButtonMenuHovered";
         CancelButton.pressedBgSprite = "ButtonMenuPressed";
         CancelButton.relativePosition = new Vector3(LoadButton.relativePosition.x + LoadButton.width + inset, LoadButton.relativePosition.y, 0);
         CancelButton.eventClick += (component, param) => { ParentBetterLoadPanelWrapper.Hide(); };
         CancelButton.isVisible = true;
         CancelButton.enabled = true;
         CancelButton.isInteractive = true;

         Snapshot.relativePosition = new Vector3(inset, inset, 0);
         //Snapshot.autoSize = true;         
         Snapshot.width = this.width - 2 * inset;
         Snapshot.height = this.height - 4*inset - LoadButton.height;
         SnapshotMaxHeight = Snapshot.height;
         SnapshotMaxWidth = Snapshot.width;

         //TrophySprite.autoSize = true;
         TrophySprite.spriteName = "ThumbnailTrophy";
         TrophySprite.isVisible = true;
         TrophySprite.fillDirection = UIFillDirection.Horizontal;
         TrophySprite.fillAmount = 1;
         TrophySprite.width = 32;
         TrophySprite.height = 32;
         
         TrophySprite.relativePosition = new Vector3(inset, LoadButton.relativePosition.y - 5 - TrophySprite.height);

         //AchSprite.autoSize = true;
         AchSprite.spriteName = "Niet";
         AchSprite.isVisible = false;
         AchSprite.fillDirection = UIFillDirection.Horizontal;
         AchSprite.fillAmount = 1;
         AchSprite.width = 32;
         AchSprite.height = 32;
         
         AchSprite.relativePosition = new Vector3(TrophySprite.relativePosition.x, TrophySprite.relativePosition.y, 0);
      }

      public void Resize()
      {
         int inset = 25;
         LoadButton.relativePosition = new Vector3(inset, this.height - inset - LoadButton.height);
         CancelButton.relativePosition = new Vector3(LoadButton.relativePosition.x + LoadButton.width + inset, LoadButton.relativePosition.y, 0);
         Snapshot.relativePosition = new Vector3(inset, inset, 0);

         Snapshot.width = this.width - 2 * inset;
         Snapshot.height = this.height - 4 * inset - LoadButton.height;
         SnapshotMaxHeight = Snapshot.height;
         SnapshotMaxWidth = Snapshot.width;
         RecalculateSnapshotSize();

         TrophySprite.relativePosition = new Vector3(inset, LoadButton.relativePosition.y - 5 - TrophySprite.height);
         AchSprite.relativePosition = new Vector3(TrophySprite.relativePosition.x, TrophySprite.relativePosition.y, 0);

         //Invalidate();
      }

      public void LocaleChange()
      {
         int inset = 25;

         LoadButton.text = Locale.Get(LocaleID.LOAD);// "LOAD";
         LoadButton.relativePosition = new Vector3(inset, this.height - inset - LoadButton.height);

         CancelButton.text = Locale.Get(LocaleID.CANCEL);// "CANCEL";
         CancelButton.relativePosition = new Vector3(LoadButton.relativePosition.x + LoadButton.width + inset, LoadButton.relativePosition.y, 0);

         TrophySprite.relativePosition = new Vector3(inset, LoadButton.relativePosition.y - 5 - TrophySprite.height);
         AchSprite.relativePosition = new Vector3(TrophySprite.relativePosition.x, TrophySprite.relativePosition.y, 0);
      }

      public void RecalculateSnapshotSize()
      {
         if (Snapshot != null && Snapshot.texture != null)
         {
            float texwidth = Snapshot.texture.width;
            float texheight = Snapshot.texture.height;

            // calculate new size while keeping aspect ratio the same
            float ratioX = SnapshotMaxWidth / texwidth;
            float ratioY = SnapshotMaxHeight / texheight;

            // use whichever multiplier is smaller
            float ratio = Math.Min(ratioX, ratioY);

            float newWidth = texwidth * ratio;
            float newHeight = texheight * ratio;

            Snapshot.width = newWidth;
            Snapshot.height = newHeight;
         }
      }

      public void SetSelected(SaveGameRowStruct sgrs)
      {
         if (Snapshot.texture != null)
         {
            UnityEngine.Object.Destroy(Snapshot.texture);
         }

         if (sgrs.saveMeta.imageRef != null)
         {            
            Texture newtex = sgrs.saveMeta.imageRef.Instantiate<Texture>();
            Snapshot.texture = newtex;
            Snapshot.texture.wrapMode = TextureWrapMode.Clamp;
            RecalculateSnapshotSize();
         }
         else
         {
            Snapshot.texture = null;
         }

         AchSprite.isVisible = Singleton<PluginManager>.instance.enabledModCount > 0 || sgrs.saveMeta.achievementsDisabled || sgrs.saveMeta.assetRef.package.GetPublishedFileID() != PublishedFileId.invalid;

         //// use new modinfo to show mods active when save was made (?)
         //foreach (ModInfo mi in sgrs.saveMeta.mods)
         //{
         //   //mi.modName;
         //   //mi.modWorkshopID;
         //}
      }
   }
}
