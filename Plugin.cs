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
    [BepInPlugin("zed.uk.musicreplacement", "Ultrakill Music Replacement", "0.7.0")]
    [BepInProcess("ULTRAKILL.exe")]
    [BepInDependency("zed.uk.uihelper")]
    public class Plugin : BaseUnityPlugin
    {
        GameObject menuCanvas,menu,menuButton,toggleTemplate;
        List<GameObject> toggles = new List<GameObject>();
        MusicManager music;
        ConfigEntry<bool> isEnabled;
        Dictionary<string,List<string>> songPackDict = new Dictionary<string, List<string>>();
        Dictionary<string,ConfigEntry<string>> configs = new Dictionary<string, ConfigEntry<string>>();
        Dictionary<string,string> currentSongPackDict = new Dictionary<string, string>();
        bool changedSong = false;
        bool isMenuOpen,isModEnabled,firstInit = true;
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
            InitDict();
            Logger.LogInfo("Built Dict");
        }
        void InitDict()
        {
            songPackDict.Clear();
            int l = 0;
            int le = 1;
            for(int i = 0;i < 25;i++)
            {
                string name;
                bool reset = false;
                if(l == 0 && le == 5)
                {
                    reset = true;
                }
                else if(le == 2 && (l == 3 || l == 6 || l == 9))
                {
                    reset = true;
                }
                else if(le == 4 && l != 0)
                {
                    reset = true;
                }
                name = ($"{l}-{le}");
                songPackDict.Add($"{l}-{le}",new List<string>());
                if(firstInit)
                {
                    if(name == "4-1" || name == "4-2")
                    {
                        configs[name] = Config.Bind($"Music {name}", "Current music", "Example", $"Sound pack used for {name}");
                    }
                    else configs[name] = Config.Bind($"Music {name}", "Current music", "None", $"Sound pack used for {name}");
                } 
                if(reset)
                {
                    l++;
                    le = 1;
                }
                else le++;
            }
            firstInit = false;
        }
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.F1))
            {
                Logger.LogInfo(MusicManager.Instance.cleanTheme.clip.name);
            }
            if(Input.GetKeyDown(KeyCode.F2))
            {
                Logger.LogInfo(MusicManager.Instance.battleTheme.clip.name);
            }
            if(Input.GetKeyDown(KeyCode.F3))
            {
                Logger.LogInfo(MusicManager.Instance.bossTheme.clip.name);
            }
            if(Input.GetKeyDown(KeyCode.F4))
            {
                Logger.LogInfo(MusicManager.Instance.targetTheme.clip.name);
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
                    File.WriteAllText(workDir + "\\CustomMusic\\readme.txt","To add custom songs, create a folder with the name of your sound pack then another with the level name (i.e \"2-1\") and put your files in.\nRename them to \"clean\" and \"battle\" to make sure they are loaded correctly.\nAudio files need to be in .ogg, .mp3 or .wav to work.");
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
            menu.GetComponent<RectTransform>().SetRect(new Rect4(50,50,200,200));
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

            GameObject dp = UIHelper.CreateDropdown();
            dp.name = "Dropdown";
            dp.GetComponent<RectTransform>().SetParent(toggleTemplate.GetComponent<RectTransform>());
            dp.GetComponent<RectTransform>().SetAnchor(AnchorPresets.MiddleLeft);
            dp.GetComponent<RectTransform>().anchoredPosition = new Vector2(140,0);
            dp.GetComponent<Dropdown>().options = new List<Dropdown.OptionData>() {new Dropdown.OptionData("None")};

            Logger.LogInfo("Created mod menu");
            RefreshMenu();
            isMenuOpen = false;
        }
        void RefreshMenu()
        {
            InitDict();
            foreach(GameObject go in toggles)
            {
                Destroy(go);
            }
            toggles.Clear();
            string[] potentialSongPacks = Directory.GetDirectories(workDir + "\\CustomMusic");
            foreach(string potentialSongPack in potentialSongPacks)
            {
                foreach(string potentialSong in Directory.GetDirectories(potentialSongPack))
                {
                    if(Directory.EnumerateFiles(potentialSong).Any(file => file.EndsWith(".ogg") || file.EndsWith(".mp3") || file.EndsWith(".wav")))
                    {
                        string song = new DirectoryInfo(potentialSong).Name;
                        string pack = new DirectoryInfo(potentialSongPack).Name;
                        if(!songPackDict[song].Contains(pack)) songPackDict[song].Add(pack);
                    }
                }
            }
            int check = 0;
            int layer = 0;
            int level = 1;
            int current = 0;
            for(int i = 1;i < 26;i++)
            {
                bool reset = false;
                if(i == 10 || i == 20 || i == 30)
                {
                    check++;
                    current = 0;
                }
                if(layer == 0 && level == 5)
                {
                    reset = true;
                }
                else if(level == 2 && (layer == 3 || layer == 6 || layer == 9))
                {
                    reset = true;
                }
                else if(level == 4 && layer != 0)
                {
                    reset = true;
                }
                string name = $"{layer}-{level}";
                currentSongPackDict[name] = configs[name].Value;
                //string val = "None";
                // TODO: Check and restore previous settings
                
                GameObject toggle = Instantiate(toggleTemplate);
                toggle.name = name;
                toggle.GetComponent<RectTransform>().SetParent(menu.GetComponent<RectTransform>());
                toggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(20 + (230 * check),-150 - (current * 50));
                toggle.GetComponent<Text>().text = name;
                toggle.GetComponentInChildren<Image>().color = new Color32(200,200,200,255);
                Dropdown dp = toggle.GetComponentInChildren<Dropdown>();
                dp.GetComponent<Dropdown>().onValueChanged.AddListener(delegate {ToggleMusic(dp);});;
                dp.name = name;
                if(songPackDict[name].Count > 0)
                {
                    foreach(string data in songPackDict[name])
                    {
                        dp.options.Add(new Dropdown.OptionData(data));
                    }
                }
                bool exists = false;
                for(int j = 0;j < dp.options.Count;j++)
                {
                    if(dp.options[j].text == configs[name].Value)
                    {
                        exists = true;
                        dp.value = j;
                    }
                }
                Logger.LogInfo(exists);
                if(!exists)
                {
                    dp.value = 0;
                }
                toggles.Add(toggle);
                if(reset)
                {
                    layer++;
                    level = 1;
                }
                else level++;
                current++;
            }
        }
        void ToggleMusic(Dropdown dp)
        {
            currentSongPackDict[dp.name] = dp.captionText.text;
            configs[dp.name].Value = dp.captionText.text;
            Logger.LogInfo(configs[dp.name].Value);
        }
        void ToggleMenu()
        {
            isMenuOpen = !isMenuOpen;
            menu.SetActive(isMenuOpen);
        }
        IEnumerator ChangeSong(string level)
        {
            string pack = currentSongPackDict[level];
            string path = $"\\CustomMusic\\{pack}\\{level}";
            if(!pack.Contains("None"))
            {
                if(Directory.Exists(workDir + path))
                {
                    Logger.LogInfo("Exists");
                    if(Directory.EnumerateFiles(workDir + path).Any(file => file.EndsWith(".ogg") || file.EndsWith(".mp3") || file.EndsWith(".wav")))
                    {
                        Logger.LogInfo("Enumerate");
                        string[] allFiles = Directory.GetFiles(workDir + path);
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
                        Logger.LogInfo($"No music found in {pack}\\{level}");
                    }
                }
                else
                {
                    Logger.LogInfo($"No custom music found in {pack} for {level}");
                }
            }
            
        }
    }
}
