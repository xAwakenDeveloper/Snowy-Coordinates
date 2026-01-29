using MSCLoader;
using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

namespace SnowyCoordinates
{
    public class SnowyCoordinates : Mod
    {
        public override string ID => "SnowyCoordinates";
        public override string Name => "Snowy Coordinates";
        public override string Author => "Awaken Developer";
        public override string Version => "1.0.2";
        public override string Description => "Displays your current coordinates in a modern way!";
        public override byte[] Icon { get => base.Icon; set => base.Icon = value; }
        public override Game SupportedGames => Game.MyWinterCar;

        private bool showDisplay = false;
        private Rect coordWindow = new Rect(Screen.width - 350, 40, 320, 200);
        private Vector3 playerPosition;
        private GameObject playerObject;
        private bool guiInitialized = false;
        private Texture2D bgTexture;
        private Texture2D borderTexture;
        private Texture2D roundedBoxTexture;
        private GUIStyle labelStyle;
        private GUIStyle buttonStyle;
        private readonly Color bgColor = new Color(0.08f, 0.09f, 0.12f, 0.95f);
        private readonly Color borderColor = new Color(0.12f, 0.13f, 0.16f, 1f);
        private Color buttonBgColor;
        private Color buttonHover;

        public override void ModSetup()
        {
            SetupFunction(Setup.OnLoad, Mod_OnLoad);
            SetupFunction(Setup.Update, OnUpdate);
            SetupFunction(Setup.OnGUI, OnGUI);
            SetupFunction(Setup.ModSettings, ModSettings);
        }

