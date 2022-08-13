using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UKUIHelper;

namespace UKMusicReplacement
{
    [BepInPlugin("zed.uk.musicreplacement", "Ultrakill Music Replacement", "0.5.0")]
    [BepInProcess("ULTRAKILL.exe")]
    [BepInDependency("zed.uk.uihelper",BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        GameObject menuCanvas,menu,menuButton; 
        MusicManager music;
        bool changedSong = false;
        bool isMenuOpen,isModEnabled;
        string workDir;
        private void Awake()
        {
            ConfigEntry<bool> enabled;
            enabled = Config.Bind("General", "Enabled", true, "Enable/Disable the plugin");
            isModEnabled = enabled.Value;
            if(enabled.Value)
            {
                Logger.LogInfo("Loaded UK Music Replacement");
                workDir = Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(Plugin)).Location);
                SceneManager.sceneLoaded += CheckScene;
            }
            
        }
        
        void CheckScene(Scene scene,LoadSceneMode mode)
        {
            music = MusicManager.Instance;
            Logger.LogInfo(scene.name);
            string name = scene.name;
            if(name.Contains("Level") && changedSong == false)
            {
                name = name.Replace("Level ", "");
                Logger.LogInfo(name);
                changedSong = true;
                if(Directory.Exists(workDir + "\\CustomMusic"))
                {
                    StartCoroutine(ChangeSong(name));
                    changedSong = false;
                }
                else
                {
                    Directory.CreateDirectory(workDir + "\\CustomMusic");
                    File.WriteAllText(workDir + "\\CustomMusic\\readme.txt","To add custom songs, create a folder with the level name (i.e \"2-1\") and put your files in.\nRename hem to \"clean\" and \"battle\" to make sure they are loaded correctly.\nAudio files need to be in .ogg, .mp3 or .wav to work.");
                } 
            }
            else if(name.Contains("Menu"))
            {
                StartCoroutine(InitMenu());
            }
        }
        IEnumerator InitMenu()
        {
            ColorBlock colors = new ColorBlock()
            {
                normalColor = new Color(0,0,0,0.512f),
                highlightedColor = new Color(1,1,1,0.502f),
                pressedColor = new Color(1,0,0,1),
                selectedColor = new Color(0,0,0,0.512f),
                disabledColor = new Color(0.7843f,0.7843f,0.7843f,0.502f),
                colorMultiplier = 1f,
                fadeDuration = 0.1f
            };
            yield return new WaitForSeconds(0.1f);
            menuCanvas = GameObject.Find("Main Menu (1)");
            yield return new WaitUntil(() => menuCanvas != null);
            Logger.LogInfo("Found Canvas...");
            menuButton = UIHelper.CreateButton();
            menuButton.name = "Custom Music Button";
            menuButton.GetComponent<RectTransform>().SetParent(menuCanvas.GetComponent<RectTransform>());
            menuButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(520,-280);
            menuButton.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
            menuButton.GetComponent<Image>().color = Color.white;
            menuButton.GetComponent<Button>().colors = colors;
            menuButton.GetComponentInChildren<Text>().text = "M";
            menuButton.GetComponentInChildren<Text>().fontSize = 50;
            menuButton.GetComponentInChildren<Text>().color = Color.white;
            menuButton.GetComponentInChildren<Text>().gameObject.GetComponent<RectTransform>().SetAnchor(AnchorPresets.StretchAll);
            menuButton.GetComponentInChildren<Text>().gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 0);

            menuButton.GetComponent<Button>().onClick.AddListener(() => ToggleMenu());

            //Menu creation;
            menu = UIHelper.CreatePanel();
            menu.name = "Custom Music Menu";
            menu.GetComponent<RectTransform>().SetParent(menuCanvas.GetComponent<RectTransform>());
            menu.GetComponent<RectTransform>().pivot = new Vector2(0.5f,0.5f);
            menu.GetComponent<RectTransform>().anchoredPosition = new Vector2(0,0);
            menu.GetComponent<RectTransform>().sizeDelta = new Vector2(500,700);
            menu.GetComponent<Image>().color = new Color(1,1,1,1);
            menu.SetActive(false);
            GameObject title = UIHelper.CreateText();
            title.GetComponent<RectTransform>().SetParent(menu.GetComponent<RectTransform>());
            title.GetComponent<RectTransform>().SetAnchor(AnchorPresets.HorStretchTop);
            title.GetComponent<RectTransform>().anchoredPosition = new Vector2(0,-20);
            title.GetComponent<Text>().text = "Custom Music Menu (WIP)";
            title.GetComponent<Text>().fontSize = 32;
            title.GetComponent<Text>().color = Color.black;

