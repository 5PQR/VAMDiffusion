using System;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using MVR.FileManagementSecure;
using UnityEngine.UI;
using MeshVR;
using System.Net;
using System.Text;
using UnityEngine.Events;
using Mono.CSharp;
using UnityEngine.EventSystems;
﻿
namespace SPQRVamDiffusion
{
    public class VamDiffusion : MVRScript
    {
        string pluginName_p67 = "VamDiffusion";
        string pluginSubTitle_p67 = "PATREON.COM/SPQR_AETERNUM";
        string dirPresets_p67 = @"Saves/PluginData/VamDiffusion/";
        string screenshotsFolderName_p67 = @"VamDiffusion";
        int version_p67 = 1;
        bool enableDefaultsLogic = true;
        float previewScale = 0.9f;
        MVRScriptUI mvrscriptui;
        string diffUrl_p67 = "";
        float SetDiffApiUrlLast_p67 = 0f;
        bool isA1111Connected = false;
        bool issceneLoaded_p67 = false;
        bool isPluginStarted_p67 = false;
        string uniquePluginInstanceId = "";        
		Dictionary<string, JSONStorableBool> bools_p67 = new Dictionary<string, JSONStorableBool>();
        Dictionary<string, JSONStorableString> strings_p67 = new Dictionary<string, JSONStorableString>();
        Dictionary<string, JSONStorableString> stringsEditable_p67 = new Dictionary<string, JSONStorableString>();
        Dictionary<string, UIDynamicButton> buttons_p67 = new Dictionary<string, UIDynamicButton>();
        Dictionary<string, JSONStorableFloat> sliders_p67 = new Dictionary<string, JSONStorableFloat>();
        Dictionary<string, JSONStorableStringChooser> choosers_p67 = new Dictionary<string, JSONStorableStringChooser>();
        Dictionary<string, JSONStorableColor> colors_p67 = new Dictionary<string, JSONStorableColor>();
        Dictionary<string, JSONStorableString> titles_p67 = new Dictionary<string, JSONStorableString>();
        Dictionary<string, UIDynamicTextField> titlesFields_p67 = new Dictionary<string, UIDynamicTextField>();
        Dictionary<string, InputField> cacheInputFields = new Dictionary<string, InputField>();
        List<string> cacheInputFieldsKeys;
        string tmpSelectedTab_p67 = "";
        List<string> myTabsKeys_p67;
        Dictionary<string, bool> myTabsInit_p67 = new Dictionary<string, bool>() { };
        Dictionary<string, Action<string, bool>> myTabs_p67;
        Color defaultTitleBackground = new Color(0.427f, 0.035f, 0.517f);
        Color defaultTitleColor = new Color(1f, 1f, 1f, 0.7f);
        Color secondTitleBackground = new Color(1f, 1f, 1f, 0.1f);
        Color secondTitleColor = new Color(1f, 1f, 1f, 0.7f);
        Color colorPurple = new Color(0.42f, 0.03f, 0.51f, 1f);
        byte[] imageLogo, imageLoading;
        JSONNode cacheDiffusionOptions_p67;
        bool cacheDiffusionOptionsChanged_p67 = false;
        void DeclareMyTabs_p67()
        {
            myTabs_p67 = new Dictionary<string, Action<string, bool>>
            {
                { "Prompt", (t, b) =>  TabPrompt_p67(CheckSelected_p67(t),b) },
            };
        }
        void CheckApi()
        {
            if (titlesFields_p67.ContainsKey("titlepluginname2"))
            {
                titlesFields_p67["titlepluginname2"].text = "AUTOMATIC1111 status:\n <b><color=#ffff66>CHECKING</color></b>";
            }
            SetDiffApiUrl_p67();
        }
        List<string> listOptionsModels = new List<string> { "Select..." };
        List<string> listOptionsSamplers = new List<string> { "Select..." };
        List<string> listOptionsControlnet = new List<string> { "Select...", "None" };
        string currentModelSelected = "";
        UIDynamicPopup tmpPopupModel, tmpPopupSampler, tmpPopupControlnet;
        void CallbackSetChooserModel(string s) {
            SetCurrentChooser("Model", s);
        }
        void CallbackSetChooserSampler(string s)
        {
            SetCurrentChooser("Sampler", s);
        }
        void CallbackSetChooserControlnet(string s)
        {
            SetCurrentChooser("Model", s);
        }
        void TabPrompt_p67(bool isShow, bool isDeclare = false)
        {
            if (isDeclare)
            {
                strings_p67.Add("Prompt", new JSONStorableString("Prompt", "beautiful photo", EditableTextCallback()));
                RegisterString(strings_p67["Prompt"]); 
                strings_p67.Add("Negative Prompt", new JSONStorableString("Negative Prompt", "", EditableTextCallback()));
                RegisterString(strings_p67["Negative Prompt"]);
                strings_p67.Add("Seed", new JSONStorableString("Seed", "-1", EditableTextCallback()));
                RegisterString(strings_p67["Seed"]);
                sliders_p67.Add("steps", new JSONStorableFloat("Steps", 13f, 1f, 100f, true));
                RegisterFloat(sliders_p67["steps"]);
                sliders_p67.Add("cfg", new JSONStorableFloat("Guidance", 7f, 1f, 15f, true));
                RegisterFloat(sliders_p67["cfg"]);
                sliders_p67.Add("denoise", new JSONStorableFloat("Denoise", 0.5f, 0f, 1f, true));
                RegisterFloat(sliders_p67["denoise"]);
                sliders_p67.Add("size", new JSONStorableFloat("Max Size", 768f, 512f, 2048f, true));
                RegisterFloat(sliders_p67["size"]);
                choosers_p67["Model"] = new JSONStorableStringChooser("Model", listOptionsModels, "Select...", "Model", CallbackSetChooserModel);
                strings_p67.Add("Current Model", new JSONStorableString("Current Model", "", EditableTextCallback()));
                choosers_p67["Sampler"] = new JSONStorableStringChooser("Sampling method", listOptionsSamplers, "Select...", "Sampling method", CallbackSetChooserSampler);
                strings_p67.Add("Current Sampler", new JSONStorableString("Current Sampler", "", EditableTextCallback()));
                RegisterString(strings_p67["Current Sampler"]);
            }
            if (isShow)
            {
                CreateEditableTextField_p67(strings_p67["Prompt"], false, secondTitleBackground, secondTitleColor, 150);
                CreateEditableTextField_p67(strings_p67["Negative Prompt"], false, secondTitleBackground, secondTitleColor, 150);
                CreateEditableTextField_p67(strings_p67["Seed"], false, secondTitleBackground, secondTitleColor);
                var uds = CreateSlider(sliders_p67["steps"], false);
                uds.slider.wholeNumbers = true;
                uds.quickButtonsEnabled = false;
                uds.defaultButtonEnabled = false;
                var uds2 = CreateSlider(sliders_p67["cfg"], false);
                uds2.slider.wholeNumbers = false;
                uds2.quickButtonsEnabled = false;
                uds2.defaultButtonEnabled = false;
                var uds3 = CreateSlider(sliders_p67["denoise"], false);
                uds3.slider.wholeNumbers = false;
                uds3.quickButtonsEnabled = false;
                uds3.defaultButtonEnabled = false;
                var uds4 = CreateSlider(sliders_p67["size"], false);
                uds4.slider.wholeNumbers = true;
                uds4.quickButtonsEnabled = false;
                uds4.defaultButtonEnabled = false;
                var text = "AUTOMATIC1111 status:\n <b><color=#66ff66>CONNECTED</color></b>";
                if (!isA1111Connected || diffUrl_p67 == "")
                {
                    text = "AUTOMATIC1111 status:\n <b><color=#993333>DISCONNECTED</color>" + diffUrl_p67 + " - " + isA1111Connected + "</b>";
                }
                CreateTitle_p67("titlepluginname2", text, "", 30, true);
                titlesFields_p67["titlepluginname2"].UItext.supportRichText = true;
                titlesFields_p67["titlepluginname2"].UItext.color = Color.white;
                buttons_p67["reloadApi"] = CreateButton("Check API ", true);
                buttons_p67["reloadApi"].button.onClick.AddListener(() =>
                {
                    CheckApi();
                });
                CreateEditableTextField_p67(strings_p67["Current Model"], true, secondTitleBackground, secondTitleColor, 0, true);
                CreateEditableTextField_p67(strings_p67["Current Sampler"], true, secondTitleBackground, secondTitleColor, 0, true);
                tmpPopupModel = CreateFilterablePopup(choosers_p67["Model"], true);
                tmpPopupSampler = CreateFilterablePopup(choosers_p67["Sampler"], true);
            }
            else
            {
                RemoveEditableTextField_p67(strings_p67["Prompt"]);
                RemoveEditableTextField_p67(strings_p67["Negative Prompt"]);
                RemoveEditableTextField_p67(strings_p67["Seed"]);
                RemoveSlider(sliders_p67["steps"]);
                RemoveSlider(sliders_p67["cfg"]);
                RemoveSlider(sliders_p67["denoise"]);
                RemoveSlider(sliders_p67["size"]);
                if (titlesFields_p67.ContainsKey("titlepluginname2"))
                {
                    RemoveTextField(titlesFields_p67["titlepluginname2"]);
                }
                RemovePopup(choosers_p67["Model"]);
                RemoveEditableTextField_p67(strings_p67["Current Model"]);
                RemovePopup(choosers_p67["Sampler"]);
                RemoveEditableTextField_p67(strings_p67["Current Sampler"]);
                if (buttons_p67.ContainsKey("reloadApi"))
                {
                    RemoveButton(buttons_p67["reloadApi"]);
                }
            }
        }
        void SetCurrentChooser(string key, string val)
        {
            if (val != "Select...")
            {
                strings_p67["Current " + key].val = val;
                switch (key)
                {
                    case "Model":
                        LoadDiffusionCheckpoint(val);
                        break;
                }
            }
            choosers_p67[key].val = "Select...";
        }
        void MyTab2_p67(bool isShow, bool isDeclare)
        {
            if (isDeclare)
            {
            }
            if (isShow)
            {
            }
            else
            {
            }
        }
        void MyTabBootstrap_p67(bool isShow, bool isDeclare)
        {
            if (isDeclare)
            {
                strings_p67.Add("string1", new JSONStorableString("Title string1", "default value", (string s) => { SuperController.LogMessage("text:" + s); }));
                RegisterString(strings_p67["string1"]);
                bools_p67.Add("bool1", new JSONStorableBool("Title bool1", true, (bool b) => { SuperController.LogMessage("bool:" + b); }));
                RegisterBool(bools_p67["bool1"]);
                sliders_p67.Add("slider1", new JSONStorableFloat("Title float1", 50f, (float n) => { SuperController.LogMessage("float:" + n); }, 0f, 100f, true));
                RegisterFloat(sliders_p67["slider1"]);
            }
            if (isShow)
            {
                CreateEditableTextField_p67(strings_p67["string1"]);
                CreateToggle(bools_p67["bool1"]);
                CreateSlider(sliders_p67["slider1"]);
            }
            else
            {
                RemoveEditableTextField_p67(strings_p67["string1"]);
                RemoveToggle(bools_p67["bool1"]);
                RemoveSlider(sliders_p67["slider1"]);
            }
        }
        void OnMyPluginStart_p67()
        {
            mvrscriptui = this.UITransform.GetComponentInChildren<MVRScriptUI>();
            CheckApi();
        }
        void OnMyPluginEachFrame_p67()
        {
            CheckScreenshot();
        }
        void OnMyPlugin30FPS_p67()
        {
            if (IsScreenshotMode())
            {
                if (IsLivePreview())
                {
                    DoQuickRender();
                }
            }
        }
        void OnMyPlugin5FPS_p67()
        {
        }
        void OnMyPlugin1FPS_p67()
        {
            if (IsScreenshotMode())
            {
            }
        }
        void OnMyPlugin5Seconds_p67()
        {
        }
        void OnMyPluginDisable_p67()
        {
        }
        void OnMyPluginEnable_p67()
        {
        }
        bool IsScreenshotMode()
        {
            return SuperController.singleton.currentSelectMode == SuperController.SelectMode.Screenshot;
        }
        void CheckScreenshot()
        {
            if (IsScreenshotMode())
            {
                if ((SuperController.singleton.GetRightSelect() || SuperController.singleton.GetLeftSelect() || SuperController.singleton.GetMouseSelect()) && SuperController.singleton.hiResScreenshotCamera != null)
                {
                    string path = string.Concat(str2: ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString(), str0: SuperController.singleton.savesDirResolved, str1: "screenshots\\", str3: ".jpg");
                    string path2 = string.Concat(str2: ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds - 1).ToString(), str0: SuperController.singleton.savesDirResolved, str1: "screenshots\\", str3: ".jpg");
                    Invoke_p67(() => { OnTakeScreenshot(path, path2); }, 0.11f);
                }
            }
        }
        public Vector2 GetImageSize()
        {
            return new Vector2(SuperController.singleton.hiResScreenshotCamera.targetTexture.width, SuperController.singleton.hiResScreenshotCamera.targetTexture.height);
        }
        public Vector2 GetCustomSize(Vector2 imgsize, bool isQuick = false)
        {
            var max = sliders_p67["size"].val;
            if (isQuick) max = (float)thumbSizePreview;
            var ratio = imgsize.x / imgsize.y;
            if (ratio < 1)
            {
                return new Vector2(max * ratio, max);
            } else
            {
                return new Vector2(max, max / ratio);
            }
        }
        int thumbSizePreview = 256;
        bool isLivePreviewEnabled = false;
        float timeLastRealPhoto = 0f;
        bool IsLivePreview()
        {
            if (Time.time - timeLastRealPhoto < 2f) return false;
            return isLivePreviewEnabled;
        }
        public void GetInProgressImage_p67()
        {
            if (!isGeneratingImages) return;
            var url = GetApiPath("sdapi/v1/progress");
            var call1 = new VamDiffusionApi(this, "GET", url, CallbackInProgress_p67);
        }
        bool isGeneratingImages = false ;
        float lastGenerateImageTime = 0f;
        void DoImageApiCall(byte[] inputimg_p67, string newPath, bool isQuick = false)
        {
            try
            {
                if (isGeneratingImages) return;
                var img_p67 = Convert.ToBase64String(inputimg_p67);
                var size = GetImageSize();
                var customsize = GetCustomSize(size, isQuick);
                string jsonPayload = "";
                var finalPrompt_p67 = strings_p67["Prompt"].val;
                var nprompt_p67 =  strings_p67["Negative Prompt"].val;
                var sampler_p67 = strings_p67["Current Sampler"].val;
                var w_p67 = strings_p67["Current Sampler"].val;
                var seed = VamDiffusionHelpers.ToInt_p67(strings_p67["Seed"].val);
                var cfg_p67 = sliders_p67["cfg"].val;
                var denoise_p67 = sliders_p67["denoise"].val;
                var steps_p67 = sliders_p67["steps"].val;
                if (isQuick)
                {
                    if (steps_p67 > 15f) steps_p67 = 15f;
                    else if (steps_p67 < 10f) steps_p67 = Mathf.Min(steps_p67, 7f);
                    if (strings_p67["Current Sampler"].val == "LCM") steps_p67 = 5f;
                    denoise_p67 = denoise_p67 * Mathf.Clamp01((float)thumbSizePreview / sliders_p67["size"].val);
                    SuperController.LogMessage("denoise_p67: " + denoise_p67 + " vs " + sliders_p67["denoise"].val);
                }
                var diffmode_p67 = "normal";
                string jsonBase = "\"prompt\":\"" + finalPrompt_p67 + "\"," +
                    "\"negative_prompt\":\"" + nprompt_p67 + "\"," +
                    "\"sampler_name\":\"" + sampler_p67 + "\"," +
                    "\"width\":" + customsize.x + "," +
                    "\"height\":" + customsize.y + "," +
                    "\"seed\":" + seed + "," +
                    "\"cfg_scale\":" + cfg_p67 + "," +
                    "\"denoising_strength\":" + denoise_p67 + "," +
                    "\"steps\":" + steps_p67;
                string url = "";
                if (diffmode_p67 == "normal")
                {
                    url = GetApiPath("sdapi/v1/img2img");
                    jsonPayload = "{" +
                         "\"init_images\":[\"" + img_p67 + "\"]," +
                         jsonBase +
                     "}";
                }
                var token = newPath;
                if (isQuick)
                {
                    token = "quick";
                } else
                {
                    SetPreviewLogo();
                }
                isGeneratingImages = true;
                LoadingShow();
                lastGenerateImageTime = Time.time;
                var call1 = new VamDiffusionApi(this, "POST", url, CallbackGenerateImage, jsonPayload, null, token);
            } catch (Exception e)
            {
                SuperController.LogError("err: " + e);
            }
        }
        void CustomSaveDefault()
        {
            HandleSavePreset_p67(dirPresets_p67 + "default.json");
        }
        void CustomLoadDefault()
        {
            HandleLoadPreset_p67(dirPresets_p67 + "default.json");
        }
        void LoadDiffusionCheckpoint(string model)
        {
            SetDiffusionSetting("sd_model_checkpoint", model);
        }
        void SetDiffusionSetting(string key, string val)
        {
            var json = new JSONClass();
            json.Add(key, new JSONData(val));
            var url = GetApiPath("sdapi/v1/options");
            new VamDiffusionApi(this, "POST", url, CallbackSettingSaved, json.ToString(), null, "");
        }
        void CallbackSettingSaved(JSONNode json)
        {
            SuperController.LogMessage("AUTOMATIC1111 setting saved!");
        }
        void CallbackGenerateImage(JSONNode json)
        {
            isGeneratingImages = false;
            LoadingHide();
            if (json["images"].AsArray.Count > 0)
            {
                var img64 = json["images"].AsArray[0];
                var img = Convert.FromBase64String(img64);
                if (json["x_token"].Value.Contains("screenshot") )
                {
                    FileManagerSecure.WriteAllBytes(json["x_token"].Value, img);
                    Invoke_p67(() =>{
                        SetBase64Image(diffusionPreview2_p67, img);
                        timeLastRealPhoto = Time.time;
                    }, 0.1f);
                } else if (json["x_token"].Value == "quick")
                {
                    Invoke_p67(() => {
                        SetBase64Image(diffusionPreview2_p67, img);
                        isQuickRender = false;
                    }, 0.1f);
                }
            }
        }
        bool IsEnabledDiffusion()
        {
            if (!isA1111Connected) return false;
            if (diffUrl_p67 == "") return false;
            if (!enabled) return false;
            return true;
        }
        void OnTakeScreenshot(string path , string path2)
        {
            if (!IsEnabledDiffusion()) return;
            try
            {
                if (!FileManagerSecure.FileExists(path))
                {
                    path = path2;
                }
                if (!FileManagerSecure.FileExists(path))
                {
                    SuperController.LogError("Couldn't open screenshot: " + path);
                    return;
                }
                var img = FileManagerSecure.ReadAllBytes(path);
                var pathnew = path.Replace("screenshots\\", "screenshots\\" + screenshotsFolderName_p67 + "\\");
                DoImageApiCall(img, pathnew);
                img = null;
            } 
            catch(Exception e)
            {
                SuperController.LogError("err screenshot:" + e);
            }
        }
        Image diffusionPreview_p67, diffusionPreview2_p67, diffusionPreview3_p67;
        RectTransform diffusionRect_p67, diffusionRect2_p67, diffusionRect3_p67;
        void InitScreenshotPreview()
        {
            try
            {
                var imgs = SuperController.singleton.hiResScreenshotPreview.GetComponentsInChildren<Image>().First(x => x.name == "BackPanel");
                screenshotArea_p67 = imgs.rectTransform;
                var go3 = VamDiffusionHelpers.FindInactive("SPQRDiffusionPreviewArea3");
                if (go3 != null) UnityEngine.GameObject.Destroy(go3);
                var go2 = VamDiffusionHelpers.FindInactive("SPQRDiffusionPreviewArea2");
                if (go2 != null) UnityEngine.GameObject.Destroy(go2);
                var go = VamDiffusionHelpers.FindInactive("SPQRDiffusionPreviewArea");
                if (go != null) UnityEngine.GameObject.Destroy(go);
                GameObject newOverlay = new GameObject("SPQRDiffusionPreviewArea", typeof(RectTransform), typeof(UnityEngine.UI.Image));
                diffusionRect_p67 = newOverlay.GetComponent<RectTransform>();
                newOverlay.name = "SPQRDiffusionPreviewArea";
                diffusionRect_p67.SetParent(screenshotArea_p67, false);
                diffusionRect_p67.anchorMin = Vector2.zero;
                diffusionRect_p67.anchorMax = Vector2.one;
                diffusionRect_p67.offsetMin = Vector2.zero;
                diffusionRect_p67.offsetMax = Vector2.zero;
                diffusionRect_p67.localScale = new Vector3(previewScale, previewScale, 1f);
                diffusionRect_p67.anchoredPosition = new Vector2(screenshotArea_p67.rect.width * 0.5f, 0f);
                diffusionPreview_p67 = newOverlay.GetComponent<Image>();
                diffusionPreview_p67.sprite = null;
                diffusionPreview_p67.material = new Material(Shader.Find("UI/Default-Overlay"));
                diffusionPreview_p67.color = new Color(1f, 1f, 1f, 0.392f);
                GameObject newOverlay2 = new GameObject("SPQRDiffusionPreviewArea2", typeof(RectTransform), typeof(UnityEngine.UI.Image));
                diffusionRect2_p67 = newOverlay2.GetComponent<RectTransform>();
                newOverlay2.name = "SPQRDiffusionPreviewArea2";
                diffusionRect2_p67.SetParent(newOverlay.transform, false);
                diffusionRect2_p67.localScale = new Vector3(0.98f, 0.98f, 1f);
                var padding = 0f;
                diffusionRect2_p67.anchorMin = new Vector2(0f, 0f);
                diffusionRect2_p67.anchorMax = new Vector2(1f, 1f);
                diffusionRect2_p67.offsetMin = new Vector2(padding, padding);
                diffusionRect2_p67.offsetMax = new Vector2(0f, 0f);
                diffusionRect2_p67.anchoredPosition = new Vector2(0f, 0f);
                diffusionPreview2_p67 = newOverlay2.GetComponent<Image>();
                diffusionPreview2_p67.sprite = null;
                diffusionPreview2_p67.color = Color.white;
                diffusionPreview2_p67.material = new Material(Shader.Find("UI/Default-Overlay"));
                GameObject newOverlay3 = new GameObject("SPQRDiffusionPreviewArea3", typeof(RectTransform), typeof(UnityEngine.UI.Image));
                diffusionRect3_p67 = newOverlay3.GetComponent<RectTransform>();
                newOverlay3.name = "SPQRDiffusionPreviewArea3";
                diffusionRect3_p67.SetParent(newOverlay.transform, false);
                diffusionRect3_p67.localScale = new Vector3(0.85f, 0.85f, 1f);
                var padding3 = 0f;
                diffusionRect3_p67.anchorMin = new Vector2(0f, 0f);
                diffusionRect3_p67.anchorMax = new Vector2(1f, 1f);
                diffusionRect3_p67.offsetMin = new Vector2(padding3, padding3);
                diffusionRect3_p67.offsetMax = new Vector2(0f, 0f);
                diffusionRect3_p67.anchoredPosition = new Vector2(0f, 0f);
                diffusionPreview3_p67 = newOverlay3.GetComponent<Image>();
                diffusionPreview3_p67.sprite = null;
                diffusionPreview3_p67.color = new Color(1f, 1f, 1f, 0.22f);
                diffusionPreview3_p67.material = new Material(Shader.Find("UI/Default-Overlay"));
                LoadingHide();
                Invoke_p67(() => { SetPreviewLoading(); }, 1f);
                Invoke_p67(() => { SetPreviewLogo(); }, 2f);
            } catch(Exception e)
            {
                SuperController.LogError("err:" + e);
            }
        }
        void SetPreviewLoading()
        {
            if (imageLoading == null)
            {
                imageLoading = VamDiffusionHelpers.GetVamDiffusionLoading();
            }
            SetBase64Image(diffusionPreview3_p67, imageLoading);
        }
        void SetPreviewLogo()
        {
            if(imageLogo == null)
            {
                imageLogo = VamDiffusionHelpers.GetVamDiffusionLogo();
            }
            SetBase64Image(diffusionPreview2_p67, imageLogo); ;
        }
        RectTransform screenshotArea_p67;
        RectTransform previewScreenshotVam_p67;
        public override void Init()
        {
            try
            {
                previewScreenshotVam_p67 = SuperController.singleton.hiResScreenshotPreview.GetComponent<RectTransform>();
                InitScreenshotPreview();
                if (!FileManagerSecure.DirectoryExists("Saves\\screenshots")) FileManagerSecure.CreateDirectory("Saves\\screenshots");
                if (!FileManagerSecure.DirectoryExists("Saves\\screenshots\\"+screenshotsFolderName_p67)) FileManagerSecure.CreateDirectory("Saves\\screenshots\\"+ screenshotsFolderName_p67);
                DeclareMyTabs_p67();
                myTabsKeys_p67 = new List<string>(myTabs_p67.Keys);
                foreach (var tab_p67 in myTabsKeys_p67)
                {
                    myTabs_p67[tab_p67](tab_p67, true);
                }
                InitPluginTemplate_p67();
                InitPrepareYourUI_p67();
            }
            catch (Exception e)
            {
                SuperController.LogError("e InitPrepareYourUI " + e);
            }
        }
        bool isQuickRender = false;
        void DoQuickRender()
        {
            SuperController.LogMessage("renderQuick");
            if (isQuickRender) return;
            isQuickRender = true;
            byte[] resizedBytes = ResizeTexture(GetTextureFromRenderTexture(SuperController.singleton.hiResScreenshotCamera.targetTexture), thumbSizePreview);
            SuperController.LogMessage("renderQuick2");
            DoImageApiCall(resizedBytes, "", true);
        }
        Texture2D GetTextureFromRenderTexture(RenderTexture renderTexture)
        {
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTexture;
            Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture.Apply();
            RenderTexture.active = previous;
            return texture;
        }
        public static byte[] ResizeTexture(Texture2D texture, int maxSize = 256)
        {
            int width = texture.width;
            int height = texture.height;
            float maxDimension = Mathf.Max(width, height);
            if (maxDimension > maxSize)
            {
                float scale = maxSize / maxDimension;
                width = Mathf.RoundToInt(width * scale);
                height = Mathf.RoundToInt(height * scale);
            }
            Texture2D resizedTexture = new Texture2D(width, height, texture.format, texture.mipmapCount > 0);
            Color[] pixels = texture.GetPixels();
            Color[] resizedPixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / width * texture.width;
                    float v = (float)y / height * texture.height;
                    resizedPixels[y * width + x] = texture.GetPixelBilinear(u / texture.width, v / texture.height);
                }
            }
            resizedTexture.SetPixels(resizedPixels);
            resizedTexture.Apply();
            return resizedTexture.EncodeToJPG(60);
        }
        void LoadingShow()
        {
            diffusionPreview3_p67.color = new Color(1f, 1f, 1f, 0.22f);
        }
        void LoadingHide()
        {
            diffusionPreview3_p67.color = new Color(1f, 1f, 1f, 0f);
        }
        void Update()
        {
            if (cacheDiffusionOptionsChanged_p67 && cacheDiffusionOptions_p67["sd_model_checkpoint"] != null)
            {
                cacheDiffusionOptionsChanged_p67 = false;
                strings_p67["Current Model"].val = cacheDiffusionOptions_p67["sd_model_checkpoint"].Value;
            }
            if(Time.time - lastGenerateImageTime > 60)
            {
                isGeneratingImages = false;
                LoadingHide();
            }
            if (IsScreenshotMode())
            {
                if (diffusionRect_p67 != null)
                {
                    if (screenshotArea_p67 != null)
                    {
                        diffusionRect_p67.anchoredPosition = new Vector2(screenshotArea_p67.rect.width * 0.5f + (diffusionRect_p67.rect.width * 0.5f * previewScale), 0f);
                    }
                }
                if (isGeneratingImages)
                {
                    diffusionRect3_p67.Rotate(new Vector3(0f, 0f, -90f * Time.deltaTime));
                }
            }
            if (IsDisabled()) return;
            try
            {
                OnMyPluginEachFrame_p67();
            }
            catch (Exception e)
            {
                SuperController.LogError("error update: " + e);
            }
        }
        void FixedUpdate()
        {
            if (IsDisabled()) return;
        }
        void UpdateSecond()
        {
            if (IsDisabled()) return;
            try
            {
                OnMyPlugin1FPS_p67();
            }
            catch (Exception e)
            {
                SuperController.LogError("error OnMyPlugin1FPS: " + e);
            }
        }
        void Update5Second()
        {
            if (IsDisabled()) return;
            try
            {
                OnMyPlugin5Seconds_p67();
            }
            catch (Exception e)
            {
                SuperController.LogError("error OnMyPlugin5Seconds: " + e);
            }
        }
        void UpdateConstant()
        {
            if (IsDisabled()) return;
            try
            {
                OnMyPlugin30FPS_p67();
            }
            catch (Exception e)
            {
                SuperController.LogError("error OnMyPlugin30FPS: " + e);
            }
        }
        void UpdateConstant5()
        {
            if (IsDisabled()) return;
            try
            {
                OnMyPlugin5FPS_p67();
            }
            catch (Exception e)
            {
                SuperController.LogError("error OnMyPlugin5FPS: " + e);
            }
        }
        void Start()
        {
            try
            {
                if (enableDefaultsLogic)
                {
                    CustomLoadDefault();
                }
                OnMyPluginStart_p67();
                OnMyPluginEnable_p67();
            }
            catch (Exception e)
            {
                SuperController.LogError("error OnMyPluginStart: " + e);
            }
        }
        void InitPrepareYourUI_p67()
        {
            try
            {
                foreach (var tab_p67 in myTabsKeys_p67)
                {
                    myTabs_p67[tab_p67](tab_p67, false);
                }
            }
            catch (Exception e)
            {
                SuperController.LogError("error InitPrepareYourUI: " + tmpSelectedTab_p67 + " : " + e);
            }
        }
        void InitPluginTemplate_p67()
        {
            uniquePluginInstanceId = pluginName_p67 + "_" + containingAtom.GetInstanceID().ToString() + "_";
            InvokeRepeating("Update5Second", 1f, 5f);
            InvokeRepeating("UpdateSecond", 1f, 1f);
            InvokeRepeating("UpdateConstant", 1f, 1f / 30f);
            InvokeRepeating("UpdateConstant5", 1f, 1f / 5f);
            SuperController.singleton.onSceneLoadedHandlers += OnSceneLoaded;
            if (!FileManagerSecure.DirectoryExists(dirPresets_p67)) FileManagerSecure.CreateDirectory(dirPresets_p67);
            CreateSpacer_p67(0f, false);
            CreateTitle_p67("titlepluginname", pluginName_p67 + " v" + version_p67, pluginSubTitle_p67, 32, false);
            CreateSpacer_p67(5f, false);
            CreateSpacer_p67(0f, true);
            tmpSelectedTab_p67 = myTabsKeys_p67[0];
            var udp_p67 = CreateFilterablePopup(new JSONStorableStringChooser("Selected", myTabsKeys_p67, myTabsKeys_p67[0], "", SetSelectedTab_p67), true);
            udp_p67.labelWidth = 0f;
            udp_p67.labelTextColor = new Color(0f, 0f, 0f, 0f);
            udp_p67.gameObject.GetComponentInChildren<Image>().color = new Color(0f, 0f, 0f, 0f);
            udp_p67.gameObject.GetComponentInChildren<Button>().gameObject.GetComponent<Image>().color = VamDiffusionHelpers.GetColorDeepPurple_p67();
            udp_p67.gameObject.GetComponentInChildren<Button>().gameObject.GetComponentInChildren<Text>().color = Color.white;
            buttons_p67["save_preset"] = CreateButton("Save Preset", false);
            buttons_p67["save_preset"].button.onClick.AddListener(() =>
            {
                var folder_p67 = dirPresets_p67;
                folder_p67 = VamDiffusionHelpers.RemoveTrailingSlash_p67(folder_p67);
                var fileBrowserUI_p67 = SuperController.singleton.fileBrowserUI;
                fileBrowserUI_p67.GotoDirectory(@"/");
                fileBrowserUI_p67.SetTitle("Save preset");
                fileBrowserUI_p67.fileRemovePrefix = null;
                fileBrowserUI_p67.hideExtension = true;
                fileBrowserUI_p67.keepOpen = false;
                fileBrowserUI_p67.canCancel = true;
                fileBrowserUI_p67.fileFormat = "json";
                fileBrowserUI_p67.defaultPath = folder_p67;
                fileBrowserUI_p67.showDirs = false;
                fileBrowserUI_p67.showFiles = true;
                fileBrowserUI_p67.shortCuts = null;
                fileBrowserUI_p67.browseVarFilesAsDirectories = false;
                fileBrowserUI_p67.SetTextEntry(true);
                fileBrowserUI_p67.Show(HandleSavePreset_p67);
                fileBrowserUI_p67.GotoDirectory(folder_p67);
                fileBrowserUI_p67.fileEntryField.text = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds + ".json";
                fileBrowserUI_p67.ActivateFileNameField();
            });
            buttons_p67["load_preset"] = CreateButton("Load Preset", true);
            buttons_p67["load_preset"].button.onClick.AddListener(() =>
            {
                var folder_p67 = dirPresets_p67;
                folder_p67 = VamDiffusionHelpers.RemoveTrailingSlash_p67(folder_p67);
                var shortcuts_p67 = FileManagerSecure.GetShortCutsForDirectory(folder_p67);
                SuperController.singleton.GetMediaPathDialog(HandleLoadPreset_p67, "json", folder_p67, true, true, false, null, false, shortcuts_p67, true, true);
            });
            if (enableDefaultsLogic)
            {
                buttons_p67["save_default"] = CreateButton("Save Defaults", false);
                buttons_p67["save_default"].button.onClick.AddListener(CustomSaveDefault);
                buttons_p67["load_default"] = CreateButton("Load Defaults", true);
                buttons_p67["load_default"].button.onClick.AddListener(CustomLoadDefault);
            }
            buttons_p67["save_preset"].buttonColor = VamDiffusionHelpers.GetColorNewPurple3();
            buttons_p67["save_preset"].textColor = Color.white;
            buttons_p67["load_preset"].buttonColor = VamDiffusionHelpers.GetColorNewPurple3();
            buttons_p67["load_preset"].textColor = Color.white;
            if (enableDefaultsLogic)
            {
                buttons_p67["save_default"].buttonColor = VamDiffusionHelpers.GetColorNewPurple2();
                buttons_p67["save_default"].textColor = Color.white;
                buttons_p67["load_default"].buttonColor = VamDiffusionHelpers.GetColorNewPurple2();
                buttons_p67["load_default"].textColor = Color.white;
            }
            CreateSpacer_p67(25f, false);
            CreateSpacer_p67(25f, true);
        }
        bool CheckSelected_p67(string tabName_p67)
        {
            return tabName_p67 == tmpSelectedTab_p67;
        }
        public void HandleLoadPreset_p67(string path_p67)
        {
            if (path_p67 == null) return;
            if (!FileManagerSecure.FileExists(path_p67)) return;
            var jc = (JSONClass)LoadJSON(path_p67);
            base.RestoreFromJSON(jc);
        }
        public void HandleSavePreset_p67(string path_p67)
        {
            var json = path_p67.Replace(".json", "") + ".json";
            var jpg = path_p67.Replace(".json", "") + ".jpg";
            if (FileManagerSecure.FileExists(json))
            {
                FileManagerSecure.DeleteFile(json);
                if (FileManagerSecure.FileExists(jpg)) FileManagerSecure.DeleteFile(jpg);
            }
            var jc = GetJSON(true, true, true);
            SaveJSON(jc, json);
            SuperController.singleton.DoSaveScreenshot(json);
        }
        void SetSelectedTab_p67(string s)
        {
            tmpSelectedTab_p67 = s;
            try
            {
                InitPrepareYourUI_p67();
            }
            catch (Exception e)
            {
                SuperController.LogError("e InitPrepareYourUI " + e);
            }
        }
        public void Invoke_p67(Action f_p67, float delay_p67)
        {
            StartCoroutine(VamDiffusionHelpers.InvokeRoutine_p67(f_p67, delay_p67));
        }
        void OnEnable()
        {
            try
            {
                if (isPluginStarted_p67)
                {
                    OnMyPluginEnable_p67();
                }
            }
            catch (Exception e)
            {
                SuperController.LogError("error OnMyPluginEnable: " + e);
            }
        }
        void OnDisable()
        {
            try
            {
                OnMyPluginDisable_p67();
            }
            catch (Exception e)
            {
                SuperController.LogError("error OnMyPluginDisable: " + e);
            }
        }
        void OnDestroy()
        {
            if (diffusionPreview2_p67 != null)
            {
                UnityEngine.GameObject.Destroy(diffusionPreview2_p67.gameObject);
            }
            if (diffusionPreview_p67 != null)
            {
                UnityEngine.GameObject.Destroy(diffusionPreview_p67.gameObject);
            }
            CancelInvoke();
            SuperController.singleton.onSceneLoadedHandlers -= OnSceneLoaded;
            OnMyPluginDisable_p67();
        }
        void OnSceneLoaded()
        {
            issceneLoaded_p67 = true;
        }
        bool IsDisabled()
        {
            if (!enabledJSON.val)
            {
                return true;
            }
            if (VamDiffusionHelpers.IsLoadingIcon_p67()) return true;
            if (SuperController.singleton.isLoading) return true;
            return false;
        }
        UIDynamic CreateSpacer_p67(float height, bool rightSide)
        {
            UIDynamic spacer = CreateSpacer(rightSide);
            spacer.height = height;
            return spacer;
        }
        public void CreateTitle_p67(string id, string text, string subtitle, int fontsize, bool right, bool patreon = false)
        {
            titles_p67[id] = new JSONStorableString("title " + id, "\n<size=" + fontsize + "><b>" + text + "</b></size>\n<size=" + 20 + "><b>" + subtitle + "</b></size>");
            if (!titlesFields_p67.ContainsKey(id)) titlesFields_p67.Add(id, null);
            titlesFields_p67[id] = CreateTextField(titles_p67[id], right);
            titlesFields_p67[id].UItext.supportRichText = true;
            titlesFields_p67[id].backgroundColor = new Color(0f, 0f, 0f, 0f);
            if (right)
            {
                titlesFields_p67[id].textColor = new Color(0.42f, 0.03f, 0.51f, 1f);
            }
            else
            {
                titlesFields_p67[id].textColor = new Color(0.42f, 0.03f, 0.51f, 1f);
            }
            titlesFields_p67[id].height = 100f;
            titlesFields_p67[id].UItext.alignment = right ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
            if (patreon)
            {
                var rt = titlesFields_p67[id].GetComponentsInChildren<RectTransform>()[1];
                var pos = rt.anchoredPosition;
                pos.y += 70f;
                pos.x += 0f;
                rt.anchoredPosition = pos;
            }
        }
        void RemoveEditableTextField_p67(JSONStorableString js)
        {
            if (stringsEditable_p67.ContainsKey(js.name)) RemoveTextField(stringsEditable_p67[js.name]);
            RemoveTextField(js);
        }
        void AddContentSizeFitterToChildren(InputField inputField)
        {
            Transform textTransform = inputField.transform.Find("Text");
            Transform placeholderTransform = inputField.transform.Find("Placeholder");
            if (textTransform != null)
            {
                ContentSizeFitter textFitter = textTransform.gameObject.AddComponent<ContentSizeFitter>();
                textFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
            if (placeholderTransform != null)
            {
                ContentSizeFitter placeholderFitter = placeholderTransform.gameObject.AddComponent<ContentSizeFitter>();
                placeholderFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
        }
        void CreateEditableTextField_p67(JSONStorableString js, bool rightSide = false) => CreateEditableTextField_p67(js, rightSide, defaultTitleBackground, defaultTitleColor, 0, false, null);
        void CreateEditableTextField_p67(JSONStorableString js, bool rightSide, Color titleBackground_p67, Color titleColor, int customHeight = 0, bool isHidden = false, string customTitle =null)
        {
            try
            {
                bool isCustomHeight_p67 = customHeight > 0f;
                var currentHeight_p67 = !isCustomHeight_p67 ? 40 : customHeight;
                if (!stringsEditable_p67.ContainsKey(js.name)) stringsEditable_p67.Add(js.name, new JSONStorableString(js.name, ""));
                var uit2 = CreateTextField(stringsEditable_p67[js.name], rightSide);
                uit2.UItext.fontStyle = FontStyle.Bold;
                uit2.UItext.fontSize = 26;
                uit2.UItext.color = titleColor;
                uit2.height = uit2.GetComponent<LayoutElement>().preferredHeight = uit2.GetComponent<LayoutElement>().minHeight = 40;
                uit2.backgroundImage.color = titleBackground_p67;
                uit2.UItext.GetComponent<RectTransform>().localPosition = new Vector3(10f, -4f, 0f);
                var uit = CreateTextField(js, rightSide);
                var inputField = uit.UItext.gameObject.AddComponent<InputField>();
                stringsEditable_p67[js.name].val = customTitle != null ? customTitle : js.name;
                cacheInputFields[js.name] = inputField;
                cacheInputFieldsKeys = new List<string>(cacheInputFields.Keys);
                inputField.textComponent = uit.UItext;
                inputField.text = uit.text;
                if (isHidden)
                {
                    inputField.DeactivateInputField();
                    inputField.enabled = false;
                }
                if (currentHeight_p67 > 70)
                {
                    inputField.lineType = InputField.LineType.MultiLineNewline;
                }
                inputField.onEndEdit.AddListener((s) => { js.val = s; });
                ContentSizeFitter contentSizeFitter = inputField.gameObject.AddComponent<ContentSizeFitter>();
                contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                AddContentSizeFitterToChildren(inputField);
                inputField.onValueChanged.AddListener(delegate { LayoutRebuilder.ForceRebuildLayoutImmediate(inputField.GetComponent<RectTransform>()); });
                uit.height = uit.GetComponent<LayoutElement>().preferredHeight = uit.GetComponent<LayoutElement>().minHeight = currentHeight_p67;
                uit.backgroundImage.color = new Color(1f, 1f, 1f, 0.9f);
                uit.backgroundImage.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 20f);
                uit.backgroundImage.GetComponent<RectTransform>().localPosition = new Vector3(0f, 10f, 0f);
                uit.UItext.GetComponent<RectTransform>().localPosition = new Vector3(10f, -10f, 0f);
            }
            catch (Exception e)
            {
                SuperController.LogError("err:" + e);
            }
        }
        void SyncEditableFields(string cval)
        {
            if (stringsEditable_p67.Count() < 1) return;
            foreach (var key in cacheInputFieldsKeys)
            {
                if (cval == null) return;
                if (cacheInputFields[key] == null) continue;
                if (strings_p67[key] == null) continue;
                if (cacheInputFields[key].text == strings_p67[key].val) continue;
                if (cval != strings_p67[key].val) continue;
                cacheInputFields[key].text = strings_p67[key].val;
            }
        }
        public JSONStorableString.SetStringCallback EditableTextCallback(JSONStorableString.SetStringCallback userCallback = null)
        {
            return (string val) =>
            {
                SyncEditableFields(val);
                if (userCallback == null) return;
                userCallback?.Invoke(val);
            };
        }
        public string GetDiffApiUrl_p67()
        {
            return diffUrl_p67;
        }
        public void SetDiffApiUrl_p67()
        {
            if (Time.time - SetDiffApiUrlLast_p67 < 1f && diffUrl_p67 != null)
            {
                return;
            }
            SetDiffApiUrlLast_p67 = Time.time;
            var val1 = @"http:/"+"/127.0.0.1:7861";
            var call1 = new VamDiffusionApi(this, "GET", val1 + "/sdapi/v1/options", DiffUrlCallback1_p67);
        }
        public void CallbackInProgress_p67(JSONNode res)
        {
            if (!isGeneratingImages) return;
            if (res["current_image"].Value == null) return;
            if (res["current_image"].Value == "") return;
            if (res["current_image"].Value.Length < 30) return;
            var base64 = res["current_image"].Value;
            base64 = base64.Split(',')[1];
            var img = Convert.FromBase64String(base64);
            SetBase64Image(diffusionPreview2_p67, img);
        }
        public void DiffUrlCallback1_p67(JSONNode res)
        {
            var val1 = @"http:/"+"/127.0.0.1:7861";
            var val2 = @"http:/"+"/127.0.0.1:7860";
            if (res["x_error"] != null || res.Count <= 1)
            {
                var call2 = new VamDiffusionApi(this, "GET", val2 + "/sdapi/v1/options", DiffUrlCallback2_p67);
                return;
            }
            DiffUrlSave_p67(val1);
        }
        public void DiffUrlCallback2_p67(JSONNode res)
        {
            var val1 = @"http:/"+"/127.0.0.1:7861";
            var val2 = @"http:/"+"/127.0.0.1:7860";
            if (res["x_error"] != null || res.Count <= 1)
            {
                diffUrl_p67 = null;
                isA1111Connected = false;
                OnChangeApiStatus();
                return;
            }
            DiffUrlSave_p67(val2);
        }
        void DiffUrlSave_p67(string url)
        {
            diffUrl_p67 = url;
            isA1111Connected = true;
            OnChangeApiStatus();
        }
        void OnApiConnected()
        {
            ApiCheckModels();
            Invoke_p67(ApiCheckSamplers, 0.5f);
            Invoke_p67(ApiCheckOptions, 1.5f);
        }
        string GetApiPath(string s)
        {
            return diffUrl_p67 + "/" + s;
        }
        JSONNode cacheapiModels, cacheapiSamplers,cacheapiControlnet;
        void  CallbackCheckModels(JSONNode res)
        {
            if (res["data"] != null)
            {
                cacheapiModels = res["data"];
                List<string> newList = new List<string>() { "Select..." };
                foreach (JSONNode item in res["data"].AsArray)
                {
                   newList.Add(item["title"].Value);
                }
                listOptionsModels = newList;
                RefreshPromptTab();
            }
        }
        void CallbackGetOptions(JSONNode res)
        {
            if (res!=null && res["sd_model_checkpoint"] != null)
            {
                cacheDiffusionOptions_p67 = res;
                cacheDiffusionOptionsChanged_p67 = true;
            }
        }
        void CallbackCheckSamplers(JSONNode res)
        {
            if (res["data"] != null)
            {
                cacheapiSamplers = res["data"];
                List<string> newList = new List<string>() { "Select..." };
                foreach (JSONNode item in res["data"].AsArray)
                {
                    newList.Add(item["name"].Value);
                }
                listOptionsSamplers = newList;
                RefreshPromptTab();
            }
        }
        void CallbackCheckControlnet(JSONNode res)
        {
            if (res["model_list"] != null)
            {
                cacheapiControlnet= res["model_list"];
                List<string> newList = new List<string>() { "Select...", "None" };
                foreach (JSONNode item in res["model_list"].AsArray)
                {
                    newList.Add(item.Value);
                }
                listOptionsControlnet = newList;
                RefreshPromptTab();
            }
        }
        int tmpRefreshIte = 0;
        float lastRefresh = 0f;
        void RefreshPromptTab()
        {
            if(Time.time - lastRefresh < 0.25f)
            {
                Invoke_p67(RefreshPromptTab, 0.25f);
                return;
            }
            if (tmpSelectedTab_p67 != "Prompt") return;
            lastRefresh = Time.time;
            tmpRefreshIte++;
            RemovePopup(choosers_p67["Model"]);
            RemovePopup(choosers_p67["Sampler"]);
            Invoke_p67(() => { 
                choosers_p67["Model"] = new JSONStorableStringChooser("Model"+tmpRefreshIte, listOptionsModels, "Select...", "Model", CallbackSetChooserModel);
                choosers_p67["Sampler"] = new JSONStorableStringChooser("Sampling method" + tmpRefreshIte, listOptionsSamplers, "Select...", "Sampling method", CallbackSetChooserSampler);
                tmpPopupModel = CreateFilterablePopup(choosers_p67["Model"], true);
                tmpPopupSampler = CreateFilterablePopup(choosers_p67["Sampler"], true);
            }, 0.2f);
        }
        void ApiCheckModels()
        {
            var url = GetApiPath("sdapi/v1/sd-models");
            var call1 = new VamDiffusionApi(this, "GET", url, CallbackCheckModels);
        }
        void ApiCheckSamplers()
        {
            var url = GetApiPath("sdapi/v1/samplers");
            var call1 = new VamDiffusionApi(this, "GET", url, CallbackCheckSamplers);
        }
        void ApiCheckOptions()
        {
            var url = GetApiPath("sdapi/v1/options");
            var call1 = new VamDiffusionApi(this, "GET", url, CallbackGetOptions);
        }
        void ApiCheckControlnet()
        {
            var url = GetApiPath("controlnet/model_list?update=true");
            var call1 = new VamDiffusionApi(this, "GET", url, CallbackCheckControlnet);
        }
        void OnApiDisconnected()
        {
        }
        public void SetBase64Image(Image imageComponent, byte[] imageBytes)
        {
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(imageBytes);
            imageComponent.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            AspectRatioFitter aspectFitter = imageComponent.GetComponent<AspectRatioFitter>();
            if (aspectFitter == null)
            {
                aspectFitter = imageComponent.gameObject.AddComponent<AspectRatioFitter>();
            }
            aspectFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            aspectFitter.aspectRatio = (float)texture.width / texture.height;
        }
        void OnChangeApiStatus()
        {
            var currentStatus = isA1111Connected;
            var text = "AUTOMATIC1111 status:\n <b><color=#66ff66>CONNECTED</color></b>";
            if (!currentStatus || diffUrl_p67 == "")
            {
                text = "AUTOMATIC1111 status:\n <b><color=#993333>DISCONNECTED</color></b>";
                OnApiDisconnected();
            } else
            {
                OnApiConnected();
            }
            if (titlesFields_p67.ContainsKey("titlepluginname2"))
            {
                titlesFields_p67["titlepluginname2"].text = text;
            }
        }
    }
}
namespace SPQRVamDiffusion
{
    public class VamDiffusionApi
    {
        VamDiffusion X;
        string type_p67;
        string token_p67;
        string url_p67;
        string payload_p67;
        UnityAction<JSONClass> callback_p67;
        UnityAction<object, UploadDataCompletedEventArgs> callbackPostDirect_p67;
        WebClient wc;
        string debugKeys = "";
        public VamDiffusionApi(VamDiffusion script)
        {
            X = script;
        }
        public VamDiffusionApi(VamDiffusion script, string type, string url, UnityAction<JSONClass> callback, string payload = null, Dictionary<string, string> headers = null, string token = "", UnityAction<object, UploadDataCompletedEventArgs> callbackPost = null)
        {
            X = script;
            wc = new WebClient();
            type_p67 = type;
            callback_p67 = callback;
            callbackPostDirect_p67 = callbackPost;
            url_p67 = url;
            payload_p67 = payload;
            token_p67 = token;
            if (headers != null)
            {
                List<string> keys = new List<string>(headers.Keys);
                foreach (var key in keys)
                {
                    wc.Headers.Add(key, headers[key]);
                    debugKeys = key + ":" + headers[key] + ", ";
                }
            }
            if (type == "POST") Post_p67();
            if (type == "GET") Get_p67();
        }
        public static void DebugResponse_p67(JSONNode res)
        {
            SuperController.LogMessage("received: " + res.ToString());
            if (res["data"] != null)
            {
                SuperController.LogMessage("sent: " + res["data"].ToString());
            }
        }
        void Get_p67()
        {
            wc.DownloadStringCompleted += GetCallback_p67;
            if (X == null) return;
            X.StartCoroutine(CoGetRequest_p67());
        }
        void Post_p67()
        {
            wc.UploadDataCompleted += PostCallback_p67;
            if (X == null) return;
            X.StartCoroutine(CoPostRequest_p67());
        }
        void GetCallback_p67(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                JSONClass json = new JSONClass();
                if (!e.Cancelled && e.Error == null)
                {
                    string jsonResponse = e.Result;
                    JSONNode jsonnode = JSON.Parse(jsonResponse);
                    JSONClass jsonclass = JSON.Parse(jsonResponse) as JSONClass;
                    if (jsonnode != null && jsonclass == null)
                    {
                        json = new JSONClass();
                        json.Add("data", jsonnode);
                    }
                    else
                    {
                        json = jsonclass;
                    }
                    if (json != null)
                    {
                        json.Add("x_ok", "true");
                        json.Add("x_token", token_p67);
                    }
                    else
                    {
                        json = new JSONClass();
                        json.Add("x_error", "bad json " + jsonResponse);
                        json.Add("x_response", jsonResponse);
                        json.Add("x_token", token_p67);
                    }
                }
                else
                {
                    var error = e.Error.ToString();
                    if (error.Contains("Error: ConnectFailure")) error = "connection failed";
                    json.Add("x_error", error);
                    json.Add("x_token", token_p67);
                }
                callback_p67(json);
            }
            catch (Exception err)
            {
                SuperController.LogMessage("error on get: " + err);
            }
        }
        void PostCallback_p67(object sender, UploadDataCompletedEventArgs e)
        {
            if (callbackPostDirect_p67 != null)
            {
                callbackPostDirect_p67(sender, e);
                return;
            }
            JSONClass json = new JSONClass();
            if (e.Error != null)
            {
                if (e.Error.ToString() != "")
                {
                    json.Add("x_error", e.Error.ToString());
                    json.Add("x_token", token_p67);
                    callback_p67(json);
                    return;
                }
            }
            string jsonResponse = Encoding.UTF8.GetString(e.Result);
            json = JSON.Parse(jsonResponse) as JSONClass;
            if (json == null)
            {
                json.Add("x_error", "bad json");
                json.Add("x_response", jsonResponse);
                json.Add("x_token", token_p67);
                callback_p67(json);
                return;
            }
            else
            {
                json.Add("x_ok", "true");
                json.Add("x_token", token_p67);
                callback_p67(json);
            }
        }
        IEnumerator CoGetRequest_p67()
        {
            if (wc == null) yield break;
            if (url_p67 == null) yield break;
            if (token_p67 == null) yield break;
            try
            {
                wc.DownloadStringAsync(new System.Uri(url_p67), token_p67);
            } catch(Exception e)
            {
                SuperController.LogMessage("err : " + e);
            }
            yield break;
        }
        IEnumerator CoPostRequest_p67()
        {
            if (wc == null) yield break;
            if (url_p67 == null) yield break;
            if (token_p67 == null) yield break;
            if (payload_p67 == null) yield break;
            byte[] bodyRaw = Encoding.UTF8.GetBytes(payload_p67);
            wc.UploadDataAsync(new System.Uri(url_p67), "POST", bodyRaw, token_p67);
            yield break;
        }
    }
}
﻿
namespace SPQRVamDiffusion
{
    public class VamDiffusionHelpers
    {
        public static int ToInt_p67(string s)
        {
            int v = 0;
            if (s == "" || s == null) s = "0";
            try
            {
                v = Int32.Parse(s);
            }
            catch (Exception e)
            {
                if (e.ToString().Length > 0) { }
            }
            return v;
        }
        public static Color GetColorMainPurple_p67(float opacity = 0.8f)
        {
            return new Color(156f / 255f, 35f / 255f, 187f / 255f, opacity);
        }
        public static Color GetColorDeepPurple_p67()
        {
            return new Color(0.427f, 0.035f, 0.517f);
        }
        public static Color GetColorMidPurple_p67()
        {
            return new Color(0.576f, 0.424f, 0.643f);
        }
        public static Color GetColorLightPurple_p67()
        {
            return new Color(0.631f, 0.549f, 0.667f);
        }
        public static Color GetColorDark_p67()
        {
            return new Color(0.3f, 0.3f, 0.3f, 0.8f);
        }
        public static Color GetColorNewPurple()
        {
            return new Color(131f / 255f, 94f / 255f, 147f / 255f);
        }
        public static Color GetColorNewPurple2()
        {
            return new Color(48f / 100f, 30f / 100f, 57f / 100f);
        }
        public static Color GetColorNewPurple3()
        {
            return new Color(37f / 100f, 20f / 100f, 44f / 100f);
        }
        public static Color GetColorLight_p67()
        {
            return new Color(1f, 1f, 1f);
        }
        public static string RemoveTrailingSlash_p67(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }
            if (input[input.Length - 1] == '/')
            {
                return input.Substring(0, input.Length - 1);
            }
            return input;
        }
        public static bool IsLoadingIcon_p67()
        {
            return SuperController.singleton.loadingIcon.gameObject.activeInHierarchy;
        }
        public static IEnumerator InvokeRoutine_p67(System.Action f, float delay)
        {
            yield return new WaitForSeconds(delay);
            try
            {
                f();
            }
            catch (Exception e)
            {
                SuperController.LogError("error InvokeRoutine_p67: " + e);
            }
        }
        public static void SetAtomStorableColor_p67(Atom q, string storable_id, string param_id, HSVColor newValue)
        {
            if (q == null) return;
            JSONStorable t = q.GetStorableByID(storable_id);
            if (t == null)
            {
                return;
            }
            JSONStorableColor tval = t.GetColorJSONParam(param_id);
            tval.val = newValue;
        }
        public static void SetAtomStorableColor_p67(string atom_id, string storable_id, string param_id, HSVColor newValue)
        {
            var q = GetAtomById_p67(atom_id);
            if (q == null) return;
            JSONStorable t = q.GetStorableByID(storable_id);
            if (t == null)
            {
                return;
            }
            JSONStorableColor tval = t.GetColorJSONParam(param_id);
            tval.val = newValue;
        }
        public static JSONStorableColor GetAtomStorableColor_p67(string atom_id, string storable_id, string param_id)
        {
            var q = GetAtomById_p67(atom_id);
            JSONStorable t = q.GetStorableByID(storable_id);
            if (t == null) return null;
            JSONStorableColor tval = t.GetColorJSONParam(param_id);
            return tval;
        }
        public static JSONStorableColor GetAtomStorableColor_p67(Atom q, string storable_id, string param_id)
        {
            JSONStorable t = q.GetStorableByID(storable_id);
            if (t == null) return null;
            JSONStorableColor tval = t.GetColorJSONParam(param_id);
            return tval;
        }
        public static void SetAtomStorableBool_p67(Atom q, string storable_id, string param_id, bool newValue)
        {
            if (q == null) return;
            JSONStorable t = q.GetStorableByID(storable_id);
            if (t == null) return;
            JSONStorableBool tval = t.GetBoolJSONParam(param_id);
            tval.val = newValue;
        }
        public static void SetAtomStorableBool_p67(string atom_id, string storable_id, string param_id, bool newValue)
        {
            var q = GetAtomById_p67(atom_id);
            if (q == null) return;
            JSONStorable t = q.GetStorableByID(storable_id);
            if (t == null) return;
            JSONStorableBool tval = t.GetBoolJSONParam(param_id);
            tval.val = newValue;
        }
        public static void SetAtomStorableFloat_p67(Atom q, string storable_id, string param_id, float newValue)
        {
            if (q == null) return;
            JSONStorable t = q.GetStorableByID(storable_id);
            if (t == null) return;
            JSONStorableFloat tval = t.GetFloatJSONParam(param_id);
            if (tval == null) return;
            tval.val = newValue;
        }
        public static void SetAtomStorableFloatDamped_p67(Atom q, string storable_id, string param_id, float newValue, float damp = 0.1f)
        {
            if (q == null) return;
            JSONStorable t = q.GetStorableByID(storable_id);
            if (t == null) return;
            JSONStorableFloat tval = t.GetFloatJSONParam(param_id);
            if (tval == null) return;
            if (newValue > tval.val)
            {
                tval.val += Mathf.Min(newValue - tval.val, damp);
            }
            else
            {
                tval.val -= Mathf.Min(tval.val - newValue, damp);
            }
        }
        public static Atom GetAtomById_p67(string id)
        {
            var a = SuperController.singleton.GetAtomByUid(id);
            return a;
        }
        public static void SetAtomStorableFloat_p67(string atom_id, string storable_id, string param_id, float newValue)
        {
            var q = GetAtomById_p67(atom_id);
            if (q == null) return;
            JSONStorable t = q.GetStorableByID(storable_id);
            if (t == null) return;
            JSONStorableFloat tval = t.GetFloatJSONParam(param_id);
            if (tval == null) return;
            tval.val = newValue;
        }
        public static JSONStorableBool GetAtomStorableBool_p67(Atom q, string storable_id, string param_id)
        {
            JSONStorable t = q.GetStorableByID(storable_id);
            JSONStorableBool tval = t.GetBoolJSONParam(param_id);
            return tval;
        }
        public static JSONStorableBool GetAtomStorableBool_p67(string atom_id, string storable_id, string param_id)
        {
            var q = GetAtomById_p67(atom_id);
            JSONStorable t = q.GetStorableByID(storable_id);
            JSONStorableBool tval = t.GetBoolJSONParam(param_id);
            return tval;
        }
        public static JSONStorableFloat GetAtomStorableFloat_p67(Atom q, string storable_id, string param_id)
        {
            JSONStorable t = q.GetStorableByID(storable_id);
            if (t == null) return null;
            JSONStorableFloat tval = t.GetFloatJSONParam(param_id);
            return tval;
        }
        public static JSONStorableFloat GetAtomStorableFloat_p67(string atom_id, string storable_id, string param_id)
        {
            var q = GetAtomById_p67(atom_id);
            JSONStorable t = q.GetStorableByID(storable_id);
            JSONStorableFloat tval = t.GetFloatJSONParam(param_id);
            return tval;
        }
        public static void SetAtomStorableString_p67(Atom q, string storable_id, string param_id, string newValue)
        {
            if (q == null) return;
            JSONStorable t = q.GetStorableByID(storable_id);
            if (t == null) return;
            JSONStorableString tval = t.GetStringJSONParam(param_id);
            tval.val = newValue;
        }
        public static void SetAtomStorableString_p67(string atom_id, string storable_id, string param_id, string newValue)
        {
            var q = GetAtomById_p67(atom_id);
            if (q == null) return;
            JSONStorable t = q.GetStorableByID(storable_id);
            if (t == null) return;
            JSONStorableString tval = t.GetStringJSONParam(param_id);
            tval.val = newValue;
        }
        public static JSONStorableString GetAtomStorableString_p67(string atom_id, string storable_id, string param_id)
        {
            var q = GetAtomById_p67(atom_id);
            JSONStorable t = q.GetStorableByID(storable_id);
            JSONStorableString tval = t.GetStringJSONParam(param_id);
            return tval;
        }
        public static void SetAtomStorableStringChooser_p67(Atom q, string storable_id, string param_id, string newValue)
        {
            if (q == null) return;
            JSONStorable t = q.GetStorableByID(storable_id);
            if (t == null) return;
            JSONStorableStringChooser tval = t.GetStringChooserJSONParam(param_id);
            if (tval == null) return;
            tval.val = newValue;
        }
        public static void SetAtomStorableStringChooser_p67(string atom_id, string storable_id, string param_id, string newValue)
        {
            var q = GetAtomById_p67(atom_id);
            if (q == null) return;
            JSONStorable t = q.GetStorableByID(storable_id);
            if (t == null) return;
            JSONStorableStringChooser tval = t.GetStringChooserJSONParam(param_id);
            tval.val = newValue;
        }
        public static JSONStorableStringChooser GetAtomStorableStringChooser_p67(Atom q, string storable_id, string param_id)
        {
            JSONStorable t = q.GetStorableByID(storable_id);
            JSONStorableStringChooser tval = t.GetStringChooserJSONParam(param_id);
            return tval;
        }
        public static JSONStorableStringChooser GetAtomStorableStringChooser_p67(string atom_id, string storable_id, string param_id)
        {
            var q = GetAtomById_p67(atom_id);
            JSONStorable t = q.GetStorableByID(storable_id);
            JSONStorableStringChooser tval = t.GetStringChooserJSONParam(param_id);
            return tval;
        }
        public static void GetAtomStorableUrl_p67(Atom q, string storable_id, string param_id, string newValue)
        {
            if (q == null) return;
            JSONStorable t = q.GetStorableByID(storable_id);
            if (t == null) return;
            JSONStorableUrl tval = t.GetUrlJSONParam(param_id);
            if (tval == null) return;
            tval.val = newValue;
        }
        public static void SetAtomStorableUrl_p67(string atom_id, string storable_id, string param_id, string newValue)
        {
            var q = GetAtomById_p67(atom_id);
            if (q == null) return;
            JSONStorable t = q.GetStorableByID(storable_id);
            if (t == null) return;
            JSONStorableUrl tval = t.GetUrlJSONParam(param_id);
            tval.val = newValue;
        }
        public static JSONStorableUrl GetAtomStorableUrl_p67(string atom_id, string storable_id, string param_id)
        {
            var q = GetAtomById_p67(atom_id);
            if (q == null) return null;
            JSONStorable t = q.GetStorableByID(storable_id);
            if (t == null) return null;
            JSONStorableUrl tval = t.GetUrlJSONParam(param_id);
            return tval;
        }
        public static JSONStorableUrl GetAtomStorableUrl_p67(Atom q, string storable_id, string param_id)
        {
            if (q == null) return null;
            JSONStorable t = q.GetStorableByID(storable_id);
            if (t == null) return null;
            JSONStorableUrl tval = t.GetUrlJSONParam(param_id);
            return tval;
        }
        public static byte[] GetVamDiffusionLogo()
        {
            var base64 = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAfQAAAH0CAYAAADL1t+KAAAgAElEQVR42uzdeXhU15nv+++qKs0gJCQGAQYDcoGYMQhi5sFgPOB4xMZ4aDvd8UnaOTm3u09O0qcH9/WToXP7OEl32t1JTjoxNtjBSew4sZ0Y2wjwwIyFBJhCgJHFJBCS0Kwa1v2jhDxJJdWgUgn9Ps/jJ0G1p9p71X73evcaQERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERERGLA6BT0HW632wBOYASQBwwF0oGkfnQaWoFGoBI4DZwBAh6Px6qEiIgCuvSFYO4CBgBDgFnANKAAyAZS+9GpaAQuAh8A7wP7gCqgwePx+FRSRKS/cukU9Ilg7mgL5IuAu4HxQG5b7dzVzx7MAoAPmAvcCBwFNgC7gbMqLSKiGrokajBPBgYDtwIrgQVAFpCis0NzW229CHgN+CNwyePxtOrUiIhq6JJoBhFMrd8LfAFI0ylpl0qwLcHtQDJwEjjYFuRjbumzdn7A8JtotmHhr7etMxsS5QQuetbeYww/ivI7rdm2zmyL63Gvt2ONg3cxXVdKjOV7RfebH/aFAr14o/0HLF+N8WYrt64z03r1e22wTwL3xXizR7euMwt1G1RA70sKgAcJptlVK+/gft12XmYAjwA/6KmAbkeyk9O4gJwoDvYegq8IEuPkGe4FhkWxieqGet6L+3E7eRgY3q3rZvhvwA/7SGHOtNFdj44MW/SsnbrtflPSi1/tTmL/vWp0+1NA7xPaWrQPAK4B5hN8Z+6IZpszZszg61//OhMnTiQzM5NAIEB1dTWHDx9m/fr1vPPOOwBs3LiRWbNmffqmaC11dXW8//77/MM//ANnzpxp/ywjI4N/+qd/4gtf+ALZ2dl4vV4qKir49a9/zS9/+ctPbaejbQO0tLRQVlbGt7/9bfbu3RvuV3O01dTnAuPdbne5x+OJ+Y+9aKnxLd5gXwT+PIrNrFiyyQ4oWmPqe7uMzfqJTQduiHIzL+191HjjeuCPWwfwUBhrTFj6rJ2/5X7zTr996nWwCuiVgL7wWTsJGK27es9z6BQkLCfBhnBXtwX19Gg2VlhYyM9+9jPmzZvH4MGDcblcJCcnM2zYMJYsWcL3vve9LmpyhszMTBYtWsQvf/lLXK7gs2BqaiqbNm1i9erVDBkyBJfLRVpaGtdccw3f/OY3+da3vtWt40tJSWHy5Mn88Ic/JDMzM5KvmAGMastkDOupi2ItL0S5iVTr5cZEKGAZA1hFtK9woj8fYVuUz/XhBoiAg0f69d3E9l6ZMyRGeVdAl94O6FfRzbRiV7785S+TmZmJtZbt27fzH//xH/ziF7/gzTff5MKFC/z2t7/tcL0nn3ySJ598kn/7t3/jyJEjAFx99dU8+uijAHzrW98iPz8fgNraWt555x2Ki4ux1mKMYe3atVx99dUht/2DH/yA999/H4ChQ4fyyCOPRHPOrgZG9tgPZiRvEewmF407EqLWZrk9yk1U1zfwRtxvWiaC4GxZs3K9zejH95P5SzbZAb1SzgyrdDuPD6XcEzug5wCZsdjY6NHBCo3H4+HP/7x7GePz58/zk5/8pP3fb731Fi+++CIAkydPbq/5AzQ2NvLYY4+xa9cuAL797W9z1113kZKSwj333MM///M/h9z2H/7wB9544w2MMe0PCBE+oGa21dZ7RIzS7jdN3mSTD64xvdYaf9ZPbBKGW6LcTNzT7Qs22GwLt0Ww6oAWF2uAX/TT+0lyoIVlwMvx3OnK9TajBdRwTTX0fu/yO/SYtGr3+/0ADB8+nOXLl0e0jUAg8Lm/ZWVlAXDy5Mn2YA58Kljn5eV1XRAdMSuKrp5+UI1B2j0zx8v1vVm4MjJYSrD7YxQnIv7pdoflPiJvHPpwP7/bxz313epgKWrMqxq6fCqwR+3o0aOMHz+eQYMG8eMf/5gzZ85QXl5OaWkpGzZs+FQjt8uGDBnSnlpPSkpi5cqV7Z8dOnQomEZwOj/1wHBZTc3HbdKSkpJCbtsYw9KlSzEm+FWPHTsW7fnq0fEVHCN5y56miihauzuCafdXe/FJ/o4ox8rtlXS7MVG8C7csXPKMzS96wJT10xvJDb2wU6XbFdAT2yfGVE9re/pM6oFsRzoxHNb1e9/7HuPGjcPtduNwOBg5ciQjR47kuuuuY+3atTz11FP8/Oc//9x6f/VXf/W5v508eZL/+3//b9TH1NG2KysrOzyOMIJ5GjDY7XZH8x69zuPxXOrsw6Klxrdkg33Jwpcijy3cyuPWweMmEPcC/Lh1WPhilFv5XbzT7Qs32mlYro0qqeDkEeBv++mta+zSZ+yELQ+YI/HaoUUBXQE9sYK3g4/TuJ/8L43gCG5ZBFPjzhjXDFOAcUSbFm1z5swZVq9ezbp165g7dy6jRo1ixIgRZGdnM2DAAB577DFeffXVDmvqEOy2Vl9fT3FxMf/0T/9Ec3Nzp/uaOXNmRLXsgwcP8sQTT3Dp0qUoKp7kAPnAnChOVwPwehdPDi9EE9CBIUvdLNwCW+Ndppfmc13ARNfY0tIr6fYvxWAzD969yf79C2uMvz/ez6zhRiAuAX3RBnsNwV4nooCeMMF8YFuAcLcVzqsIdovKbquZJ/VAML8cnLJiFdAv27BhAxs2fDyuyXe/+13uuOMO0tPTueGGGz7Vb/z8+fMsWLAg5PYup9ovp97z8/N56qmnqK2tbV/G6/18Re7ytp988kluvvlmIJjG379/fzRfzwlMJtjK/YYor/1pj8dT2umNcQRvEmXa3QZbmcc9oAdM1K3sqxvq2RzPY568ySbjZV0Mqowjz3m5gV583dGbAsEUeFwG2VF3NQX0RAjiToItpa8CxhDsBjWeYL/XUW3BfHBbrbzP+Pd//3dOnjzJ97///U/9ff/+/dxxR/D+npycHPZ2a2pqyMnJYcyYMcyZM4fZs2czaNAgBg8e/KnsQGeeeOIJrrvuOgYPHswtt9zC888/T2lpaTQPQdlt/0XrS8D/09mHMUq73w78j14oDtF2V4t7uj3XyxejeXj6jEf6a0A3sPi6TTbtvTWmKQ67U7pdAb1Xa+OutmA+HrgeWAbMbPubs69+t2nTprFo0SKSk5NZvHgxe/bs4fz58+Tl5bFs2bL2mvaOHTvC3vbu3bsZP3486enp/PjHP6a0tJSqqiqGDh3avszw4Z1nd6urq3n++ef56le/SlpaGt/85je5//77E+G03et2u//G4/H4Q9wco027j166wc7ass7sjdeXWrLBzrAwNrpKbvzT7VgeiVUOzMDqJRttbtF95kI/vNWlJreyhOBkRj1Xzn5hUy0sUWSJL3Vb+9gggpOf/HfgCeABYHpbTbxPn6dhw4a1v5fOz8/n3nvv5Wtf+xp33XVXe0168+bNHDhwIOxtf/e736WsLNhoeNCgQcyfP/9TwRzgpptu4jvf+U6n2/jRj37U/s599uzZ3HvvvYlw2oYDS0PGmBG8SZTjxgfiPMiMjX4wmZqqpPi2bl/2tB2JYWUMN5lMgPvpr+LQ8twksxhNJKWAHudauXG73Ulut3sSwfc99wC3EBw73U0wxZdEH59mdvPmzdxxxx386le/4sMPP6S+vp5AIEBjYyPHjh3jP//zP/n6178e0babm5tZs2YNr776KhcuXMDn89Hc3MyxY8f4/ve/z8svv8yZM2d46qmnQm7nqaeeIhAIYIzh0UcfbR9atpetDfVh0VLjw/BSlPu4I86/+Kj2Zwy/i/eAOH4XD8X6XmVNv+6T3uMB3a/uar30rNaPgznBKTezgHVtN9bZaBCEmEtNTQ3ZKj6BXQKGeTyeTg9+6QZ7QyA4D3s0QbKg6D7zQU9/mSXP2Hzr4GhUgRBu3rbOxPX98+IN1kNwPoNY12Zmx/N1R7eu0Ub7L9by1z29H+tn3LYHzYkevGYfABN6+Gsc2brOTNQdVjV0CPbvngp8C1hDcJrSJBWJ2OujwRyCbSdCttSNRdrd2vjU0m30rdvjnm5ftMEu6olgDhCw/XfCFuPquRboS56zV8chmIsCerDxm9vtTgauBVa33bAnE2y5rjYF8lnxSLvfHpdvYqLbT2+k2x2mB4OuYe2SX9jUflmqbQ+mxP1Ktyugx09SW/C+jeCcyuPpY13QJK5Wu93uQaHjQtStvmcves5e1ZNfYv5zdgTB+eKjCehxbd0+/+d2oLXc1YO7yLYpcXqYSjzLJm+yyT2xYetQ/3MF9PgZC3wNWAwMVa1cupBKF7N71dVFn3Yn0LNpd5flNqJrM1NT6YzvYDJJadxDD86c11ZT7a9p94yhrbGfBW3yJpuMZZluGwroPc7tdo9uq6XcSPC9XBr9uGGgdFvItPveR4030dPu0b6n7410u41PsF224Gk7uj8W6p5oiZ7jYwHKeCqgxyGYG4Kt2G8AJhHjIVXlina92+0eFjLgRZl2N7Bg4SY7pCcOfsEGm22CGaloAnpc0+1LNtqJwHXxuAc6XP2zC1tPDM1qAnp/roDe88E8FcgDFhLsY67W7BIOJ8GeEJ1qS7tXR7MP4+XWHvmRW24lulEha+OdbrdxnLvcwENY2x8zdZOXPGtHxfhkKqAroPe4wcA8giO/XYXem0v4uk67E13a3fTUIDMm+rHb45luv3uTdWJ5II7Xduyi50KPCngFR4CYBeBlT9uRBLsCiwJ6jxrZVsMaj96ZS2Suc7vd+SHjpmFTlPtYPv/ndmAsD3rlepthiG7YVIcj6u8VlkofNxHMqMXvRthPG8cFYth9zedS7VwBvYe53e4sgi3bZwG5uuQShXtCfRiDtHtKUio3x/KAvcGbbDRjasc93d4bLc8t3HH9JjuovxVoA9cv2WJjNc5yJO/k64AzurUooHfXGIKjwI0A0nXJJQoh5+OORdrdxjjtHoh+FLq4ptvnrbdDIbYPNd2U5msN/VqlD0TnUxGsNchWRN/48O5N1mlgeQQF/jWgFVFA76YpBKdAdepyS5QK3G739C5uUNG2Br/xxldtTOYTmPUTmxRtcDQ2vq3bXQ4eIPJGqxbwRPwwZfp22t1aXowwCkSdKj/v4zoi6DlkDS/rtqKAHo6xBGdOU0O4KDmdTpKTk8nIyGDAgAGkpaWRnJyMMf2qWULIWlx9A28QXdp9QGNNbKYKHTiQ5QSnBY5U7flkXo9rJTOaWdAMb1vLv0ax+8KlG+3kPnwz/13bQ03YD5ExeJiIZBu+ALyKxO6B+Er9Ym6320VwlK9RBNPtCuhdBGuXy0VqaipJSUmkpqbicrlISUnB6XTidDpJSkpq/5sxBr/fj9frxefz4fV62//t9XppbW1t/6+lpQWfz4ff778iArrb7f6Wx+Pp8Ma591HjXbzBvkQ03a6Cc5b/PgY32T7Vun3pc3ZuIMDkKM7bc74Av0ly8sNI720By5eAv+qTJdPPWZzsIPz++zOWb7TD3rzPnIti75HU8re9vc5UL95gEQX0rqQAwwkO7zpIlzq05ORksrKyGDFiBMOHDycvL4+xY8dy9dVX43a7yczM7HIbly5d4vDhwxw/fpyKigpOnTrFuXPnOHPmDBcvXrxSAvpogmMZvB0isLxAdPNt33r3Jut8YY2J/IQ9bh3AF6OqLcc53W79PBxFHxSfMbzw7oPmwqIN9k0THEAqEvfP+on9X23tIfoey0uYsAO68VpuANZHssu2dg8zI3lg1J1XAb270gn2Oe+RYQjT0tIYOXIkI0aMIC8vj6ysLFwuFz6fj7q6Os6cOUNFRQWnT5+msbERaxPnKfRy6nzo0KHk5eUxYsQIhg4dypAhQxg0aBCDBw8mOzub3Nxchg4d2u3tZmZmUlhYyOjRo6mqqqK6upqamhpqa2uprKzk7NmznDt3jlOnTnHx4sWEOy/h1NJDBfT6Bt4YMIBqIDvC7eec87EYeCvSA1ziZp61DIviO8Y13X7dJptmvdwbxSZeL7rPXGj7/xuJPKAPGZDBLRDh++jejueGFw38c9gPb8G0e0QB3eVkFRF0BzYOvT9XQA8j5rYF9JhO7uB0OsnOzmb06NFMmjSJqVOnMnv2bEaN+njApcrKSnbu3MnBgwcpLS2loqKC8+fP4/P5ej2IDxgwgKysLHJychgzZgxut5s5c+bgdrtjsh+Hw0FeXh55eZ/uRhwIBDh48CDFxcV4PB5OnjzZHvTr6upobW3tS8F9jdvt/h8ej6fDWlxb2v13wJ9FUdO6PZqAHoM51l+OZ7o9xcedNopMmg0GcQD8zbzoSuUnBF+5RZCa4JG+GtC3rTNHF2+wByHsVxcreNw6eNwEIsjkrIogs3KgaK35UCFYAb3XAroxhuTkZK699lruv/9+5s7teDbKoUOHsnr1alavXs2hQ4f45S9/ybZt27h06VKvpZ2Tk5MZNmwYM2fOZP78+axevTqu+3c4HEydOpWpU4MDSTU3N7N582aKioooLi7m3LlztLb2md4rucAKQjTosfCCiSKgG7gNa/87xkT6lBPd+/M4j90e5UQsjY6kj9O373zJ1C3eaP9A5FOvrlqyyQ4vWmPO9sUbn7G8ZE3YAT1n6QQKt8DOsNZ63DowETXiVLpdAT28GELwPXrMuqtlZmYyb948Fi9eTGFhYbfWmTRpEsuWLcPhcLB9+3aqqqriVhN1uVykp6eTn59Pfn4+EydOZPz48UycOLHXL05qaioLFixg8ODBTJkyhbKyMjweDx999FGvPviEYW2ogN5Qz+Yo0+6jFm6kcDvsCnfFRc/ZmQS4OorvVnvBxZ/idSIXrbdjgSVRPPz8vmiNqf90SoiNmIgDuivg5SEiSF0nREA3vGjhf4f9UBXgRsIM6EsnUBgIkBPBzVkBXQE97ICeTIxat6ekpDB8+HCWL1/OihUrcDi6v9lVq1aRlJREWVkZzc3N1NfX9/AP2pCZmUlOTg6jRo2isLCQBQsWMGnSpIS6QNnZ2cyfP5/58+dz+PBhtmzZwv79+9vT8Q0NDYmchr/d7XZneDyeho4+jEXa3RkcZCbsgB6DudXjmm43Th4miiGZA4GP0+2XpQ/m1cZqaohwVkUT7KXQJwP6lnVm7+IN9iOCGcrun8fgMLCPh7OOP8CqCC5cxZZ1Zq/Cb88EvSuVaaudx6ST9NChQ5k4cSJz584lNTX8V3MzZsxgypQp5Ob27OizxhiSkpKYMGECt912Gz/72c/48pe/nHDB/LMKCgr46le/yre//W1uueUWxo0bR1JSQk+KlwGEfG9ho5xS1UaYNjd9Kd0ebI3/UBRbqK5K4Y+f/eNrN5kWDL+NYrsTlj5r5/fZu58Nf8RCYyhc/rTNCXM/N0ZwbGoMpxp6RAE9ZiOeZGdn43a7w2r1/Uk5OTlMnz6dsrIyPvzwwx75wpdb3s+YMYOpU6cyZ86cqLdZWVlJRUUFdXV11NbWUldXR3NzM9ba9oZ2mZmZZGVlMWDAAMaPH9+tLm6hHpxWrFjB4MGDGTt2LPv27aOyspKWlpZELGNrgec7+7AqiTdyvZHXEgH30o128pb7zMHurrBog70GiGZwlEvpWfFr3b4on+sJdgWM1G86yyYYy3OWyN/NBxw8ArzTR+9+LwJfC7eC50tiJfBcdxZe/rTN8RkKwz00h0aHU0Dv7aB++V1vNGbNmsVbb70V+4vY9q581KhRzJgxg3Xr1pGfnx/2di5cuEB5eXl7d7NLly5x4cIFLl26RHNzMw0NDTQ2Nra3SL88GE16ejoZGRmkpqa2d3cbOHAggwYNak/75+R0/8G/oKCAgoIC9u/fj9Pp5NChQ1RUVNDU1JRo79ZvdLvdOR6Pp6qjDw+uMa1LNtrfWRt5DbRtLPaDYawSXbrd8vJrN5m4PT0ZE/W85xs7+6DoKG8tzucshuERnos1K9fb//76g6ahr934hiaxrdLLRYJTR4fznVd1N6D7kliJDTvLW1eZxBaFXgX0XjVo0KCoa7yjR48mIyMj5seWkZHB+PHjufXWW7nzzjtJTk4OextlZWXs3LmT4uJiPvroI86ePUtVVdXnAmhH77Q/O/RrSkoKgwYNIi8vjzFjxjB9+nTmzp3L2LFjwzqmmTNnMnPmTH7605/y+uuvc/z4cRoaEuremgTcCfw0REDeZKJLKd8OPBHGE2x0AT2OU6Uu2GCzgdui2MTprUfZ2umnj5uA2WB/ZeHrEW5/QKuTu4Ff9rX71QtrjH/JRvv7CB4mb8Ba063eFZFMvWp5LZ7tMxTQpeNyaG1CNtAaNmwYEyZMYNWqVcybNy+sYH45gB85coTKykqqq6upr6+nsbGR5ubmiNPcfr8fn89HU1MTZ86c4eDBg7zyyivk5eXhdruZNWsWM2d2f2Cp66+/nvT0dLZu3crRo0c5cyahZltcGyqgxyDtPnPJc/bq7vTZXfa0Hekn/BToJ8Q13e6w3IeJsK940K+66jdtHDxnAxEH9MsTtvyyj962XiL8h8lhC59l5nbY18UN0bAx/MF7rFq3K6AngubmZk6ePMm4ceMi3kYgEIhZX2uXy8XAgQMpKChg0aJF3Hnnnd1ar6mpqX141uLiYsrKyjhx4gR1dXUxG/jGWktLSwstLS1UV1e3H29WVhYVFRVUVFRw7NgxrrnmGgoKCrp8CBk3bhzjxo1rH1O+ubk5pscbpUVut3uUx+Op6OjDWKTd2waJebLL8uXidqJ5xRT/dHtUs5sZR+fp9su2rDU7F2+wx4DxEZ6ThUuesflFD5iyvnbPanHxp2QvjYQ5bbQjOPtayIC+8Flm4gh7JEKfw6vJWBTQE0BdXR0HDx6MKqDv3buXurq6mBxPVlYWkyZN4q677mLFihXdXm/Lli0UFRWxY8cO6uvr2ydO6Wk+n4+qqipqamr44IMPePvtt5k9ezbLli1j5crujUuxdu1aBgwYQFJSErt3725/WOhlDuAe4P+ECFwvRBPQ20aNe7LrxaJr3W7jOHb7wo12GpZro9iEp2it2dPNWuFzBv4u4vPi5BHgb/vaPeu9NaZp8Ub7J8KdpMewCvhON4J+uLXzrVsfNjWKJj17M5JuOH/+PCUlJTQ1NUVcw3/vvfe4cOFClLUaQ25uLlOmTOG2225j3rx53VrvzTff5O///u959tln2bVrF9XV1TQ2Nsa1lmutxefz0djY2D487jPPPMMTTzzBu+++SyDQ9aiTCxYsYOXKlUyaNKnHuwCGIeSUqpVONgPR3MjmLd9oQ9aG2robLYpiH5cycuI3mIwjOKtZ5L+DbjbcAsDZdU2+Cw/evck6++J9y0bQfQ3Ldddvsl0Nw3tjBNdMrdsV0BNDVVUVR44c4cCBAxGtf/jwYQ4ePMj58+cjPgan00lGRgYTJkxg4cKF3HzzzV02squurubXv/41r776Ktu3b6e0tJQzZ8706tjpl1Pyp0+fpqSkhK1bt/LKK6/w8ssvU1VVFXLd7OxsVq9ezcKFC8nPzyctLQ2ns9fvtbPcbnenHf0PrjGtxkT17tDhs6FnTvO7WE00Gbc4ptsnb7LJwLqoAnqg+wF9273mMIbiKM7NyHPeiCd76VXeJP4AhPvU7vL7uL6zD9uC/RfCvmaajEUBPVHU1NRw4sQJdu7cyZEjR8Ja9+zZs7z//vscOXIkqhp6amoqeXl53HTTTdx///1dLn/q1CleeeUVnn/+ebZs2cLZs2cTrj93U1MTp0+f5rXXXuM3v/kNr732Wrdasj/88MMsXLiQMWPGRNSqvwesCZ1ZiXqQmTui+bwbNbm4pdtzvXwRwh8u9BP2bXnAhPcjDERdS3+EPui9NeYisC3c9QIhWrC3BfvwHh4NxZqMRQE9YVhrqamp4Y033uDFF1/k0KFD3VrvxIkTbNy4kVdeeYWampqIa8Uul4tRo0Zx8803d6v73OV09gsvvMDJkycTeqpSv99PY2MjR48e5aWXXuKpp55i586uh5SeN28ey5YtY9iwYYkQ1O93u92dNkhrS7vXRlwjhWWdpUFXrrcZBCeLiVRc0+0QXd9za8MPztbJc8HnnojP/+qwR1FLECaymeNWhTj/4Y8OF1Dr9nhQo7gwtLS0cOzYMVyu4GkrLy9n7Nix5OXlfWp0tKamJk6dOsXJkycpLi7m3Xff5fDhwxG/rzbGkJOTw6RJk/jKV77S5fLHj4MEilwAACAASURBVB/nzTffZPfu3XzwwQd95oGpurqahoYG/H4/Xq+3fXS+zkyaNIlBgwZx/PhxWlpaers723hgNrC7ow8PrjGtbWO7Pxjh9pNavdwCbPhcuXRyE0Te/cvA7+OVbm/rWrcyik0EHPCrcFfattZ8tHiDfQdYEOF+k70u7gd+1PdqI7yE4V8JrwfEqCXr7ZSiB01pB9mgsF8/BKwCugJ6AvL5fBw5coTTp0+zd+9eJk6cyNSpUxk7diyZmZk0Nzdz4sQJSkpKOHLkCGVlZVF3sUpKSmL27NksX768y2WLi4t57bXXKCoq4tSpU33u/La2tuLxeGhubqa1tZWHHnoo5IA0I0eO5Itf/CJerzcR+qev7SygAzgcbAoEIg7olweN2dDBDfv2aMZDDATiN5iM38VDRDEDooXtW+83FRGuvjGKgH55VLs+F9CL7jcVizfYvW0PnN3nZBXwqYC+6Fk7FRgV5iFUbH/A7FP0UEBP2KB+6dIlPvzwQ6qrqyktLSU1NRWn00kgEGifUe3SpUtRTwWakZHBiBEj2kdbC2XPnj28/vrrbN26lcrKykTppx3R+T137hy7d+9mwIABrFixgunTp3e6/HXXXUdpaSknTpzg1KlTvdlO4F632/0/PR5Phxe80snm3AC1wKAIt7/quk027b01pr2rxeRNNhkvN0dxzPFOt/9ZNCsbIn8X3tZ98F8jvu9Zpi/dYGf10ZnCXgw3oNtgS/Z/+dQ5dLAq7BcXmoxFAT3R+f3+9oDdkwYPHsz06dMpLCwMOelJU1MTRUVF7Nmzp8cmf4mnhoYGysvL2bFjB8nJyUyaNKnT2dfS0tKYNWsW586do66uLqqeBFHKIziv95sdfRiDtHt6io8b4OOuSLmtLMcQ8Ww48Uy3L9pgFwHXRLEJb2sSv464pnqfubB4g30duCnibIblEaDPBfSA5SWH4dthrrZg5Xqb8amx7G1E/c+Vbo8TNYpL5Kctl4uRI0eybt26kNOfXrp0ia1bt/Luu+9SVlZ2xXz/1tZWDh8+zK5du/jjH/+I1+vtdNn58+dz6623MmTIkPY2Dr0kZJ90E2VrcvvZQUJMdK3bDfFr3e4wUbcU/1Nbq+3Iv68Jo/96xyds7ZJf2NS+9lvafr85BHjCXC25xcWyy/9YsskOIPxXFpeqkinS3VwBvV9zOp2MHDmS/Pz8Lucyf+utt3j++ec5ffp0ok4zGjGfz8eJEyd4+eWXeeed0DNZTps2jQkTJpCXl9ebh3y32+3u9IZ/PpnXiaK1O7B6yRYbfGIJziV+axTbqrOt8Um3z/+5HWgtd0UZTDdGfSAuXgIao9hCtk2Jcr75XhJRa/dPtGgPtLAMSA5zn5qMRQFdXC4X+fn5TJ06NeRyZWVl7Nu3jwMHDvR4+r+3VFdXU1JSwp49e0KOAZCWlsbcuXO5+uqrPzcDXBxlEmIUrbabWzQpyGx7hqUAi65hATA0ihv8y0UPm+Z4nJSkNO4BoplqsKG+LvrUbdEaU2/g91FtxPbNPulEMmrcJ1u0O8LvrhbQ+3MFdNXOnaSnpzN16lSWLVsWctlXX32VQ4cOtXf3uhL5/X4aGhrYsWMHb775Zshlb7/9dvLz80lPT+/NEeR6NO3ePja3ia6mGM90uw1EOe+55eW9j5rGWBxLWMPGdmzZgqft6L72Oypax07gdJirjVvwvHW3nbdwu6tpMhYFdMnIyGDUqFHtXeE6c+TIEd5//30qKiqu+HPi9XopLy/n8OHDXbYTGDNmDGPGjCEtLa23Dne12+3utCV7DNLut/G4dRgbVUCPW7p96TN2AoZ50Wwjmtbtn1WZxGtANDP7OJxJ0bXW7xXGWBvBeOoOPzcufcZOAMaG9wzG1iJNxhJXauWegAYNGsSkSZNC9r8uLy/n3Xffpby8PFFmHevZGp611NbWcvLkSbZu3UpWVlank7OMGzeO6dOnc/78eerr63vjcFOBLwLrO/qwrbX7y8ADEW4/b8k1fNXCmCiO8ffxSrcHnDxClIMUWgdDlmy098fkgHxgIbqnPcufYe0TGGP71g+JFzH8t7CeAyyrrAl/pIMo5y8QBfQrQ3Z2NosXLyY/P7/TZYqLiykqKrpi35t3WruqrKSoqIiCgoJOA/q1115LQ0MD7733Xm92YVvbWUBvq3K+gI04oGO7mN6yGzfbuKTb795knZXeyL/nJ77wfyVY5By76DmWboO3+tLvp6GBLQMGUANkhZEeWRyAtHAjulPvz+NOKfcEk5KSwpAhQ5gzZ06n74ADgQBHjx6lrKysWxOZXEkaGhr46KOPOHLkSKdT2SYlJTFlyhSGDh3KgAEDeutQr3e73Z1OeXrBxZ+ILu0+MIp162jhj3F5APNxE8H++VfezbMPNo7b+6jxQtjvtdMMLA7viZHit9aZk7qjK6D3W06nsz2VPHBg5/frffv2UV5ezoULF/rsaHCRam1t5eLFi5SVlVFc3PmMmEOHDmXkyJFkZWX11qG6gLs7+7CttXtv1WDilm7vsy3Cu5cluaMb84YnHGMjmqwlPJqMRQG9vzPGkJeXx6hRoYdKfvfdd/tFQ7jO+P1+PB5PyIAOwcZxgwcP7s1DXdtFLeaF3iln8dnvvPV2KEQ1LG2iS/O1dnGNE1EyfwR69IFOk7EooKuG7nSSl5fH6NGhe8QcO3aMysrKfh3QT58+TXl5ecjl8vPzGTJkSG8e6nVut7vTlo3pWbwOxLsRRH280u0uBw8ASVdyWbSm72UgitaYeuCNHtzFR5qMRQFdF8PhYOTIkSGnDL1w4QKnT5/uFy3bO72Jts1NX1lZSSAQ6HS5KVOmkJPTq1NYm1C19NduMi1xn7jCxi/d3jY72ZWucOlGO7nv/YZ6Lu1uUGM4BXRpr6GPGzeuw8+9Xi/FxcXU19fT2tq/R1P0+XxUV1fzxhtvdDrGe15eHtnZ2aSkpPTmyHH3d/EL3BTPgzFx2t/CDXYOMLk/lMVAH2wnYJP5PdAjI1FZdVdTQO/vXC4XmZmZZGdnd7rMiRMnQrbu7m+ampr44IMPOHfuXKfLDBgwgIEDB3Y6U1scFLjd7mmdfRjntHvc0u3OK7gxXEcPbbN+YvvUq4Xta8x54J0e2PSl+jpNxqKA3s+lpqaSnZ0dspvVmTNn+PDDD0POOtafNDQ0UFFREfL1w8CBA8nOzu7NgA6JknaPU7r9uk02zRru7UdFceiADG7pg8f9Uqw3aOC1tq5xooDefyUlJTFw4EBSUzufmbG6uppz5871+3T7Zc3NzZw+fZqams5Hl8zOziYvL4/k5OTePNT73G53pzl/a+PW2j0u+0nxcScwqF8Vxj7YOM44Yv8eXel2BXRpq6FnZWWFDOgNDQ3U1NRcsZOwhKu1tZWamhqam5tDnteMjIzenKgFYDR0PpZ5Rg5/oufT7vXGy2txSQT0r3T7ZauWbLLD+9IBF601HwLvx3CT3iSXJmNRQBdcLheDBg0KOaFIU1MT9fX1/W4wmU7vHl4vDQ0NIQN6cnJyyIekOOrdtHuc0u2L1tuxwJL++BMOeHmorx20jW3afesba0wtooDe7y+Ew0FSUhIuV+fD6zc2NlJXV6ca+idq6JcuXaKlpaXTZTIzM8nNze3td+gAa9xud1KIWm1Pp8Pjkm43Th4GTH8sj4a+103Pmhim3Y26qymgS/C3YAxJSUkhU8Otra00NTUpoH8qENqQGYv09HQyMzNDPijFyRDg+s4+7OG0e3zS7Y9bB/S9WmoMTVj0nJ3Xlw54+33mAHA8FtvSZCwK6PKJGrrL5QoZ0H0+H62trVhrdcI+E9Q7k5KSQnp6eqIcasi0u4Hf99B+/xCPdPtiN8sJthfov7/jQJ9sPxCLtPv7moyl92n61AThdDpJTU1NhJpknxNqtDigNweV+aw73G73VzweT0PH34NNxsG62D/xxGnwmhg0hrPwY4fhV71xcYwlI0B0/fQtrFm53n799QdNn5kG0RhetJa/imobqHW7Arp8LjApnR5BrcjRZxJNGcAt0HHAysjhT43VXAIyY7jPuKTbF2yw2cBt0R6rN4lvvLfG9NrISYuftc9hoppwZWCrk7uBX/aVQlnk4d3F11AJDI343uVQQE+Ie6FOQWLw+/20tLSEDOgJVNPsM6y1ifaQFO+0e1zS7Q7LfUB03QksL/VmMG+raW6Musz1tT7pj5sA0Y2//tG2tWa/7jYK6PKJ2rnX6w0ZfJKSknp7XPKEPG/daUiYQO0ObnS73TkhAkpMW6PbeLVuj00Q29jbF6eugT8BF6M86QuXPGPz+1QgcET+Hl2TsSQOpdwTqCbp9XpDBp60tDQyMjLw+/3qi87H7Q5CdUmrra2lqqoqkWrpycAdwM86LAet/Ilk6oCBMdhXvTep59PtCzfaaViujXIzF8xINvf2xdn7qPEu3mB/DXw5qt+zk0eAv+0rv6XUQbzRVM3XIgrogd6/bqKAnnA1zebm5pDjtKemppKZmUlDQ4MCOsHBeAYMGBByWNf6+nqqq6sTbfz7tZ0F9KKHTfOSDfZlS0wax/0hHilsh+VLMdjMC0VLTUIUaodhY8BGF9CBB+/eZP/+hTWmTzSKee0m0wL8WHfivk0p9wTR3NxMbW0tjY2NXdbQE2CQlITQnfHv/X5/Io59v9jtdo/qtMYTozR5PNLtkzfZZGLw8GFM76fbL9uylm1ARZQnf2RlKyv1KxUF9H7I5/NRV1cXctSzgQMHkpOTo4D+iYxFTk4OGRkZnS5TV1fHxYsXE62G7gDu6TQWtPInoC7KfTTEI90+pJVbgZwoN1NetLZHpvKM9OnCmlh0nXP0yzHtRQFdvF4vly5dCjkueU5ODsOHD+/tiUYSRlpaGnl5eWRmdt7Lq7a2lsrKykSspXfa2r2tVXpUrd1NnNLtMWnRbXgeYxJqtCS/PwYZA8uty5+2OfqligJ6P9PU1MSFCxeoq+u8YjZixAjGjRuXKJONJEQN/aqrrmLw4MGdV1MbGqitrU3EOeRnud3uCZ1XEqNLl0e7fncse9qOhOjTyg5/4qTbL9v+gNkHfBDlZpK9Lu7XL1UU0PsZv99Pc3MzlZWVnY58Nnr0aCZNmkRaWlq/r6UbYxg4cCDTp09n+PDOZ628dOkSDQ0NiTpgz72dftLCH4k87d7QHIdpLP0uHgKiLYiHtjxgihOzkPFc9OW0703YIgroEqOgfubMGQ4dOtTpMoWFhWRlZYVs2d0fJCUlkZ2dzdy5cztd5sSJE1y8eDGRx7/vtPYWTdo9Xul24M+ij5nRB82eYm0Mjs0yfekGO0t3N1FA72cCgQCnT5+mrKys8wvmcHDVVVeRm5vbb8+T0+lk2LBh5OXlhVyuuLiYCxcuJPJXyXe73bND1O4iSpvHI92+aINdBFwT7XZ8JF66/bJt68xRYE/Uv2urxnGigN5va+jl5eUhlysoKAiZZu4PAX3UqFGMHTs25HLHjx9P9IAOIRrHRZh2j0u6PRZzf1vLzrfXmeMJfn02xuBkrV3yC6uGL9LjNLBMAgb006dPh1xu4cKFHD58mD179vTLqVSTkpLIz89nxowZIZcrLy+nqqoq4QO62+3+hsfj+dxL/ra0e2YiHvTWdeZhuPLfD29dZ34A/CBe+yu6z/wN8Df96fe8dZ25Wnd/1dCvONZa6urqqKys5NSpU50uN3bsWEaPHk1eXh4pKSn96hylpKQwePBg3G43U6ZM6XS5kydPUlFRQXV1daJ/pTxgsUq/iCigX2F8Ph/V1dVs27aNmpqaTpdzu91MmjQp5KAqV6KsrCwmTJjANdd0/vq2srKS9957j5qamkTsf95hLV0lX0QU0K9A1dXV7N69m5MnT3a6zLXXXsuKFStCDqpyJRo6dCjLli3D7XZ3uszhw4fZu3cvTU1NfeVr3eV2u1NU8kVEAf0KU1NTw8GDB0M2jrvcZSs/P58hQ4Zc8efEGENubi5jx45l+fLlITMTx48fp7i4OOQgPYmWeABuVMkXEQX0K0xzczMXL17kgw8+CNmFLS8vj9mzZzN27Fhcriu7fWNqairXXHMN06dPJysrq9PlTp48yfHjxzl79mxfSbdfprS7iCigX2mstTQ2NnLw4EF27NgRctmHH36YmTNnkp6efsUG9cvTpC5evJi77ror5LLbt2/nww8/pKWlpa/1ALjV7XZnqvSLiAL6Fcbv93P06FEOHDjQ5bKzZs1i+fLlZGdnX5HnIi8vj4ULFzJjxoyQ49g3Nzezc+dOTpw40SeTEMAXVfJFJOLKj05B4tbSa2pqOHHiBC+//HLI98Zz587FWsvZs2fbW8lfCYwxZGVlkZ+fz4oVK5g5c2any1ZXV7N161ZOnDgRj+//D8BPgAEhlknq4nMXMPAzf6tQyRcRBfQrkM/n49y5c2zevJm8vDwKCws7rtqlprJkyRJOnjyJ3+9n165dV8T3T05O5pprruG6665j2bJlIZfdvXs3b775JhcuXMDn8/X0oTV5PJ5KoFKlVEQU0KVbqqurKSkp4b333mPEiBGMHDmy02Wvv/56GhoaqK+vp7y8nPr6+j77vQcNGsTo0aNZtmwZ119/fchlL1y4wJ49e9i3b1+8WrYblUwRSTRX7BycOTk5w4AZBCeQ6LMzmVyeVtXhcOBwOJg2bVqny2ZmZpKRkYHX66W+vp7m5mZaWlr61Pc1xpCZmcm4ceOYN28et99+OyNGjAi5zq9//Wt27NhBWVlZp1PPxtgbVVVV7+j2ISKqoUtYfD4fJSUlpKenc+edd5KWltbpspMmTWLSpEm4XC62bdtGSUlJn/quycnJjBkzhmXLlvHoo492ufypU6d48803OXLkSFyfO1QqpSt7ZtlBaZYh1skAHLT6HNROf4/TBpNw3S8OLLDZLi+5gQAZBrw2QN35gZxeWmR8upIK6BJjzc3NlJWV8Z3vfId77rkn5DjmAKtWrSI9PZ3s7GwOHTrUF2YdIy8vjwkTJrBw4ULmz5/f5fLvvPMOf/jDHygvL6exsTEhAvqBOfYFEyDtM0t/Achp/7fllU98Zq2h2QSoBY4bB3sz0tk6tsg0d/dgSgrtX2NZGuJoT07dbf4y5LEaRgHTP7fuJ4+1mwJO/tv0naYinvs5MMfeZwLcF2r5qXvMLR39vXi2neCA/xNyX5Z/mL7X7Au1zIklNrWhkTUBWG0sC4Dh7fmiADgDUFpIYwn2fQOvWXh66m7zUU9e484Uz7NDna2sw7DCwhxayPG3XwrAAbkN+Epm26MYirC8OGUPb4TzMNLhbwGwhg+n7TaPdWcbpXPsQzbA3Z197gjwPybvM2WKEArofa6WfuHCBXbt2kVubi6pqank5+d3urzb7Wbw4MFkZ2eTlJSEx+OhsrKS1tbWhOqf7XQ6SU5OJjc3l4KCAhYvXsytt95KcnJyyPWOHz/O22+/zb59+6iqqopHQ7huBXRjuRFDRhdr3/yZddq3aC3UN1J/oNA+nezjOxP3m9PdOJ6Zn93mp2MlB2N1rN06Ob5Pt+6Px34MuCPZRrAQkk0g9Louw487P7/WUTKbr9Y38I9Abhfpm3RgnoV5wD+WzLZPuxz8r4JdpqonrnFHgdzh4//FyyPWkNRlfDAUAAUYvlJayNES7Den7ja/7db16eS6G2D/F+y/zNxhPuxyI5aCUN874OJxRYdPPODoFPQdDQ0NfPTRR2zfvp0//vGPXS6fm5vLnXfeyde//nVuuOEGBg8e3GWgjPsTpcvFsGHDuPXWW3nssce46667unWMmzdvZs+ePe2DyMRZz6bcLQMM/KXXxeHS2VYjyCWw/TNs1sFCXjeGfyP8tjouDF/yWg4cmGtn9fSxlsy2SxxeSrA8Cl0G845cA/ympNA+8+51Ni2q370vdDZFVEPvF/x+PydPnmTHjh3k5OSwYsUKcnND30cmTJjAqlWryMvLY/fu3Xg8HsrLy/H7/b1SWzfGkJqayqhRo5gwYQIzZ86ksLCQCRMmdLnuiRMnKCoq4o033uDDDz/srcsQr3fomdawsbTQZk/ZbZ5S6U8se2bZdJeD1y0URlmYRhDgzZIv2Oum7jCHe+JYS+fapTbAqwQHMIrW/Zk+cvbMsl+cvdd4I3pmNdwHfEelSAG937t06RLHjh1j8+bN+P1+li5dGrI7G8DUqVOZOnUqubm55ObmkpWVRVVVFZcuXaKurq7HU9bGGJKSksjIyGDQoEEMHTqUgoIC5s2bx5IlS7q1jYqKCjZv3sz27ds5fvx4b3bLi2ujOAv/WjrbFk/ZY9SyPoEkO/ghUQbzTxiEj9+UTrYzphw0MZ2EoHieHWq9bIpRML9cJm9McfI48L8j/AFNLp1jp0/ZZYpVkhTQ+72LFy+yc+dO/H4/gUCABx98sFvrrVq1ilWrVnHp0iXWr1/Pzp07OXLkCA0NDT0a1J1OJ5mZmRQUFDBnzhxWr15NXl5eWNvYvHkzW7ZsYd++fb19+uPdyt1pDf9hsdMTsYV0f1Qyx87G8ucxLlUFgQweA56M5WYdPr5LT3Tdtfyvg9faX0TaKC0QbMSogK6ALhBsKHfkyBFaW1uprKzk5ptvpqCgoFvrZmZmsnLlSsaNG0dFRQUnT57k1KlTnD9/nosXL9LQ0BDxu2ljDMnJyQwcOJDs7GyGDh3KqFGjGD16dHuaPZxgfnkUuF27dlFRkRCjo/ZG25OpJbO5kT28qpKfACx/3RMPdsby9U132x+tecH4Y7G90kI73Foe6KkHzYCTvwK+Gtl9gnst9pt6SFVAlzbV1dV4vV4aGxvx+/1cvHixW12+INgS3u12A3D48GGKi4spKyvj3LlzVFVVUV9fT0tLC36/n9bWVvz+zu8xSUlJuFwunE4nqampZGRkMGTIEPLy8sjPz2fGjBkhW+V3Zvv27bz11lvs3bu3txrAJQ7DGlBA720nltjU+oYem0hn9KQTfAGIyesVa7gNG1EDuO6602IfM5hIRnQafWAOC9jFdpUqBXRpU19fz5EjR6iurub06dOMGTOGUaNGhbWNgoKCT9XuA4EAZWVlHD16lPLycs6cOUNzc3OHc4xfro0PGzaM0aNHM3bs2G41cOvK/v37efbZZzl48CDnz59PrNAauQZj+buPK3qk42A+lpu6sdOlcf6enzrWbgeRNM4l6H5iorGReUBXrbwtlmcclqccTsp8lnTgDoINwdJDr8j1sQroWBZ3c8m9wM8tHMUwwFiWAH/R1bECQw8VUsDu7nWb+ywHrAMFdAX07hTlj/+74l2ena24uJhvfetbzJ49m/nz5zN79uyItudwOHC73QwfPpwLFy5QVVVFa2srXq+3w9p5SkoKOTk55OTkkJkZ3bTee/bsoaioiH379lFeXk5tbW3i1ZUjL5U/nLLH/PCzfy6ZbR/B8PMu1r6qeJrNmH7ANMTpF9ThsfbZ/cRIwDKlG4s9OXWP+ZtP/LsK+FFJoT0LPB+ycBkmx/Bwu9yWNTz3wRge+Eya/6UDs+zPjYP3IPQYAn7DJIgsoGO5a88s+7VIW8tL/6qhB/rTxWxpaeHs2bNUVVXR1NREVVUV586do6CggHHjxkW0zczMzPax1XvakSNHKC0tpbi4mJKSEsrKyjrMCPTxGnqHpu4x/1VSaP8C+EKo/SY5yQUakN68+qO6qiY4XB13yZq62/yqpNA+Dkzs9KZlGB3Dox3S1W2DZP6yo3f20/aaktJC+39scLrgzr+r7XIfoeSkOLkB+IMKlgJ6VzV0f38L6tZaWltbKS0tpby8nOLiYpYtW8aqVatikgbvyWD+4osv8u6771JRUUFjY2NCjWjX0wG9baNbbeiATiC5ixHXJB7VhIFdlIDKye+ZiyGuc4kNEdCNZWAMjzajizK3a+rbprrTzwO8ah2hAzrm0yMDRnCjvk8BXQG9O7Xz1v4W0D8Z2BsaGqioqODVV19l//79jB8/nhkzZlBYWMjw4cN7/RgrKyt55513OHDgAGVlZZw5c4bq6mqam5sTOZj3WEDH8lFXW/b7NbpjAkjqonQ0dXFjqjSh14/lcI4hy0vAhG6HYAKc7arEBWx0ZdJYvhjXV0kK6H2SF6htC+r9ks/no76+nvr6es6dO8fp06c5c+YMR48eJT8/n7y8PK666qq4BvdTp05RUVHR3lXO4/Fw/Phxzpw5k6jp9bgFdOvAjzrwiI3fOAfGErJ7nDcZv6Pnq0TpJoXbgA26+AronWkGzkDop+X+oqmpifLyck6dOsXOnTvJzc1l4sSJzJgxg+XLlzN69OgeP4YjR46wdetW3n//fY4dO8b58+fbu8UleI08PjV06Rvx1hCIpgAYQyOWi4CP4GtBvwGfDf6vPwDH+9s5NcGhYBXQFdA7j2FAOVCvy9x2I7IWn89HU1MT586do6mpiWPHjvH6668zePBgcnJyyMvLY/jw4eTl5ZGXl8eYMWPC3s+JEyfaswGVlZWcPXu2vaV8fX09tbW11NfX94XUugK6dHTxG7uoYWeH+njqLvMN4Bs6k586ZyuPzLK5E/aaCzoZCugdaQQ+Iph290KPDq7Qp/j9fpqamtoDO8CAAQPIzMxs73o2ePBgsrKyyM7OJjMzk6SkJJKTk0lKSsLpdLY/IAQCAbxeb3uXttraWqqrq6mpqaGqqorq6mqqqqqora2loeGKeUWmgN6/A/qFLh5DM0vn2klTdppDOlvdj0VeB2sATUKkgN6hZuAUcL6tlp6lG3Hn6uvraWhooLKyEgj2QzcmeLouB/OMjAxSU1M/FdD9fj/Nzc00NjbS0tLSPh785WB/+QGij9bEFdDlcwKGI10NVhoI8F2LvU3DmoZTSec+BXQF9A55PB4L+N1u90mC76RmAE5d8hA/qLaU/Ge1tLTgcrloaGjA5XK1B/pPruP3+3t8xjYFdEkEyV7e8bq6LCC3lhTyk9JG+1isZ0+7gs07NNOOmbTfnNSpiEx/6AJzFDgMSgtB8wAACu1JREFU+HW5I+fz+WhpaaGhoaG95fzlWv0na+YK6HKlm7jfnAZ2daOQ/IVNZ2fpLLtAZ617vyufi7U6DQrooZQC+xXQRQFdYlYALD/o5qIzrIPtJYX25dI5drrOXJc/rPt0FiLXHyZnqWyrpR8BxgKDdNlFAb1b3/KWkjl2R3cXt5ZD03ab4wm7nxiassc8X1Jov0RwIpXuWG0tt5TMtr9zGP5u8m5zsJ//hsqBER3EoKkHZtmp0/aaEt1mFNA/x+Px1Lnd7g+BtwnOkKSALgro3TMdy+/DOCn/E/iXBN5PTAUMX3bAAWy3hz41GG4LwOoDhfa/bBJ/N/1dU9lPf0PngWJg9Wc/cDi4D/iWbjPh6y/DSJ4GNgHHQGNxiQK6xOBpZ5c5YS23EP5kOU4Df+Hw4ikptF+x2H5Znozl6U4+ure/nhMF9O6pAz4AdgIlBEdoElFAl6hM2222Bgw3ApEMiDIIeKqkkDeK59mh/e7kNfF74HMTw1i4+uBs5ql0KaB3yOPxtHo8nvPAe8B2Ph5sRkQBXaKtqW83MBXLKxEWpmUOLzuK59pR/em8tXXn63BueGtYp5KlgN6VncBrBBvJ1enyiwK6xCQ47TZnp+4xtwD3Gvgwgk2MdQT4/dF8m9KvAlCg07T73VuWWJdKlgJ6KPXAQeDnwN62f+uduojExNTd5lcp1Uwk2HDvfJirz2jK5u/60/mavNfsJNgD6bNyc+pZqRKlgN4pj8cTINhA7kXgTeAQwQYt6qMuqqFLTFxTZlqm7jb/EmhhLPC3wMUwCtbf7Jtph/SrE2ZZ3+G5MOqTHq7+mNLwAjXAb9v+92vAaGCgioMooH/2Xks4/aUvJPp+4mn6AdMAfHf/DPsfSS7+3hq+TtfDT6cmOfkz4P/rR7+mZ4AnOqhg3mYt/6XHZwX0ULX0y2O8nyLYQM4BrAIKgaFovHdRQL8cZb89bY/5uytmP71k5vumBvjrkkL7G+AloKsa+I39KaBP3W0+Ki20Wyws/8xHGRi+qNtN9zn66xf3eDyNgAfYAPwO2EFw9KI6IKCiIaqhS4wD17vWwY101W3WMKO/nRvbeZ/00So5Cujd5SfYMO4l4PvAeoLv1b2osZwooEuMTdtp9tpgJSKU7D2zbL8a0TLQym8x1KuEKKBHU0u3Ho8n4PF4LhKcke0PwM+AnwLbgIq2p2kFd1FAlxhVR3mvq0WSXP2rTc/0A6bBWn6jwhEd9fP7OLjXAnvcbvflEeWWAV8ACoAsIANIbfvPibIbCugikdWiaruqITh8pPS78+LgaRvgIZUQBfRYagTKgHPAH4FRwHRgCjARGN8W4FN0qhTQpf8pmW1XYCgwFoc1OGn7X2NxGIMDODdlt/lZVMHN9r8yNnknRaWFlKP35groMaypB4BmoNntdlcTnH71HMEx4IcDuQRnbUvS2eq39usU9OvHuYeAde3Th5i2JzzT/m5uL8FXdxLWaTW2xNpnMPxvnQ0F9J4I7j6CfdVrCK+frIhcQaxRO5p48DtY77QK6JHSe2CRRAkatsv+ybgUWDoWoLWrRYqn2YxO1s3oRvWxffu269kak0Ne525k91qTYzQjZNflZXCoD52WnBjso9tm7DIe6LrRoCigiyT6j3Fql3ErQLPOVIdBpcsuT85U8js874ZruvGwVffxrrqc/3zcnlm206BtTMfH8UmpgRhNHmW7LC8zQ02CEgjwhS5PvaUpxtdyvQq0ArpIn3Ww0E62cEeXNXQnNTpbHaroOrbxlc//zToIfP7vHQStik8E9DNdLJ6R4uRLHX3w7nU2zcIDXazfUjImRtfZdLmd3NwG/rKzjIYx/HWXuzCxLZM2mV8BLSrS4dM7dJH41iTzD15r22tofkOacTI/YPlHuh52uLpgl6nSSeyo+s3hLqdYsvzFgdm21mH592Y4kwwTSw3fxTCtizVrpuw2Zz/xYHCoGzXjHxyYbW2r5b9m7zVegNJZdrT18VNgbBdrH13zgonVhFEe6DIj8OSB2XaEsfx0yl5OHJhGmklmkTF8F7rOXvgNnlheymlvm+rSQvuyhbtVsBXQRRLZPQEn93yittftYYtscHwE6cCUHXxQWsh5Qo+T7jCGb1jDN1KCZ91088QXffKfXh/bk1wECJ3hTDWG/0xx8C8lhfYEhjRrGd/NfW6L2YkJsAvDTV0s5TCGb2D4RmkhAUd4mdtmr48Dsb6e1rD+/2/vXkKtrKI4gP/X1ZvSFQKhSZMoDK3uUSoxnBkYvQbRrBckjaWCoFE1c1hERDVo0gOiBvaid6GDAtNzk5ulGJEpoj0kRDEq7j27wTVIut1HnpMpv9/sTPY+Z33fx9prf2fvnSahz5cpdzhbivvKG6LwTxMf1ZJ5xWc+67w3//XD1TvrpySfzHEwsCRJJy3L5tpnmzreuT9xGcrrA84JH60eq1/6fT2PnJ/3MrVkGAkdzjlH60ReEoYZitFenhlAsz8uPppXpxkOPDGQH9Gyp7MjH/dt5mJ7jdfUqZIZ0Pd9chDNXre1JlrNuuc9EjqcfVrLI6NflcMrZrBqrD5vlZf7W/nn4cu+qd/+nijzenLqVHx/8mMePDnb0DeTQ3kogzlB8v1Otz4cYHJ63l0tocO55rVON08Lw+wWTea+VPb3qbm3rtyR56ZP9NUWTGRDpnaR7M/gofLUym692/eBzme1LZVH+9zsweGJ3DvIazm6vcbT+v9+XkIHzpRXlozkzkr1hGJ2y8fqSKbOHD94Wg21bBk+L3fNVC1fsbP2V+WGlhzqQ2n+wu6L88Cg4tLZXpvSsqlPze3rtaxfsbMODfp6tiFVuoQOZ7lKvmvJ3Z0ddfslW8tmMvNJXttqz/BErq3k31S7vSSPLT6am1Z8WrNu7jK6vcYX9rI6bd5/PvvTsWrZ2OnWPX1cqjZ9XLr1cLXcehozGC0tLwwtzOpV3dr7nzwHU+/RJ9zVc2PZGpx5v2fqvIBvq9Lt9fLOaDfvq8r/vZPV4827Vrfrk9yfyvrMfELiz0k2Z0Ee72yrPfPp64qxOpzkti/WtLVDLRtbckuSC2YZsH2d5MXhXp5dPlZH/qu4jHbrzX3r2gfHT+SOSjYkWZvZt6I9nOS11suzK8dq1zy6+3WGHDOnjWM6n9UPX65pb7eWG6cdfU3GM3LqfQVwbhtf2UZqUa5Jy+VVWZrKovRyIsn3vZav9l6a8X5VyFvWtYVLj2dlLciKarkoLSNVmWzJsUr2pZfx0bE68H+Iy5dXtiWTI7mqWpYNVS5syfnVMtGSY61yoJfsPrm/OgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABw7vsDlXdIhPqL5VsAAAAASUVORK5CYII=";
            base64 = base64.Split(',')[1];
            return Convert.FromBase64String(base64);
        }
        public static byte[] GetVamDiffusionLoading()
        {
            var base64 = "data:image/png;base64,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";
            base64 = base64.Split(',')[1];
            return Convert.FromBase64String(base64);
        }
        public static GameObject FindInactive(string name)
        {
            Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
            foreach (Transform t in allTransforms)
            {
                if (t.gameObject.name == name)
                {
                    return t.gameObject;
                }
            }
            return null;
        }
    }
    public class CompResizeOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public float resizeMultiplier = 2f;        
		private Vector2 originalSize;
        private RectTransform rectTransform;
        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                originalSize = rectTransform.sizeDelta;
            }
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = originalSize * resizeMultiplier;
            }
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = originalSize;
            }
        }
    }
}