        private void ModSettings()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream("SnowyCoordinates.icon.png"))
            {
                if (stream != null)
                {
                    byte[] iconBytes = new byte[stream.Length];
                    stream.Read(iconBytes, 0, iconBytes.Length);
                    Icon = iconBytes; 
                }
            }
        }

        private void Mod_OnLoad()
        {
            ModConsole.Log("[<color=#34d8eb>Snowy Coordinates</color>] Mod loaded successfully! Press <color=#34d8eb>F5</color> to show menu.");
        }

        private void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                showDisplay = !showDisplay;
            }

            if (playerObject == null)
                playerObject = GameObject.Find("PLAYER");

            if (playerObject != null)
                playerPosition = playerObject.transform.position;
        }

        void OnGUI()
        {
            if (!showDisplay) return;

            if (!guiInitialized)
            {
                InitializeGUI();
                guiInitialized = true;
            }

            GUI.skin = null;
            coordWindow = GUI.Window(0, coordWindow, DrawCoordWindow, "");
        }

        void InitializeGUI()
        {
            bgTexture = CreateSolidTexture(new Color(0.08f, 0.09f, 0.12f, 1f));
            borderTexture = CreateSolidTexture(new Color(0.12f, 0.13f, 0.16f, 1f));
            roundedBoxTexture = CreateRoundedBoxTexture(32);

            buttonBgColor = borderColor;
            buttonHover = new Color(borderColor.r + 0.05f, borderColor.g + 0.05f, borderColor.b + 0.05f, 1f);

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                richText = true,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = {
            textColor = Color.white,
            background = CreateSolidTexture(buttonBgColor)
            },

            hover = {
            textColor = Color.white,
            background = CreateSolidTexture(buttonHover)
            },

            active = {
            textColor = Color.white,
            background = CreateSolidTexture(buttonHover)
            },
                padding = new RectOffset(10, 10, 6, 6),
                margin = new RectOffset(2, 2, 2, 2)
            };
        }


        Texture2D CreateSolidTexture(Color c)
        {
            var t = new Texture2D(1, 1);
            t.SetPixel(0, 0, c);
            t.Apply();
            t.filterMode = FilterMode.Point;
            t.wrapMode = TextureWrapMode.Clamp;
            return t;
        }

        Texture2D CreateRoundedBoxTexture(int radius)
        {
            int size = 256;
            Texture2D tex = new Texture2D(size, size, TextureFormat.ARGB32, false);

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    bool corner = false;

                    if (x < radius && y < radius &&
                        Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius)) > radius)
                        corner = true;

                    if (x >= size - radius && y < radius &&
                        Vector2.Distance(new Vector2(x, y), new Vector2(size - radius, radius)) > radius)
                        corner = true;

                    if (x < radius && y >= size - radius &&
                        Vector2.Distance(new Vector2(x, y), new Vector2(radius, size - radius)) > radius)
                        corner = true;

                    if (x >= size - radius && y >= size - radius &&
                        Vector2.Distance(new Vector2(x, y), new Vector2(size - radius, size - radius)) > radius)
                        corner = true;

                    tex.SetPixel(x, y, corner ? Color.clear : Color.white);
                }
            }

            tex.Apply();
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            return tex;
        }

        void DrawCoordWindow(int windowID)
        {
            int topBarHeight = 18;
            float gap = 12f;
            float boxW = 85;
            float boxH = 70;

            GUI.DrawTexture(new Rect(0, 0, coordWindow.width, coordWindow.height), borderTexture);
            GUI.DrawTexture(new Rect(1, topBarHeight, coordWindow.width - 2, coordWindow.height - topBarHeight - 1), bgTexture);

            GUI.DrawTexture(new Rect(0, 0, coordWindow.width, topBarHeight), borderTexture);
            GUI.DragWindow(new Rect(0, 0, coordWindow.width, topBarHeight));

            GUI.Label(new Rect(0, topBarHeight + 4, coordWindow.width, 25),
                "<size=18><b><color=#34d8eb>Snowy Coordinates</color></b></size>", labelStyle);

            float totalW = boxW * 3 + gap * 2;
            float startX = (coordWindow.width - totalW) / 2;
            float yPos = topBarHeight + 25 + gap;

            DrawCoordBox("X", playerPosition.x.ToString("F2"), new Rect(startX, yPos, boxW, boxH));
            DrawCoordBox("Y", playerPosition.y.ToString("F2"), new Rect(startX + boxW + gap, yPos, boxW, boxH));
            DrawCoordBox("Z", playerPosition.z.ToString("F2"), new Rect(startX + (boxW + gap) * 2, yPos, boxW, boxH));

            float btnW = 80;
            float btnH = 25;
            float totalBtnsWidth = btnW * 2 + gap;
            float btnStartX = (coordWindow.width - totalBtnsWidth) / 2;
            float btnY = yPos + boxH + gap;

            if (GUI.Button(new Rect(btnStartX, btnY, btnW, btnH), "COPY", buttonStyle))
                CopyToClipboard();

            if (GUI.Button(new Rect(btnStartX + btnW + gap, btnY, btnW, btnH), "SAVE", buttonStyle))
                SaveCoordinates();

            // hint
            float hintY = btnY + btnH + gap;
            GUI.Label(new Rect(0, hintY, coordWindow.width, 16),
                "<size=10><color=#8899AA>Press F5 to toggle the window.</color></size>",
                labelStyle);
        }

        void DrawCoordBox(string label, string value, Rect rect)
        {
            GUI.color = new Color(0.20f, 0.85f, 0.90f, 0.2f);
            GUI.DrawTexture(rect, roundedBoxTexture);
            GUI.color = Color.white;

            GUI.Label(new Rect(rect.x, rect.y + 5, rect.width, 22),
                $"<size=14><b><color=#34d8eb>{label}</color></b></size>",
                labelStyle);

            GUI.Label(new Rect(rect.x, rect.y + 28, rect.width, 30),
                $"<size=16><b>{value}</b></size>",
                labelStyle);
        }

        [DllImport("user32.dll")]
        private static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll")]
        private static extern bool CloseClipboard();

        [DllImport("user32.dll")]
        private static extern bool EmptyClipboard();

        [DllImport("user32.dll")]
        private static extern IntPtr SetClipboardData(uint format, IntPtr hMem);

        private const uint CF_UNICODETEXT = 13;

        void CopyToClipboard()
        {
            string text =
                playerPosition.x.ToString("R", CultureInfo.InvariantCulture) + ", " +
                playerPosition.y.ToString("R", CultureInfo.InvariantCulture) + ", " +
                playerPosition.z.ToString("R", CultureInfo.InvariantCulture);

            IntPtr hGlobal = Marshal.StringToHGlobalUni(text);

            if (OpenClipboard(IntPtr.Zero))
            {
                EmptyClipboard();
                SetClipboardData(CF_UNICODETEXT, hGlobal);
                CloseClipboard();

                ModConsole.Log($"[<color=#34d8eb>Snowy Coordinates</color>] Coordinates XYZ copied to clipboard: {text}");
            }
            else
            {
                Marshal.FreeHGlobal(hGlobal);
                ModConsole.Log("[<color=#34d8eb>Snowy Coordinates</color>] Clipboard busy!");
            }
        }

        void SaveCoordinates()
        {
            try
            {
                string assemblyFolder = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string folderPath = System.IO.Path.Combine(assemblyFolder, "SavedCoordinates");
                string filePath = System.IO.Path.Combine(folderPath, "coordinates.txt");

                if (!System.IO.Directory.Exists(folderPath))
                    System.IO.Directory.CreateDirectory(folderPath);

                string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] XYZ: {playerPosition.x:R}, {playerPosition.y:R}, {playerPosition.z:R}";

                System.IO.File.AppendAllText(filePath, line + Environment.NewLine);

                ModConsole.Log($"[<color=#34d8eb>Snowy Coordinates</color>] Coordinates XYZ saved to file: {line}");
            }
            catch (Exception ex)
            {
                ModConsole.Error($"[Snowy Coordinates] Failed to save coordinates: {ex.Message}");
            }
        }
    }
}
