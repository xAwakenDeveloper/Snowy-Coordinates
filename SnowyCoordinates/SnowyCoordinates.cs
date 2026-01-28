using MSCLoader;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace SnowyCoordinates
{
    public class SnowyCoordinates : Mod
    {
        public override string ID => "SnowyCoordinates";
        public override string Name => "Snowy Coordinates";
        public override string Author => "Awaken Developer";
        public override string Version => "1.0.5";
        public override string Description => "Displays your current coordinates in a modern way!";
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
        }

        private void Mod_OnLoad()
        {
            ModConsole.Log("[Snowy Coordinates] Mod loaded successfully. Press INSERT to show menu.");
        }

        private void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Insert))
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
            bgTexture = CreateSolidTexture(bgColor);
            borderTexture = CreateSolidTexture(borderColor);
            roundedBoxTexture = CreateRoundedBoxTexture(32);

            buttonBgColor = borderColor;
            buttonHover = new Color(borderColor.r + 0.05f, borderColor.g + 0.05f, borderColor.b + 0.05f, 1f);

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                richText = true,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.95f, 0.96f, 0.98f, 1f) }
            };

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = {
                    textColor = new Color(0.95f, 0.96f, 0.98f, 1f),
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

            GUI.DrawTexture(new Rect(0, 0, coordWindow.width, topBarHeight), borderTexture);
            GUI.DragWindow(new Rect(0, 0, coordWindow.width, topBarHeight));

            GUI.DrawTexture(new Rect(0, topBarHeight, coordWindow.width, coordWindow.height - topBarHeight), bgTexture);

            GUI.DrawTexture(new Rect(0, topBarHeight, coordWindow.width, 1), borderTexture);
            GUI.DrawTexture(new Rect(0, coordWindow.height - 1, coordWindow.width, 1), borderTexture);
            GUI.DrawTexture(new Rect(0, topBarHeight, 1, coordWindow.height - topBarHeight), borderTexture);
            GUI.DrawTexture(new Rect(coordWindow.width - 1, topBarHeight, 1, coordWindow.height - topBarHeight), borderTexture);

            GUI.Label(new Rect(0, topBarHeight + 4, coordWindow.width, 25),
                "<size=18><b><color=#34d8eb>Snowy Coordinates</color></b></size>",
                labelStyle);

            float gap = 12f;

            float yPos = topBarHeight + 25 + gap;
            float boxW = 85;
            float boxH = 70;
            float spacing = gap;
            float totalW = boxW * 3 + spacing * 2;
            float startX = (coordWindow.width - totalW) / 2;

            DrawCoordBox("X", playerPosition.x.ToString("F2"), new Rect(startX, yPos, boxW, boxH));
            DrawCoordBox("Y", playerPosition.y.ToString("F2"), new Rect(startX + boxW + spacing, yPos, boxW, boxH));
            DrawCoordBox("Z", playerPosition.z.ToString("F2"), new Rect(startX + (boxW + spacing) * 2, yPos, boxW, boxH));

            float btnY = yPos + boxH + gap;
            float btnW = boxW;
            float btnStartX = startX;

            if (GUI.Button(new Rect(btnStartX + boxW + spacing, btnY, btnW, 25), "COPY", buttonStyle))
                CopyToClipboard();
            
            float bottomY = btnY + 20 + gap;

            GUI.Label(
                new Rect(0, bottomY, coordWindow.width, 16),
                "<size=10><color=#8899AA>Press INSERT to toggle the window.</color></size>",
                labelStyle
            );
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
            string text = $"{playerPosition.x}, {playerPosition.y}, {playerPosition.z}";
            IntPtr hGlobal = Marshal.StringToHGlobalUni(text);

            if (OpenClipboard(IntPtr.Zero))
            {
                EmptyClipboard();
                SetClipboardData(CF_UNICODETEXT, hGlobal);
                CloseClipboard();
                ModConsole.Log($"<color=yellow>[Snowy Coordinates] XYZ copied to clipboard: {text}</color>");
            }
            else
            {
                Marshal.FreeHGlobal(hGlobal);
                ModConsole.Log("<color=red>[Snowy Coordinates] Clipboard busy!</color>");
            }
        }
    }
}
