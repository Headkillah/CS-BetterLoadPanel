using System;
using System.Collections.Generic;
using System.Text;
using ICities;
using ColossalFramework;
using ColossalFramework.UI;
using ColossalFramework.Globalization;
using ColossalFramework.Plugins;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Reflection;
using System.ComponentModel;

namespace BetterLoadPanel
{
   public class BetterLoadMod : IUserMod, ILoadingExtension
   {
      private BetterLoadPanelWrapper PanelWrapper = null;
      private BetterLoadPanelWrapper PanelWrapperForPauseMenu = null;
      private static float adjustedLoadButtonHeight = 0f;

      public string Description
      {
         get { return "An enhanced panel for loading saved games."; }
      }

      public string Name
      {
         get { return "BetterLoadPanel"; }
      }

      // default constructor
      public BetterLoadMod()
      {
         try
         {
            if (PluginManager.instance == null || PluginManager.instance.GetPluginsInfo() == null)
            {
               //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "BetterLoadPanel quitting, PluginManager.instance is null");
               return;
            }

            DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "BetterLoadPanel initializing");

            // the very first thing we need to do is check if we're enabled.  This constructor 
            // is called even if we're marked as not to be loaded, so if not enabled, don't do anything
            PluginManager.PluginInfo myPluginInfo = null;
            foreach (PluginManager.PluginInfo info in PluginManager.instance.GetPluginsInfo())
            {
               //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "pugins: " + info.name + ", id: " + info.publishedFileID.ToString());

               if (info.name == "BetterLoadPanel" || info.publishedFileID.AsUInt64 == 413584409)
               {
                  myPluginInfo = info;
                  break;
               }
            }

            if (myPluginInfo == null)
            {
               DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "BetterLoadPanel plugin not found");

               return;
            }

            // we need to be notified if our mod is disabled, so we can remove our added buttons
            // this also handles being enabled, and re-adding buttons
            PluginManager.instance.eventPluginsChanged += () => { this.EvaluateStatus(); };
            PluginManager.instance.eventPluginsStateChanged += () => { this.EvaluateStatus(); };

            // register for locale changes so we can updat button
            LocaleManager.eventLocaleChanged += () => { this.LocaleHasChanged(); };

            // create our panel now
            PanelWrapper = (BetterLoadPanelWrapper)UIView.GetAView().AddUIComponent(typeof(BetterLoadPanelWrapper));

            PanelWrapper.isVisible = false;
            PanelWrapper.Initialize();

            if (!myPluginInfo.isEnabled)
            {
               DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "BetterLoadPanel disabled");
               return;
            }
            
            DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "BetterLoadPanel enabled");

            if (!IsMainMenuUpdated())
            {
               AddMainMenuButton();
            }
         }
         catch (Exception ex)
         {
            DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Error, "BetterLoadPanel exception thrown in constructor: " + ex.Message);
         }
      }

      // event handler for plugin change
      public void EvaluateStatus()
      {
         try
         {
            PluginManager.PluginInfo myPluginInfo = null;
            foreach (PluginManager.PluginInfo info in PluginManager.instance.GetPluginsInfo())
            {
               if (info.name == "BetterLoadPanel")
               {
                  myPluginInfo = info;
                  break;
               }
            }

            if (myPluginInfo == null || !myPluginInfo.isEnabled)
            {
               // disable ourselves
               if (IsMainMenuUpdated())
               {
                  RemoveMainMenuButton();
               }
               if (IsPauseMenuUpdated())
               {
                  RemovePauseMenuButton();
               }
            }
            else
            {
               // enable ourselves (if not already enabled)
               if (!IsMainMenuUpdated())
               {
                  AddMainMenuButton();
               }
               //if (!IsPauseMenuUpdated())
               //{
               //   AddPauseMenuButton();
               //}
            }
         }
         catch (Exception ex)
         {
            DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Error, "evaluate: " + ex.Message);
         }
      }

      // event handler for locale changed
      public void LocaleHasChanged()
      {
         if (IsMainMenuUpdated())
         {
            RemoveMainMenuButton();
            AddMainMenuButton();
            PanelWrapper.LocaleChange();
         }

         if (IsPauseMenuUpdated())
         {
            RemovePauseMenuButton();
            AddPauseMenuButton();
            PanelWrapperForPauseMenu.LocaleChange();
         }
      }

      public bool IsMainMenuUpdated()
      {
         bool retval = false;

         UIComponent mmUIComp = MainMenu.m_MainMenu;

         if (mmUIComp != null)
         {
            UIButton ourButton = mmUIComp.Find<UIButton>("BetterLoadPanel");

            if (ourButton != null)
            {
               retval = true;
            }
         }

         return retval;
      }

      public bool IsPauseMenuUpdated()
      {
         UIButton ourButton = UIView.Find<UIButton>("BetterLoadPanel2");

         return ourButton != null; 
      }


      public void RemoveMainMenuButton()
      {
         UIComponent mmUIComp = MainMenu.m_MainMenu;

         if (mmUIComp != null)
         {
            UIButton ourButton = mmUIComp.Find<UIButton>("BetterLoadPanel");

            if (ourButton != null)
            {
               mmUIComp.RemoveUIComponent(ourButton);
               UnityEngine.Component.DestroyImmediate(ourButton);
            }
         }
      }

      public void RemovePauseMenuButton()
      {
         UIButton ourButton = UIView.Find<UIButton>("BetterLoadPanel2");

         if (ourButton != null)
         {
            ourButton.parent.RemoveUIComponent(ourButton);
            UnityEngine.Component.DestroyImmediate(ourButton);

         }
      }


      public void AddMainMenuButton()
      {
         // find main menu panel
         UIComponent mmUIComp = MainMenu.m_MainMenu;

         if (mmUIComp != null)
         {
            //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, string.Format("found main menu UIComponent {0}", mmUIComp.GetType().FullName));

            UIButton loadGameButton = mmUIComp.Find<UIButton>("LoadGame");

            if (loadGameButton != null)
            {
               // we don't want to clone or copy existing button, as it has event handlers we can't get rid of
               UIButton newButton = UnityEngine.Object.Instantiate<UIButton>(loadGameButton);//mmUIComp.AddUIComponent<UIButton>(); //UnityEngine.Object.Instantiate<UIButton>(loadGameButton);
               newButton.height = (adjustedLoadButtonHeight == 0f) ? loadGameButton.height / 2 : adjustedLoadButtonHeight;
               newButton.width = loadGameButton.width;

               if (adjustedLoadButtonHeight == 0f)
               {
                  // only do this once
                  loadGameButton.height = loadGameButton.height / 2;
                  adjustedLoadButtonHeight = loadGameButton.height;
               }

               newButton.name = "BetterLoadPanel";
               newButton.cachedName = "BetterLoadPanel";
               newButton.stringUserData = "BetterLoadPanel";

               newButton.transform.parent = mmUIComp.transform;
               newButton.text = string.Format("{0}++", Locale.Get(LocaleID.LOADGAME_TITLE)); // locale is subtly different than what game uses...
               newButton.textColor = new Color32(0xFF, 0, 0, 0xFF);
               newButton.relativePosition = Vector3.zero;

               newButton.transform.SetSiblingIndex(loadGameButton.transform.GetSiblingIndex());

               // get rid of click handler that is hooked up as a result of cloning.  Note - probably not necessary, the issue was in the name and cachedname of the new button...
               RemoveClickEvent(newButton);

               // hook up our click handler
               newButton.eventClick += (component, param) => { PanelWrapper.Show(true); };

               mmUIComp.PerformLayout();

               //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "BetterLoadPanel enabled");
            }
            else
            {
               //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "could not find button called LoadGame");
            }
         }
         else
         {
            //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "didn't find main menu");
         }
      }

      public void AddPauseMenuButton()
      {
         // find library pause menu
         //UIDynamicPanels.DynamicPanelInfo[] libpanels = UIView.library.m_DynamicPanels;
         
         //foreach (UIDynamicPanels.DynamicPanelInfo dpi in libpanels)
         //{
         //   DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "panel: " + dpi.name);
         //}

         // create our panel now
         if (PanelWrapperForPauseMenu == null)
         {
            PanelWrapperForPauseMenu = (BetterLoadPanelWrapper)UIView.GetAView().AddUIComponent(typeof(BetterLoadPanelWrapper));

            PanelWrapperForPauseMenu.isVisible = false;
            PanelWrapperForPauseMenu.Initialize();
         }

         UIComponent pMenu = UIView.GetAView().FindUIComponent("Menu");
         //DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "uiview instanceid: " + pMenu.GetUIView().GetInstanceID().ToString());
         if (pMenu != null)
         {
            //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, string.Format("found pause menu UIComponent {0}", pMenu.GetType().FullName));
            
            UIButton loadGameButton = pMenu.Find<UIButton>("LoadGame");

            if (loadGameButton != null)
            {
               // we don't want to clone or copy existing button, as it has event handlers we can't get rid of
               UIButton newButton = UnityEngine.Object.Instantiate<UIButton>(loadGameButton);//mmUIComp.AddUIComponent<UIButton>(); //UnityEngine.Object.Instantiate<UIButton>(loadGameButton);
               newButton.height = loadGameButton.height / 2;
               newButton.width = loadGameButton.width;
               loadGameButton.height = loadGameButton.height / 2;
               newButton.autoSize = true;
               //newButton.useGUILayout = true;
               
               newButton.name = "BetterLoadPanel2";
               newButton.cachedName = "BetterLoadPanel2";
               newButton.stringUserData = "BetterLoadPanel2";

               //pMenu.AttachUIComponent(newButton.gameObject);
               newButton.transform.parent = loadGameButton.transform.parent.transform;               
               
               newButton.text = string.Format("{0}++", Locale.Get(LocaleID.LOADGAME_TITLE)); // locale is subtly different than what game uses...//loadGameButton.text);//
               newButton.textColor = new Color32(0xFF, 0, 0, 0xFF);
               newButton.relativePosition = Vector3.zero;
               newButton.horizontalAlignment = UIHorizontalAlignment.Center;
               newButton.textHorizontalAlignment = UIHorizontalAlignment.Center;
               newButton.verticalAlignment = UIVerticalAlignment.Middle;
               newButton.textVerticalAlignment = UIVerticalAlignment.Middle;

               newButton.transform.SetSiblingIndex(loadGameButton.transform.GetSiblingIndex());

               // get rid of click handler that is hooked up as a result of cloning.  Note - probably not necessary, the issue was in the name and cachedname of the new button...
               RemoveClickEvent(newButton);

               
               // when hiding, pop modal that we pushed in eventClick handler
               PanelWrapperForPauseMenu.eventVisibilityChanged += (component, visible) => 
               {
                  if (visible)
                  {
                     PanelWrapperForPauseMenu.Refresh(); //need to ensure up to date
                     PanelWrapperForPauseMenu.Focus();
                     //DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "eventvisibilitychanged1");
                  }
                  else
                  {
                     ////UIView.library.Hide(this.GetType().Name, -1);
                     //UIView.PopModal(-1);
                     //UIView.GetModalComponent().Focus();

                     //DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "eventvisibilitychanged2");
                  }
                  //if (UIView.GetModalComponent() == PanelWrapperForPauseMenu)
                  //{
                  //   UIView.PopModal();
                  //}
               };

               // hook up our click handler
               newButton.eventClick += (component, param) =>
               {
                  //DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "eventclick1");
                  //UIView.PushModal(PanelWrapperForPauseMenu, (UIView.ModalPoppedReturnCallback)((comp, ret) =>
                  //{
                  //   DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "eventclick2");
                  //   if (ret != 1)
                  //      return;
                  //   DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "eventclick3");
                  //   UIView.library.Hide(typeof(PauseMenu).Name, -1);
                  //}));

                  UIView.library.Hide(typeof(PauseMenu).Name);
                  //DebugOutputPanel.AddMessage(PluginManager.MessageType.Message, "eventclick4");
                  PanelWrapperForPauseMenu.Show(true);
                  
               };

               //pMenu.PerformLayout();

               //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "BetterLoadPanel enabled");
            }
            else
            {
               //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "could not find button called LoadGame in pause menu");
            }
         }
         else
         {
            //DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, "didn't find pause menu");
         }
      }

      private void RemoveClickEvent(UIButton b)
      {
         try
         {
            FieldInfo f1 = typeof(UIComponent).GetField("eventClick", BindingFlags.NonPublic | BindingFlags.Instance);
            EventInfo e1 = typeof(UIComponent).GetEvent("eventClick", BindingFlags.Public | BindingFlags.Instance);
            
            if (f1 == null)
            {
               return;
            }

            if (e1 == null)
            {
               return;
            }

            MulticastDelegate d1 = f1.GetValue(b as UIComponent) as MulticastDelegate;

            if (d1 != null)
            {
               e1.RemoveEventHandler(b, d1);
            }
         }
         catch (Exception ex)
         {
            DebugOutputPanel.AddMessage(ColossalFramework.Plugins.PluginManager.MessageType.Message, string.Format("removeclickevent() exception: {0}", ex.Message));
         }
      }

      public void OnCreated(ILoading loading)
      {
      }

      public void OnLevelLoaded(LoadMode mode)
      {
         if (mode == LoadMode.LoadGame || mode == LoadMode.NewGame)
         {
            if (!IsPauseMenuUpdated())
            {
               this.AddPauseMenuButton();
            }
         }
      }

      public void OnLevelUnloading()
      {
      }

      public void OnReleased()
      {
      }
   }
}