            GameObject wipDesc = UIHelper.CreateText();
            wipDesc.GetComponent<RectTransform>().SetParent(menu.GetComponent<RectTransform>());
            wipDesc.GetComponent<RectTransform>().SetAnchor(AnchorPresets.HorStretchTop);
            wipDesc.GetComponent<RectTransform>().SetPivot(PivotPresets.TopCenter);
            wipDesc.GetComponent<RectTransform>().anchoredPosition = new Vector2(0,-40);
            wipDesc.GetComponent<RectTransform>().sizeDelta = new Vector2(0,60);
            wipDesc.GetComponent<Text>().text = "This menu will allow you to change the music on a per level basis.";
            wipDesc.GetComponent<Text>().fontSize = 24;
            wipDesc.GetComponent<Text>().color = Color.black;
            wipDesc.GetComponent<Text>().horizontalOverflow = HorizontalWrapMode.Wrap;
            
            GameObject toggle = UIHelper.CreateToggle();
            toggle.GetComponent<RectTransform>().SetParent(menu.GetComponent<RectTransform>());
            toggle.GetComponent<RectTransform>().SetAnchor(AnchorPresets.MiddleRight);
            toggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(-20,-100);
            toggle.GetComponent<RectTransform>().sizeDelta = new Vector2(10,10);
            toggle.GetComponentInChildren<Image>().color = new Color32(200,200,200,255);
            Logger.LogInfo("Created mod menu");

            isMenuOpen = false;
        }
        void ToggleMenu()
        {
            isMenuOpen = !isMenuOpen;
            menu.SetActive(isMenuOpen);
        }
        void Update()
        {
            if(isMenuOpen && Input.GetKeyDown(KeyCode.Escape) && isModEnabled)
            {
                ToggleMenu();
            }
        }
        IEnumerator ChangeSong(string name)
        {
            if(Directory.Exists(workDir + "\\CustomMusic\\" + name))
            {
                string[] allFiles = Directory.GetFiles(workDir + "\\CustomMusic\\" + name);
                foreach(string file in allFiles)
                {
                    AudioType type = AudioType.UNKNOWN;
                    string typeName = Path.GetExtension(file);
                    string target = "";
                    if(file.Contains("clean"))
                    {
                        target = "clean";
                    }
                    else if(file.Contains("battle"))
                    {
                        target = "battle";
                    }
                    else if(file.Contains("boss"))
                    {
                        target = "boss";
                    }
                    Logger.LogInfo(typeName);
                    if(file.Contains("ogg"))
                    {
                        type = AudioType.OGGVORBIS;
                        typeName = ".ogg";
                    }
                    else if(file.Contains("mp3"))
                    {
                        type = AudioType.MPEG;
                        typeName = ".mp3";
                    }
                    else if(file.Contains(".wav"))
                    {
                        type = AudioType.WAV;
                        typeName = ".wav";
                    }
                    UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(file,type);
                    yield return request.SendWebRequest();
                    if(request.isHttpError) Logger.LogError(request.error);
                    else
                    {
                        if(target == "clean")
                        {
                            music.cleanTheme.clip = DownloadHandlerAudioClip.GetContent(request);
                        }
                        else if(target == "battle")
                        {
                            music.battleTheme.clip = DownloadHandlerAudioClip.GetContent(request);
                        }
                        else if(target == "boss")
                        {
                            music.bossTheme.clip = DownloadHandlerAudioClip.GetContent(request);
                        }
                    }
                }
            }
            else
            {
                Logger.LogInfo("No custom music found for " + name);
            }
        }
    }
}
