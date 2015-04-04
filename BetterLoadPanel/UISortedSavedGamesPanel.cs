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
using ColossalFramework.Packaging;
using ColossalFramework.Plugins;
using ColossalFramework.Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace BetterLoadPanel
{
   public class SaveGameRowStruct : IEquatable<SaveGameRowStruct>, IComparable<SaveGameRowStruct>
   {      
      public UIPanel RowPanel;
      public UILabel CityLabel;
      public UILabel SaveLabel;
      public UILabel TimestampLabel;
      public SaveGameMetaData saveMeta;
      public string SaveName;
      public string CityName;
      public DateTime SaveTimestamp;
      public bool IsSteam;

      // also, keep filename so we can launch easily later
      public string PathOnDisk;     //!! this may not be available from Package.Asset

      public string SaveNameForCompare
      {
         get { return IsSteam ? "_steam_"+SaveName : SaveName; }
      }

      public SaveGameRowStruct()
      {
      }

      public SaveGameRowStruct(Package.Asset asset, SaveGameMetaData sgmd)
      {
         SaveName = asset.name;
         CityName = sgmd.cityName;
         SaveTimestamp = sgmd.timeStamp;
         PathOnDisk = asset.pathOnDisk;
         
         saveMeta = sgmd;

         if (saveMeta != null && saveMeta.assetRef != null && saveMeta.assetRef.package != null && PackageManager.IsSteamCloudPath(saveMeta.assetRef.package.packagePath))
         {
            IsSteam = true;
         }
      }


      // default comparer uses SaveName
      public int CompareTo(SaveGameRowStruct other)
      {
         // A null value means that this object is greater. 
         if (other == null)
            return 1;
         else
            return this.SaveNameForCompare.CompareTo(other.SaveNameForCompare);         
      }

      public static int SortBySaveNameAscending(SaveGameRowStruct struct1, SaveGameRowStruct struct2)
      {
         if (struct1 == null)
         {
            if (struct2 == null)
               return 0; 
            else
               return -1;
         }
         else
         {
            if (struct2 == null)
               return 1;
            else
               return struct1.SaveNameForCompare.CompareTo(struct2.SaveNameForCompare);            
         }
      }

      public static int SortBySaveNameDescending(SaveGameRowStruct struct1, SaveGameRowStruct struct2)
      {
         if (struct1 == null)
         {
            if (struct2 == null)
               return 0;
            else
               return -1;
         }
         else
         {
            if (struct2 == null)
               return 1;
            else
               return struct2.SaveNameForCompare.CompareTo(struct1.SaveNameForCompare);
         }
      }

      public static int SortByCityNameAscending(SaveGameRowStruct struct1, SaveGameRowStruct struct2)
      {
         if (struct1 == null)
         {
            if (struct2 == null)
               return 0;
            else
               return -1;
         }
         else
         {
            if (struct2 == null)
               return 1;
            else
               return struct1.CityName.CompareTo(struct2.CityName);
         }
      }

      public static int SortByCityNameDescending(SaveGameRowStruct struct1, SaveGameRowStruct struct2)
      {
         if (struct1 == null)
         {
            if (struct2 == null)
               return 0;
            else
               return -1;
         }
         else
         {
            if (struct2 == null)
               return 1;
            else
               return struct2.CityName.CompareTo(struct1.CityName);
         }
      }

      public static int SortByTimestampAscending(SaveGameRowStruct struct1, SaveGameRowStruct struct2)
      {
         if (struct1 == null)
         {
            if (struct2 == null)
               return 0;
            else
               return -1;
         }
         else
         {
            if (struct2 == null)
               return 1;
            else
               return struct1.SaveTimestamp.CompareTo(struct2.SaveTimestamp);
         }
      }

      public static int SortByTimestampDescending(SaveGameRowStruct struct1, SaveGameRowStruct struct2)
      {
         if (struct1 == null)
         {
            if (struct2 == null)
               return 0;
            else
               return -1;
         }
         else
         {
            if (struct2 == null)
               return 1;
            else
               return struct2.SaveTimestamp.CompareTo(struct1.SaveTimestamp);
         }
      }


      // equality on SaveName
      public bool Equals(SaveGameRowStruct other)
      {
         if (other == null) return false;
         return (this.SaveNameForCompare.Equals(other.SaveNameForCompare));
      }
   }

   // title bar with descriptive lable (non editable) and close button
   // holds draggable handle for moving window
   class UISortedSavedGamesPanel : UIPanel
   {
      public BetterLoadPanelWrapper ParentBetterLoadPanelWrapper;

      public UILabel SaveNameLabel;
      public UILabel CityNameLabel;
      public UILabel TimestampLabel;

      public UIButton SortAsc_SaveName;
      public UIButton SortAsc_CityName;
      public UIButton SortAsc_Timestamp;

      public UIScrollablePanel SavesPanel;
      public UIScrollbar vertSB;
      public UISlicedSprite tracSprit;
      public UISlicedSprite thumbSprit;

      public List<SaveGameRowStruct> GamesList = new List<SaveGameRowStruct>();
      public int CurrentSelected = 0;
      public ColumnSortingMode CurrentSortingMode = ColumnSortingMode.TimestampDescending; // default

      // runs only once, do internal init here
      public override void Awake()
      {
         base.Awake();

         // Note - parent may not be set yet

         SaveNameLabel = AddUIComponent<UILabel>();
         CityNameLabel = AddUIComponent<UILabel>();
         TimestampLabel = AddUIComponent<UILabel>();

         SortAsc_SaveName = AddUIComponent<UIButton>();
         //SortDesc_SaveName = AddUIComponent<UIButton>();

         SortAsc_CityName = AddUIComponent<UIButton>();
         //SortDesc_CityName = AddUIComponent<UIButton>();

         SortAsc_Timestamp = AddUIComponent<UIButton>();
         //SortDesc_Timestamp = AddUIComponent<UIButton>();

         //ListOfSaves = AddUIComponent<UIMultipartListBox>();

         SavesPanel = AddUIComponent<UIScrollablePanel>();
         
         // sortable list of row structs
         

         //// some defaults
         //this.height = 400;
         //this.width = 750;
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
         //this.height = ParentBetterLoadPanelWrapper.height - ParentBetterLoadPanelWrapper.TitleSubPanel.height;
         this.isVisible = true;
         this.isEnabled = true;
         this.canFocus = true;
         this.isInteractive = true;
         this.builtinKeyNavigation = true;
         //this.relativePosition = Vector3.zero;
         this.autoLayout = false;
         this.clipChildren = false; //temp
         int inset = 5;

         // something like:
         //
         // [Name]^v    [City]^v   [Timestamp]^v
         //
         int thirds = (int)(this.width / 3f);
         int workingWidth = 0;


         SaveNameLabel.text = Locale.Get(LocaleID.SAVEGAME_TITLE);// "Save Name";
         SaveNameLabel.autoSize = true;
         //SaveNameLabel.width = thirds;
         SaveNameLabel.relativePosition = new Vector3(workingWidth + inset, inset, 0);

         SortAsc_SaveName.relativePosition = new Vector3(SaveNameLabel.relativePosition.x + SaveNameLabel.width + inset, SaveNameLabel.relativePosition.y, 0);
         SortAsc_SaveName.normalFgSprite = "IconUpArrowFocused";
         SortAsc_SaveName.hoveredFgSprite = "IconDownArrowFocused";
         SortAsc_SaveName.foregroundSpriteMode = UIForegroundSpriteMode.Scale;         
         SortAsc_SaveName.size = new Vector2(16, 16);
         SortAsc_SaveName.eventClick += SortAsc_SaveName_eventClick;

         workingWidth += thirds;

         CityNameLabel.text = Locale.Get(LocaleID.CITY_NAME); //"City Name";
         CityNameLabel.autoSize = true;
         //CityNameLabel.width = thirds;
         CityNameLabel.relativePosition = new Vector3(workingWidth, SortAsc_SaveName.relativePosition.y, 0);

         SortAsc_CityName.relativePosition = new Vector3(CityNameLabel.relativePosition.x + CityNameLabel.width + inset, CityNameLabel.relativePosition.y, 0);
         SortAsc_CityName.normalFgSprite = "IconUpArrowDisabled";
         SortAsc_CityName.hoveredFgSprite = "IconUpArrowFocused";
         SortAsc_CityName.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
         SortAsc_CityName.size = new Vector2(16, 16);
         SortAsc_CityName.eventClick += SortAsc_CityName_eventClick;

         workingWidth += thirds;

         TimestampLabel.text = "Timestamp";
         TimestampLabel.autoSize = true;
         //TimestampLabel.width = thirds;
         TimestampLabel.relativePosition = new Vector3(workingWidth, SortAsc_CityName.relativePosition.y, 0);

         SortAsc_Timestamp.relativePosition = new Vector3(TimestampLabel.relativePosition.x + TimestampLabel.width + inset, TimestampLabel.relativePosition.y, 0);
         SortAsc_Timestamp.normalFgSprite = "IconUpArrowDisabled";
         SortAsc_Timestamp.hoveredFgSprite = "IconUpArrowFocused";
         SortAsc_Timestamp.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
         SortAsc_Timestamp.size = new Vector2(16, 16);
         SortAsc_Timestamp.eventClick += SortAsc_Timestamp_eventClick;

         
         SavesPanel.relativePosition = new Vector3(inset, SortAsc_Timestamp.relativePosition.y + SortAsc_Timestamp.height + 2 * inset, 0);
         SavesPanel.backgroundSprite = "GenericPanel";
         SavesPanel.autoSize = false;
         SavesPanel.width = (this.width - 25 - 3*inset);
         SavesPanel.height = (this.height - SavesPanel.relativePosition.y - inset);

         // hackish, but we want these two to line up
         ParentBetterLoadPanelWrapper.DetailsPanel.relativePosition = new Vector3(ParentBetterLoadPanelWrapper.DetailsPanel.relativePosition.x, SavesPanel.relativePosition.y + this.relativePosition.y, 0);

         SavesPanel.autoLayout = true;
         SavesPanel.isInteractive = true;
         SavesPanel.clipChildren = true;
         SavesPanel.useGUILayout = true;
         SavesPanel.canFocus = true;

         SavesPanel.autoLayoutDirection = LayoutDirection.Vertical;
         SavesPanel.autoLayoutPadding = new RectOffset(0, 0, 1, 1);
         SavesPanel.autoLayoutStart = LayoutStart.TopLeft;
         SavesPanel.builtinKeyNavigation = true;//conflicts with onkeypress?
         SavesPanel.scrollWithArrowKeys = false;
         
         
         //SavesPanel.freeScroll = true;
         SavesPanel.scrollWheelDirection = UIOrientation.Vertical;

         vertSB = this.AddUIComponent<UIScrollbar>();
         vertSB.useGUILayout = true;
         
         vertSB.width = 25;//?
         vertSB.height = SavesPanel.height;
         vertSB.orientation = UIOrientation.Vertical;
         vertSB.isInteractive = true;
         vertSB.isVisible = true;
         vertSB.enabled = true;
         vertSB.relativePosition = new Vector3(SavesPanel.width, SavesPanel.relativePosition.y, 0);
         vertSB.minValue = 0;
         vertSB.value = 0;
         vertSB.incrementAmount = 10;
         vertSB.maxValue = SavesPanel.height;

         tracSprit = vertSB.AddUIComponent<UISlicedSprite>();
         tracSprit.relativePosition = Vector2.zero;
         tracSprit.autoSize = true;
         tracSprit.size = vertSB.size;
         tracSprit.fillDirection = UIFillDirection.Horizontal;
         tracSprit.spriteName = "ScrollbarTrack";
         vertSB.trackObject = tracSprit;

         thumbSprit = tracSprit.AddUIComponent<UISlicedSprite>();
         thumbSprit.relativePosition = Vector2.zero;
         thumbSprit.fillDirection = UIFillDirection.Horizontal;
         thumbSprit.autoSize = true;
         thumbSprit.width = tracSprit.width;
         thumbSprit.spriteName = "ScrollbarThumb";         
         vertSB.thumbObject = thumbSprit;

         SavesPanel.verticalScrollbar = vertSB;
         SavesPanel.scrollWheelAmount = 10;

         Refresh();

         SavesPanel.enabled = true;
      }

      public void Resize()
      {
         int inset = 5;
         float thirds = (int)(this.width / 3f);
         int workingWidth = 0;
         SaveNameLabel.relativePosition = new Vector3(workingWidth + inset, inset, 0);
         SortAsc_SaveName.relativePosition = new Vector3(SaveNameLabel.relativePosition.x + SaveNameLabel.width + inset, SaveNameLabel.relativePosition.y, 0);
         workingWidth += (int)thirds;
         CityNameLabel.relativePosition = new Vector3(workingWidth, SortAsc_SaveName.relativePosition.y, 0);
         SortAsc_CityName.relativePosition = new Vector3(CityNameLabel.relativePosition.x + CityNameLabel.width + inset, CityNameLabel.relativePosition.y, 0);
         workingWidth += (int)thirds;
         TimestampLabel.relativePosition = new Vector3(workingWidth, SortAsc_CityName.relativePosition.y, 0);
         SortAsc_Timestamp.relativePosition = new Vector3(TimestampLabel.relativePosition.x + TimestampLabel.width + inset, TimestampLabel.relativePosition.y, 0);
         SavesPanel.relativePosition = new Vector3(inset, SortAsc_Timestamp.relativePosition.y + SortAsc_Timestamp.height + 2 * inset, 0);
         SavesPanel.width = (this.width - 25 - 3 * inset);
         SavesPanel.height = (this.height - SavesPanel.relativePosition.y - inset);
         ParentBetterLoadPanelWrapper.DetailsPanel.relativePosition = new Vector3(ParentBetterLoadPanelWrapper.DetailsPanel.relativePosition.x, SavesPanel.relativePosition.y + this.relativePosition.y, 0);
         vertSB.height = SavesPanel.height;
         vertSB.relativePosition = new Vector3(SavesPanel.width, SavesPanel.relativePosition.y, 0);
         vertSB.maxValue = SavesPanel.height;
         tracSprit.size = vertSB.size;
         thumbSprit.width = tracSprit.width;

         thirds = SavesPanel.width / 3f;
         // 
         foreach (SaveGameRowStruct row in GamesList)
         {
            row.SaveLabel.width = thirds;

            row.CityLabel.relativePosition = new Vector3(thirds, 0, 0);
            row.CityLabel.width = thirds;

            row.TimestampLabel.relativePosition = new Vector3(thirds + thirds, 0, 0);
            row.TimestampLabel.width = thirds;

            row.RowPanel.width = SavesPanel.width;
            row.RowPanel.height = row.SaveLabel.height;//?
         }
      }

      public void LocaleChange()
      {
         int inset = 5;

         SaveNameLabel.text = Locale.Get(LocaleID.SAVEGAME_TITLE);// "Save Name";
         SortAsc_SaveName.relativePosition = new Vector3(SaveNameLabel.relativePosition.x + SaveNameLabel.width + inset, SaveNameLabel.relativePosition.y, 0);

         CityNameLabel.text = Locale.Get(LocaleID.CITY_NAME); //"City Name";
         
         SortAsc_CityName.relativePosition = new Vector3(CityNameLabel.relativePosition.x + CityNameLabel.width + inset, CityNameLabel.relativePosition.y, 0);
      }

      public void Refresh()
      {
         LoadSaveInformation();

         SetSortingMode(CurrentSortingMode);

         // do the actual sort and update the list
         switch (CurrentSortingMode)
         {
            case ColumnSortingMode.CityNameAscending:
               GamesList.Sort(SaveGameRowStruct.SortByCityNameAscending);
               break;
            case ColumnSortingMode.CityNameDescending:
               GamesList.Sort(SaveGameRowStruct.SortByCityNameDescending);
               break;
            case ColumnSortingMode.SaveNameAscending:
               GamesList.Sort(SaveGameRowStruct.SortBySaveNameAscending);
               break;
            case ColumnSortingMode.SaveNameDescending:
               GamesList.Sort(SaveGameRowStruct.SortBySaveNameDescending);
               break;
            case ColumnSortingMode.TimestampAscending:
               GamesList.Sort(SaveGameRowStruct.SortByTimestampAscending);
               break;
            case ColumnSortingMode.TimestampDescending:
               GamesList.Sort(SaveGameRowStruct.SortByTimestampDescending);
               break;
         }

         InitSavesListUI(SavesPanel);

         SetSelected(CurrentSelected);
      }

      protected override void OnKeyDown(UIKeyEventParameter p)
      {
         if (this.builtinKeyNavigation)
         {
            switch (p.keycode)
            {
               case KeyCode.UpArrow:
                  SetSelected(CurrentSelected - 1);
                  break;
               case KeyCode.DownArrow:
                  SetSelected(CurrentSelected + 1);
                  break;
               case KeyCode.Home:
                  SetSelected(0);
                  break;
               case KeyCode.End:
                  SetSelected(GamesList.Count - 1);
                  break;
               //case KeyCode.PageUp:
               //   this.selectedIndex = Mathf.Max(0, this.selectedIndex - Mathf.FloorToInt((this.size.y - (float)this.listPadding.vertical) / (float)this.itemHeight));
               //   break;
               //case KeyCode.PageDown:
               //   this.selectedIndex += Mathf.FloorToInt((this.size.y - (float)this.listPadding.vertical) / (float)this.itemHeight);
               //   break;
               case KeyCode.Escape:
                  // end dialog
                  ParentBetterLoadPanelWrapper.Hide();
                  break;
               case KeyCode.Return:
                  // launch current row
                  LaunchSelectedSaveGame();
                  break;
            }
         }
         base.OnKeyDown(p);
      }

      public void SetSelected(int index)
      {
         if (index < 0 || index >= GamesList.Count)
            return;


         SaveGameRowStruct currentStruct = GamesList[CurrentSelected];

         if (currentStruct != null)
         {
            // unset selected
            currentStruct.RowPanel.backgroundSprite = "";
         }

         currentStruct = GamesList[index];

         if (currentStruct != null)
         {
            CurrentSelected = index;

            // set selected
            currentStruct.RowPanel.backgroundSprite = "ListItemHighlight";

            // notify detail panel about new selected object
            ParentBetterLoadPanelWrapper.DetailsPanel.SetSelected(currentStruct);
         }
      }

      public void LoadSaveInformation()
      {
         GamesList.Clear();

         Package.AssetType[] assetTypeArray = new Package.AssetType[1];
         int index = 0;
         Package.AssetType assetType = UserAssetType.SaveGameMetaData;
         assetTypeArray[index] = assetType;
         foreach (Package.Asset asset in PackageManager.FilterAssets(assetTypeArray))
         {
            if (asset != (Package.Asset)null && asset.isEnabled)
            {
               SaveGameMetaData mmd = asset.Instantiate<SaveGameMetaData>();

               GamesList.Add(new SaveGameRowStruct(asset, mmd));               
            }
         }
         using (List<Package.Asset>.Enumerator enumerator = SaveHelper.GetSavesOnDisk().GetEnumerator())
         {
            while (enumerator.MoveNext())
            {
               Package.Asset current = enumerator.Current;
               SaveGameMetaData saveGameMetaData = new SaveGameMetaData();
               saveGameMetaData.assetRef = current;
               SaveGameMetaData mmd = saveGameMetaData;

               GamesList.Add(new SaveGameRowStruct(current, mmd));
            }
         }
         
         //this.m_SaveList.items = this.GetListingItems();
         //if (this.m_SaveList.items.Length > 0)
         //{
         //   int indexOf = this.FindIndexOf(LoadPanel.m_LastSaveName);
         //   this.m_SaveList.selectedIndex = indexOf == -1 ? 0 : indexOf;
         //   this.m_LoadButton.isEnabled = true;
         //}
         //else
         //   this.m_LoadButton.isEnabled = false;
      }

      public void InitSavesListUI(UIScrollablePanel panel)
      {
         if (panel == null)
            return;
         
         // clear panel
         List<UIComponent> workingChildren = new List<UIComponent>(panel.components);

         foreach (UIComponent child in workingChildren)
         {
            if (child.name == "BetterLoadRowPanel" && child is UIPanel)
            {
               panel.RemoveUIComponent(child);

               UnityEngine.Object.Destroy(child);
            }
         }
         //panel.Reset();

         if (GamesList == null || GamesList.Count <= 0)         
            return;
         
         float thirds = panel.width / 3f;
         // 
         foreach(SaveGameRowStruct row in GamesList)
         {
            UIPanel rowPanel = panel.AddUIComponent<UIPanel>();

            row.RowPanel = rowPanel;
            rowPanel.objectUserData = row;

            rowPanel.name = "BetterLoadRowPanel";
            rowPanel.autoSize = false;

            rowPanel.eventMouseEnter += (component, param) => // was eventMouseHover
            {              
               if ((component as UIPanel).backgroundSprite != "ListItemHighlight")
                  (component as UIPanel).backgroundSprite = "ListItemHover"; 
            };

            rowPanel.eventMouseLeave += (component, param) => 
            {
               if ((component as UIPanel).backgroundSprite != "ListItemHighlight")
                  (component as UIPanel).backgroundSprite = "";
            };

            rowPanel.eventMouseDown += (component, param) => 
            { 
               this.SelectRowObject(component); 
            };

            rowPanel.eventDoubleClick += (component, param) =>
            {
               this.LaunchRowObject(component);
            };

            row.SaveLabel = rowPanel.AddUIComponent<UILabel>();
            row.SaveLabel.text = row.SaveName;
            row.SaveLabel.autoSize = false;
            row.SaveLabel.relativePosition = Vector3.zero;
            row.SaveLabel.width = thirds;
            row.SaveLabel.padding = new RectOffset(3, 3, 0, 0);
            
            if (row.IsSteam)
            {
               row.SaveLabel.processMarkup = true;
               row.SaveLabel.text = "<sprite SteamCloud> " + row.SaveLabel.text;
            }

            row.CityLabel = rowPanel.AddUIComponent<UILabel>();
            row.CityLabel.text = row.CityName;
            row.CityLabel.autoSize = false;
            row.CityLabel.relativePosition = new Vector3(thirds, 0, 0);
            row.CityLabel.width = thirds;
            row.CityLabel.padding = new RectOffset(3, 3, 0, 0);

            row.TimestampLabel = rowPanel.AddUIComponent<UILabel>();
            row.TimestampLabel.text = row.SaveTimestamp.ToString();
            row.TimestampLabel.autoSize = false;
            row.TimestampLabel.relativePosition = new Vector3(thirds + thirds, 0, 0);
            row.TimestampLabel.width = thirds;
            row.TimestampLabel.padding = new RectOffset(3, 3, 0, 0);

            rowPanel.width = panel.width;
            rowPanel.height = row.SaveLabel.height;//?
         }
      }

      public void SelectRowObject(UIComponent component)
      {
         UIPanel panel = component as UIPanel;

         if (panel != null)
         {
            SaveGameRowStruct sgrs = panel.objectUserData as SaveGameRowStruct;

            if (sgrs != null)
            {
               int idx = GamesList.IndexOf(sgrs);

               if (idx >= 0 && idx < GamesList.Count)
               {
                  SetSelected(idx);
               }
            }
         }
      }

      public void LaunchSelectedSaveGame()
      {
         LaunchRowObject(GamesList[CurrentSelected].RowPanel);
      }

      public void LaunchRowObject(UIComponent component)
      {
         UIPanel panel = component as UIPanel;

         if (panel != null)
         {
            SaveGameRowStruct sgrs = panel.objectUserData as SaveGameRowStruct;

            if (sgrs != null)
            {
               if (SavePanel.isSaving || !Singleton<LoadingManager>.exists || Singleton<LoadingManager>.instance.m_currentlyLoading)
                  return;

               //this.CloseEverything();
               WorldInfoPanel.HideAllWorldInfoPanels();

               SaveGameMetaData listingMetaData = sgrs.saveMeta;// this.GetListingMetaData(this.m_SaveList.selectedIndex);
               Package.Asset listingData = sgrs.saveMeta.assetRef;// this.GetListingData(this.m_SaveList.selectedIndex);

               SimulationMetaData ngs = new SimulationMetaData()
               {
                  m_CityName = listingMetaData.cityName,
                  m_updateMode = SimulationManager.UpdateMode.LoadGame//,
                  //m_environment = this.m_forceEnvironment
               };

               if (Singleton<PluginManager>.instance.enabledModCount > 0 || listingData.package.GetPublishedFileID() != PublishedFileId.invalid)
                  ngs.m_disableAchievements = SimulationMetaData.MetaBool.True;

               Singleton<LoadingManager>.instance.LoadLevel(listingData, "Game", "InGame", ngs);

               //UIView.library.Hide(this.GetType().Name, 1);
               ParentBetterLoadPanelWrapper.Hide();
            }
         }
      }


      public void SortAsc_SaveName_eventClick(UIComponent component, UIMouseEventParameter eventParam)
      {
         UIButton button = component as UIButton;

         if (button == null)
            return;

         if (CurrentSortingMode == ColumnSortingMode.SaveNameAscending)
         {
            SetSortingMode(ColumnSortingMode.SaveNameDescending);
            GamesList.Sort(SaveGameRowStruct.SortBySaveNameDescending);
         }
         else
         {
            SetSortingMode(ColumnSortingMode.SaveNameAscending);
            GamesList.Sort(SaveGameRowStruct.SortBySaveNameAscending);
         }

         InitSavesListUI(SavesPanel);
      }

      public void SortAsc_CityName_eventClick(UIComponent component, UIMouseEventParameter eventParam)
      {
         UIButton button = component as UIButton;

         if (button == null)
            return;

         if (CurrentSortingMode == ColumnSortingMode.CityNameAscending)
         {
            SetSortingMode(ColumnSortingMode.CityNameDescending);
            GamesList.Sort(SaveGameRowStruct.SortByCityNameDescending);
         }
         else
         {
            SetSortingMode(ColumnSortingMode.CityNameAscending);
            GamesList.Sort(SaveGameRowStruct.SortByCityNameAscending);
         }

         InitSavesListUI(SavesPanel);
      }

      public void SortAsc_Timestamp_eventClick(UIComponent component, UIMouseEventParameter eventParam)
      {
         UIButton button = component as UIButton;

         if (button == null)
            return;

         if (CurrentSortingMode == ColumnSortingMode.TimestampAscending)
         {
            SetSortingMode(ColumnSortingMode.TimestampDescending);
            GamesList.Sort(SaveGameRowStruct.SortByTimestampDescending);
         }
         else
         {
            SetSortingMode(ColumnSortingMode.TimestampAscending);
            GamesList.Sort(SaveGameRowStruct.SortByTimestampAscending);
         }

         InitSavesListUI(SavesPanel);
      }

      public void SetSortingMode(ColumnSortingMode mode)
      {
         CurrentSortingMode = mode;

         switch (mode)
         {
            case ColumnSortingMode.CityNameAscending:
               SortAsc_SaveName.normalFgSprite = "IconUpArrowDisabled";
               SortAsc_CityName.normalFgSprite = "IconUpArrowFocused";
               SortAsc_Timestamp.normalFgSprite = "IconUpArrowDisabled";

               SortAsc_SaveName.hoveredFgSprite = "IconDownArrowFocused";
               SortAsc_CityName.hoveredFgSprite = "IconDownArrowFocused";
               SortAsc_Timestamp.hoveredFgSprite = "IconDownArrowFocused";
               break;
            case ColumnSortingMode.CityNameDescending:
               SortAsc_SaveName.normalFgSprite = "IconUpArrowDisabled";
               SortAsc_CityName.normalFgSprite = "IconDownArrowFocused";
               SortAsc_Timestamp.normalFgSprite = "IconUpArrowDisabled";

               SortAsc_SaveName.hoveredFgSprite = "IconDownArrowFocused";
               SortAsc_CityName.hoveredFgSprite = "IconUpArrowFocused";
               SortAsc_Timestamp.hoveredFgSprite = "IconDownArrowFocused";
               break;
            case ColumnSortingMode.SaveNameAscending:
               SortAsc_SaveName.normalFgSprite = "IconUpArrowFocused";
               SortAsc_CityName.normalFgSprite = "IconUpArrowDisabled";
               SortAsc_Timestamp.normalFgSprite = "IconUpArrowDisabled";

               SortAsc_SaveName.hoveredFgSprite = "IconDownArrowFocused";
               SortAsc_CityName.hoveredFgSprite = "IconDownArrowFocused";
               SortAsc_Timestamp.hoveredFgSprite = "IconDownArrowFocused";
               break;
            case ColumnSortingMode.SaveNameDescending:
               SortAsc_SaveName.normalFgSprite = "IconDownArrowFocused";
               SortAsc_CityName.normalFgSprite = "IconUpArrowDisabled";
               SortAsc_Timestamp.normalFgSprite = "IconUpArrowDisabled";

               SortAsc_SaveName.hoveredFgSprite = "IconUpArrowFocused";
               SortAsc_CityName.hoveredFgSprite = "IconDownArrowFocused";
               SortAsc_Timestamp.hoveredFgSprite = "IconDownArrowFocused";
               break;
            case ColumnSortingMode.TimestampAscending:
               SortAsc_SaveName.normalFgSprite = "IconUpArrowDisabled";
               SortAsc_CityName.normalFgSprite = "IconUpArrowDisabled";
               SortAsc_Timestamp.normalFgSprite = "IconUpArrowFocused";

               SortAsc_SaveName.hoveredFgSprite = "IconDownArrowFocused";
               SortAsc_CityName.hoveredFgSprite = "IconDownArrowFocused";
               SortAsc_Timestamp.hoveredFgSprite = "IconDownArrowFocused";
               break;
            case ColumnSortingMode.TimestampDescending:
               SortAsc_SaveName.normalFgSprite = "IconUpArrowDisabled";
               SortAsc_CityName.normalFgSprite = "IconUpArrowDisabled";
               SortAsc_Timestamp.normalFgSprite = "IconDownArrowFocused";

               SortAsc_SaveName.hoveredFgSprite = "IconDownArrowFocused";
               SortAsc_CityName.hoveredFgSprite = "IconDownArrowFocused";
               SortAsc_Timestamp.hoveredFgSprite = "IconUpArrowFocused";
               break;
         }
      }

      public ColumnSortingMode GetSortingMode()
      {
         return CurrentSortingMode;
      }

      public enum ColumnSortingMode
      {
         SaveNameAscending = 0,
         SaveNameDescending = 1,
         CityNameAscending = 2,
         CityNameDescending = 3,
         TimestampAscending = 4,
         TimestampDescending = 5
      }
   }
}
