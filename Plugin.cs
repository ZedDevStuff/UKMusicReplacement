using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UKUIHelper;

namespace UKMusicReplacement
{
    [BepInPlugin("zed.uk.musicreplacement", "Ultrakill Music Replacement", "0.6.0")]
    [BepInProcess("ULTRAKILL.exe")]
    [BepInDependency("zed.uk.uihelper",BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        GameObject menuCanvas,menu,menuButton,toggleTemplate;
        List<GameObject> toggles = new List<GameObject>();
        MusicManager music;
        ConfigEntry<bool> isEnabled;
        Dictionary<string,ConfigEntry<bool>> musicEntries = new Dictionary<string, ConfigEntry<bool>>();
        Dictionary<string,bool> musicDict = new Dictionary<string, bool>();

        bool changedSong = false;
        bool isMenuOpen,isModEnabled;
        string workDir;
        private void Awake()
        {
            
            isEnabled = Config.Bind("General", "Enabled", true, "Enable/Disable the plugin");
            isModEnabled = isEnabled.Value;
            if(isEnabled.Value)
            {
                Logger.LogInfo("Loaded UK Music Replacement");
                workDir = Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(Plugin)).Location);
                SceneManager.sceneLoaded += CheckScene;
            }
            
        }
        
        void CheckScene(Scene scene,LoadSceneMode mode)
        {
            music = MusicManager.Instance;
            string name = scene.name;
            if(name.Contains("Level") && changedSong == false)
            {
                name = name.Replace("Level ", "");
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
            menuButton.GetComponentInChildren<Text>().text = "MR";
            menuButton.GetComponentInChildren<Text>().fontSize = 42;
            menuButton.GetComponentInChildren<Text>().alignment = TextAnchor.MiddleCenter;
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
            menu.GetComponent<RectTransform>().SetAnchor(AnchorPresets.StretchAll);
            menu.GetComponent<RectTransform>().SetRect(new Rect4(50,50,450,450));
            menu.GetComponent<Image>().color = new Color(1,1,1,1);
            menu.SetActive(false);

            GameObject title = UIHelper.CreateText();
            title.GetComponent<RectTransform>().SetParent(menu.GetComponent<RectTransform>());
            title.GetComponent<RectTransform>().SetAnchor(AnchorPresets.HorStretchTop);
            title.GetComponent<RectTransform>().SetPivot(PivotPresets.TopLeft);
            title.GetComponent<RectTransform>().anchoredPosition = new Vector2(0,0);
            title.GetComponent<Text>().text = "Custom Music Menu";
            title.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
            title.GetComponent<Text>().fontSize = 30;
            title.GetComponent<Text>().fontStyle = FontStyle.Bold;
            title.GetComponent<Text>().color = Color.black;

            GameObject exit = UIHelper.CreateButton();
            exit.GetComponent<RectTransform>().SetParent(menu.GetComponent<RectTransform>());
            exit.GetComponent<RectTransform>().SetAnchor(AnchorPresets.TopRight);
            exit.GetComponent<RectTransform>().SetPivot(PivotPresets.TopRight);
            exit.GetComponent<RectTransform>().anchoredPosition = new Vector2(0,0);
            exit.GetComponent<RectTransform>().sizeDelta = new Vector2(50,30);
            exit.GetComponent<Image>().color = Color.red;
            exit.GetComponentInChildren<Text>().text = "X";
            exit.GetComponentInChildren<Text>().fontSize = 32;
            exit.GetComponentInChildren<Text>().color = Color.white;
            exit.GetComponent<Button>().onClick.AddListener(() => ToggleMenu());

            GameObject refresh = UIHelper.CreateButton();
            refresh.GetComponent<RectTransform>().SetParent(menu.GetComponent<RectTransform>());
            refresh.GetComponent<RectTransform>().SetAnchor(AnchorPresets.BottomRight);
            refresh.GetComponent<RectTransform>().SetPivot(PivotPresets.BottomRight);
            refresh.GetComponent<RectTransform>().anchoredPosition = new Vector2(0,0);
            refresh.GetComponent<RectTransform>().sizeDelta = new Vector2(100,50);
            refresh.GetComponent<Image>().color = Color.green;
            refresh.GetComponentInChildren<Text>().text = "Refresh";
            refresh.GetComponentInChildren<Text>().fontSize = 24;
            refresh.GetComponentInChildren<Text>().color = Color.white;
            refresh.GetComponent<Button>().onClick.AddListener(() => RefreshMenu());

            GameObject desc = UIHelper.CreateText();
            desc.GetComponent<RectTransform>().SetParent(menu.GetComponent<RectTransform>());
            desc.GetComponent<RectTransform>().SetAnchor(AnchorPresets.HorStretchTop);
            desc.GetComponent<RectTransform>().SetPivot(PivotPresets.TopCenter);
            desc.GetComponent<RectTransform>().anchoredPosition = new Vector2(0,-40);
            desc.GetComponent<RectTransform>().sizeDelta = new Vector2(0,80);
            desc.GetComponent<Text>().text = "This menu allows you to change the music on a per level basis";
            desc.GetComponent<Text>().fontSize = 24;
            desc.GetComponent<Text>().color = Color.black;
            desc.GetComponent<Text>().horizontalOverflow = HorizontalWrapMode.Wrap;

            toggleTemplate = UIHelper.CreateText();
            toggleTemplate.GetComponent<RectTransform>().SetAnchor(AnchorPresets.TopLeft);
            toggleTemplate.GetComponent<RectTransform>().SetPivot(PivotPresets.TopLeft);
            toggleTemplate.GetComponent<RectTransform>().sizeDelta = new Vector2(60,40);
            toggleTemplate.GetComponent<Text>().text = "";
            toggleTemplate.GetComponent<Text>().fontSize = 20;
            toggleTemplate.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
            toggleTemplate.GetComponent<Text>().color = Color.black;

            GameObject toggleTemplateToggle = UIHelper.CreateToggle();
            toggleTemplateToggle.GetComponent<RectTransform>().SetParent(toggleTemplate.GetComponent<RectTransform>());
            toggleTemplateToggle.GetComponent<RectTransform>().SetAnchor(AnchorPresets.MiddleRight);
            toggleTemplateToggle.GetComponent<RectTransform>().SetPivot(PivotPresets.MiddleRight);
            toggleTemplateToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(0,0);
            toggleTemplateToggle.GetComponent<RectTransform>().sizeDelta = new Vector2(10,10);

            Logger.LogInfo("Created mod menu");
            RefreshMenu();
            isMenuOpen = false;
        }
        void RefreshMenu()
        {
            foreach(GameObject go in toggles)
            {
                Destroy(go);
            }
            toggles.Clear();
            string[] potentialSongs = Directory.GetDirectories(workDir + "\\CustomMusic");
            List<string> songs = new List<string>();
            foreach(string potentialSong in potentialSongs)
            {
                if(Directory.EnumerateFiles(potentialSong).Any(file => file.EndsWith(".ogg") || file.EndsWith(".mp3") || file.EndsWith(".wav")))
                {
                    songs.Add(potentialSong);
                }
            }
            int current = 0;
            int check = 0;
            musicEntries.Clear();
            foreach(string song in songs)
            {
                if(current == 10)
                {
                    check++;
                    current = 0;
                }
                else if(current == 20)
                {
                    check++;
                    current = 0;
                }
                string name = new DirectoryInfo(song).Name;
                ConfigEntry<bool> cfg;
                cfg = Config.Bind("Music " + name, "Enabled", true, "Enable/Disable music in " + name);
                bool val = true;
                if(musicDict.ContainsKey(name))
                {
                    val = musicDict[name];
                }
                else
                {
                    musicDict.Add(name, cfg.Value);
                    val = cfg.Value;
                }
                musicEntries.Add(name,cfg);
                GameObject toggle = Instantiate(toggleTemplate);
                toggle.name = name;
                toggle.GetComponent<RectTransform>().SetParent(menu.GetComponent<RectTransform>());
                toggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(20 + (100 * check),-150 - (current * 50));
                toggle.GetComponent<Text>().text = new DirectoryInfo(song).Name;
                toggle.GetComponentInChildren<Toggle>().gameObject.name = name;
                toggle.GetComponentInChildren<Toggle>().isOn = val;
                toggle.GetComponentInChildren<Toggle>().onValueChanged.AddListener((bool value) => ToggleMusic(value));
                toggle.GetComponentInChildren<Image>().color = new Color32(200,200,200,255);
                toggles.Add(toggle);

                current++;
            }
        }
        void ToggleMusic(bool value)
        {
            string name = EventSystem.current.currentSelectedGameObject.name;
            Logger.LogInfo("Toggled " + name + " to " + value);
            if(musicEntries.ContainsKey(name))
            {
                musicEntries[name].Value = value;
                musicDict[name] = value;
            }
            
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
                if(Directory.EnumerateFiles(workDir + "\\CustomMusic\\" + name).Any(file => file.EndsWith(".ogg") || file.EndsWith(".mp3") || file.EndsWith(".wav")) && musicDict[name])
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
                        if(typeName.Contains("ogg"))
                        {
                            type = AudioType.OGGVORBIS;
                        }
                        else if(typeName.Contains("mp3"))
                        {
                            type = AudioType.MPEG;
                        }
                        else if(typeName.Contains("wav"))
                        {
                            type = AudioType.WAV;
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
                    Logger.LogInfo("No music found in " + name);
                }
            }
            else
            {
                Logger.LogInfo("No custom music found for " + name);
            }
        }
    }
}
