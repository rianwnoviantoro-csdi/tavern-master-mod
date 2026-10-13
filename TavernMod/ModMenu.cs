using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace TavernMod
{
    public class ModMenu : MonoBehaviour
    {
        public static ModMenu Instance { get; private set; }

        // Window & UI State
        private bool isVisible = false;
        private Rect windowRect = new Rect(40, 40, 520, 600);
        private int activeTab = 0;
        private readonly string[] tabNames = new string[] { "💰 Ekonomi", "👨‍🍳 Karyawan", "👥 Tamu", "📦 Stok & Operasi", "⚡ Riset & Waktu" };

        // Mod Toggles (User can turn ANY of them ON / OFF independently)
        public static bool InfiniteMoney = false;
        public static bool InfiniteIngredients = false;
        public static bool InfiniteDrinks = false;
        public static bool StaffGodMode = false;
        public static bool StaffSuperSpeed = false;
        public static float StaffSpeedMultiplier = 2.5f;
        public static bool FreezeCustomerPatience = false;
        public static bool AutoCleanTables = false;
        public static float CustomGameSpeed = 1.0f;

        // Backup original speed curves
        private static bool curvesBackedUp = false;
        private static AnimationCurve origWaitressEmpty;
        private static AnimationCurve origWaitressCarrying;
        private static AnimationCurve origWaitressStairs;
        private static AnimationCurve origBartenderEmpty;
        private static AnimationCurve origBartenderCarrying;
        private static AnimationCurve origBartenderDrink;
        private static AnimationCurve origChefSpeed;
        private static AnimationCurve origChefSoup;
        private static AnimationCurve origChefMain;
        private static AnimationCurve origChefDessert;
        private static AnimationCurve origScientistSpeed;
        private static AnimationCurve origScientistRead;

        // Reflection for CustomerController.TimeRemaining
        private static readonly PropertyInfo TimeRemainingProp = typeof(CustomerController).GetProperty("TimeRemaining");
        private float updateTimer = 0f;
        private Vector2 scrollPos = Vector2.zero;

        // Styles & Textures
        private Texture2D bgTex;
        private Texture2D borderTex;
        private Texture2D headerTex;
        private Texture2D goldAccentTex;
        private Texture2D itemCardTex;
        private GUIStyle windowStyle;
        private GUIStyle itemCardStyle;
        private GUIStyle cardContainerStyle;
        private GUIStyle titleStyle;
        private GUIStyle tabStyle;
        private GUIStyle tabActiveStyle;
        private GUIStyle toggleOnStyle;
        private GUIStyle toggleOffStyle;
        private GUIStyle buttonStyle;
        private GUIStyle sectionHeaderStyle;
        private GUIStyle labelStyle;
        private bool stylesInitialized = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Init()
        {
            if (Instance != null) return;
            GameObject go = new GameObject("TavernMaster_ModMenu");
            UnityEngine.Object.DontDestroyOnLoad(go);
            Instance = go.AddComponent<ModMenu>();
            Debug.Log("[TavernMod] ModMenu Initialized successfully!");
        }

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            // Toggle menu visibility with F1, F2, BackQuote (~), or Fn+F1
            if (Input.GetKeyDown(KeyCode.F1) || 
                Input.GetKeyDown(KeyCode.F2) || 
                Input.GetKeyDown(KeyCode.BackQuote))
            {
                isVisible = !isVisible;
            }

            // Periodic updates for active cheats (runs 4 times per second to ensure optimal performance)
            updateTimer += Time.unscaledDeltaTime;
            if (updateTimer >= 0.25f)
            {
                updateTimer = 0f;
                ApplyActiveToggles();
            }
        }

        private void ApplyActiveToggles()
        {
            try
            {
                // 1. Infinite Money
                if (InfiniteMoney && TavernModel.I != null)
                {
                    if (TavernModel.I.money < 999999)
                    {
                        TavernModel.I.ChangeMoney(999999 - TavernModel.I.money, TavernData.SummaryItem.Rewards);
                    }
                }

                // 2. Infinite Ingredients (Food Crates)
                if (InfiniteIngredients && FoodsModel.I != null && FoodsModel.I.CrateInfos != null)
                {
                    for (int i = 0; i < FoodsModel.I.CrateInfos.Count; i++)
                    {
                        var crate = FoodsModel.I.CrateInfos[i];
                        if (crate != null && crate.Ingredients != null)
                        {
                            for (int j = 0; j < crate.Ingredients.Count; j++)
                            {
                                var ing = crate.Ingredients[j];
                                if (ing != null) ing.Amount = ing.Max;
                            }
                        }
                    }
                }

                // 3. Infinite Drinks (Kegs / Barrels in Bars)
                if (InfiniteDrinks && DrinksModel.I != null && DrinksModel.I.BarInfos != null)
                {
                    for (int i = 0; i < DrinksModel.I.BarInfos.Count; i++)
                    {
                        var bar = DrinksModel.I.BarInfos[i];
                        if (bar != null && bar.DrinkInfos != null)
                        {
                            for (int j = 0; j < bar.DrinkInfos.Count; j++)
                            {
                                var drink = bar.DrinkInfos[j];
                                if (drink != null) drink.Amount = drink.Capacity;
                            }
                        }
                    }
                }

                // 4. Staff God Mode (100% Mood / Anti-fatigue)
                if (StaffGodMode && StaffModel.I != null && StaffModel.I.StaffInfos != null)
                {
                    for (int i = 0; i < StaffModel.I.StaffInfos.Count; i++)
                    {
                        var staff = StaffModel.I.StaffInfos[i];
                        if (staff != null) staff.Mood = 100;
                    }
                }

                // 5. Customer Infinite Patience (Never angry / Never walk out)
                if (FreezeCustomerPatience && TimeRemainingProp != null)
                {
                    CustomerController[] customers = UnityEngine.Object.FindObjectsOfType<CustomerController>();
                    for (int i = 0; i < customers.Length; i++)
                    {
                        var c = customers[i];
                        if (c != null && c.IsWaitingForOrder)
                        {
                            TimeRemainingProp.SetValue(c, c.TimeRemainingOriginal);
                        }
                    }
                }

                // 6. Auto Clean Dirty Tables & Dishes
                if (AutoCleanTables && OrdersModel.I != null && OrdersModel.I.Orders != null)
                {
                    for (int i = OrdersModel.I.Orders.Count - 1; i >= 0; i--)
                    {
                        var o = OrdersModel.I.Orders[i];
                        if (o != null && (o.OrderType == OrdersModel.OrderType.Clean || o.OrderType == OrdersModel.OrderType.CleanFood || o.OrderType == OrdersModel.OrderType.CleanRoom))
                        {
                            if (o.CleanupObject != null)
                            {
                                for (int j = 0; j < o.CleanupObject.childCount; j++)
                                {
                                    o.CleanupObject.GetChild(j).gameObject.SetActive(false);
                                }
                            }
                            OrdersModel.I.Orders.RemoveAt(i);
                        }
                    }
                }

                // 7. Staff Speed Curves
                ApplyStaffSpeed(StaffSuperSpeed);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[TavernMod] Error applying cheats: " + ex.Message);
            }
        }

        private void ApplyStaffSpeed(bool enable)
        {
            if (StaffSettings.I == null) return;

            if (!curvesBackedUp)
            {
                origWaitressEmpty = CloneCurve(StaffSettings.I.StatWaitressWalkingSpeedEmpty);
                origWaitressCarrying = CloneCurve(StaffSettings.I.StatWaitressWalkingSpeedCarrying);
                origWaitressStairs = CloneCurve(StaffSettings.I.StatWaitressClimbingStairs);
                origBartenderEmpty = CloneCurve(StaffSettings.I.StatBartenderWalkingSpeedEmpty);
                origBartenderCarrying = CloneCurve(StaffSettings.I.StatBartenderWalkingSpeedCarrying);
                origBartenderDrink = CloneCurve(StaffSettings.I.StatBartenderDrinkFillingSpeed);
                origChefSpeed = CloneCurve(StaffSettings.I.StatChefSpeed);
                origChefSoup = CloneCurve(StaffSettings.I.StatChefSoupMaking);
                origChefMain = CloneCurve(StaffSettings.I.StatChefMainCourseMaking);
                origChefDessert = CloneCurve(StaffSettings.I.StatChefDessertMaking);
                origScientistSpeed = CloneCurve(StaffSettings.I.StatScientistWalkingSpeed);
                origScientistRead = CloneCurve(StaffSettings.I.StatScientistReadingSpeed);
                curvesBackedUp = true;
            }

            float mult = enable ? StaffSpeedMultiplier : 1.0f;
            StaffSettings.I.StatWaitressWalkingSpeedEmpty = ScaleCurve(origWaitressEmpty, mult);
            StaffSettings.I.StatWaitressWalkingSpeedCarrying = ScaleCurve(origWaitressCarrying, mult);
            StaffSettings.I.StatWaitressClimbingStairs = ScaleCurve(origWaitressStairs, mult);
            StaffSettings.I.StatBartenderWalkingSpeedEmpty = ScaleCurve(origBartenderEmpty, mult);
            StaffSettings.I.StatBartenderWalkingSpeedCarrying = ScaleCurve(origBartenderCarrying, mult);
            StaffSettings.I.StatBartenderDrinkFillingSpeed = ScaleCurve(origBartenderDrink, mult);
            StaffSettings.I.StatChefSpeed = ScaleCurve(origChefSpeed, mult);
            StaffSettings.I.StatChefSoupMaking = ScaleCurve(origChefSoup, mult);
            StaffSettings.I.StatChefMainCourseMaking = ScaleCurve(origChefMain, mult);
            StaffSettings.I.StatChefDessertMaking = ScaleCurve(origChefDessert, mult);
            StaffSettings.I.StatScientistWalkingSpeed = ScaleCurve(origScientistSpeed, mult);
            StaffSettings.I.StatScientistReadingSpeed = ScaleCurve(origScientistRead, mult);
        }

        private AnimationCurve CloneCurve(AnimationCurve src)
        {
            if (src == null) return new AnimationCurve();
            return new AnimationCurve(src.keys);
        }

        private AnimationCurve ScaleCurve(AnimationCurve src, float multiplier)
        {
            if (src == null) return new AnimationCurve();
            Keyframe[] keys = new Keyframe[src.keys.Length];
            for (int i = 0; i < src.keys.Length; i++)
            {
                Keyframe k = src.keys[i];
                k.value *= multiplier;
                keys[i] = k;
            }
            return new AnimationCurve(keys);
        }

        private void InitStyles()
        {
            if (stylesInitialized) return;

            bgTex = MakeTex(2, 2, new Color(0.10f, 0.11f, 0.14f, 0.99f)); // Solid deep dark slate
            borderTex = MakeTex(2, 2, new Color(0.28f, 0.32f, 0.42f, 1f)); // Frame border
            headerTex = MakeTex(2, 2, new Color(0.15f, 0.17f, 0.22f, 1f)); // Header bar
            goldAccentTex = MakeTex(2, 2, new Color(0.95f, 0.75f, 0.22f, 1f)); // Tavern Gold stripe
            itemCardTex = MakeTex(2, 2, new Color(0.15f, 0.17f, 0.22f, 0.98f)); // Item card

            Texture2D tabNormTex = MakeTex(2, 2, new Color(0.18f, 0.20f, 0.26f, 1f));
            Texture2D tabActTex = MakeTex(2, 2, new Color(0.24f, 0.46f, 0.86f, 1f));
            Texture2D btnTex = MakeTex(2, 2, new Color(0.22f, 0.26f, 0.35f, 1f));
            Texture2D onTex = MakeTex(2, 2, new Color(0.16f, 0.65f, 0.30f, 1f));
            Texture2D offTex = MakeTex(2, 2, new Color(0.35f, 0.38f, 0.45f, 1f));

            windowStyle = new GUIStyle(GUI.skin.window);
            windowStyle.normal.background = bgTex;
            windowStyle.onNormal.background = bgTex;
            windowStyle.fontStyle = FontStyle.Bold;
            windowStyle.padding = new RectOffset(12, 12, 10, 12);

            titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 16;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = new Color(1f, 0.85f, 0.3f);
            titleStyle.alignment = TextAnchor.MiddleCenter;

            tabStyle = new GUIStyle(GUI.skin.button);
            tabStyle.normal.background = tabNormTex;
            tabStyle.normal.textColor = Color.white;
            tabStyle.fontSize = 11;
            tabStyle.margin = new RectOffset(2, 2, 2, 2);

            tabActiveStyle = new GUIStyle(tabStyle);
            tabActiveStyle.normal.background = tabActTex;
            tabActiveStyle.fontStyle = FontStyle.Bold;

            toggleOnStyle = new GUIStyle(GUI.skin.button);
            toggleOnStyle.normal.background = onTex;
            toggleOnStyle.normal.textColor = Color.white;
            toggleOnStyle.fontStyle = FontStyle.Bold;
            toggleOnStyle.fontSize = 12;

            toggleOffStyle = new GUIStyle(GUI.skin.button);
            toggleOffStyle.normal.background = offTex;
            toggleOffStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);
            toggleOffStyle.fontSize = 12;

            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.normal.background = btnTex;
            buttonStyle.normal.textColor = Color.white;
            buttonStyle.fontSize = 12;
            buttonStyle.margin = new RectOffset(4, 4, 4, 4);

            sectionHeaderStyle = new GUIStyle(GUI.skin.label);
            sectionHeaderStyle.fontStyle = FontStyle.Bold;
            sectionHeaderStyle.fontSize = 13;
            sectionHeaderStyle.normal.textColor = new Color(0.4f, 0.8f, 1.0f);
            sectionHeaderStyle.margin = new RectOffset(0, 0, 8, 4);

            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.normal.textColor = Color.white;
            labelStyle.fontSize = 12;

            itemCardStyle = new GUIStyle();
            itemCardStyle.normal.background = itemCardTex;
            itemCardStyle.padding = new RectOffset(10, 10, 8, 8);
            itemCardStyle.margin = new RectOffset(2, 2, 4, 4);

            cardContainerStyle = new GUIStyle();
            cardContainerStyle.normal.background = MakeTex(2, 2, new Color(0.12f, 0.13f, 0.17f, 0.95f));
            cardContainerStyle.padding = new RectOffset(10, 10, 8, 8);
            cardContainerStyle.margin = new RectOffset(0, 0, 4, 4);

            stylesInitialized = true;
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++) pix[i] = col;
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private void OnGUI()
        {
            InitStyles();

            // Screen HUD Toggle Button in Top-Left Corner
            Rect hudBtnRect = new Rect(10, 10, 140, 32);
            string hudText = isVisible ? "✖ Tutup Menu" : "⚡ TAVERN MOD";
            if (GUI.Button(hudBtnRect, hudText, isVisible ? toggleOffStyle : toggleOnStyle))
            {
                isVisible = !isVisible;
            }

            if (!isVisible) return;

            // Render Mod Window
            windowRect = GUI.Window(999901, windowRect, DrawModWindow, "", windowStyle);
        }

        private void DrawModWindow(int windowID)
        {
            // 1. Draw 100% Solid Dark Background & Frame (covers game 3D graphics completely)
            GUI.DrawTexture(new Rect(0, 0, windowRect.width, windowRect.height), borderTex);
            GUI.DrawTexture(new Rect(2, 2, windowRect.width - 4, windowRect.height - 4), bgTex);

            // 2. Header bar with tavern gold accent stripe
            GUI.DrawTexture(new Rect(2, 2, windowRect.width - 4, 40), headerTex);
            GUI.DrawTexture(new Rect(2, 40, windowRect.width - 4, 3), goldAccentTex);

            GUILayout.Space(6);
            GUILayout.Label("🏰 TAVERN MASTER - MOD MENU v1.0", titleStyle);
            GUILayout.Space(12);

            // Tabs Row
            GUILayout.BeginHorizontal();
            for (int i = 0; i < tabNames.Length; i++)
            {
                bool isCurrent = (activeTab == i);
                if (GUILayout.Button(tabNames[i], isCurrent ? tabActiveStyle : tabStyle, GUILayout.Height(30)))
                {
                    activeTab = i;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(8);

            // Scrollable Tab Content with clean dark card container
            scrollPos = GUILayout.BeginScrollView(scrollPos, false, false, GUILayout.ExpandHeight(true));
            GUILayout.BeginVertical(cardContainerStyle);

            switch (activeTab)
            {
                case 0:
                    DrawTabEconomy();
                    break;
                case 1:
                    DrawTabStaff();
                    break;
                case 2:
                    DrawTabCustomers();
                    break;
                case 3:
                    DrawTabOperations();
                    break;
                case 4:
                    DrawTabResearchAndTime();
                    break;
            }

            GUILayout.EndVertical();
            GUILayout.EndScrollView();

            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            GUILayout.Label("💡 Shortcut: [F1] / [~] / [F2] untuk buka/tutup menu.", labelStyle);
            if (GUILayout.Button("✖ Tutup", buttonStyle, GUILayout.Width(75), GUILayout.Height(26)))
            {
                isVisible = false;
            }
            GUILayout.EndHorizontal();

            // Allow window dragging from header
            GUI.DragWindow(new Rect(0, 0, 10000, 42));
        }

        // ==================== TAB 1: EKONOMI ====================
        private void DrawTabEconomy()
        {
            GUILayout.Label("💰 PENGATURAN KEUANGAN", sectionHeaderStyle);

            DrawToggleItem("Infinite Money (Otomatis 999,999 Gold)", ref InfiniteMoney,
                "Gold tidak akan pernah habis, selalu dijaga di angka maksimal.");

            GUILayout.Space(10);
            GUILayout.Label("⚡ Aksi Cepat Tambah Koin:", labelStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+20,000 Gold", buttonStyle, GUILayout.Height(32)))
            {
                if (TavernModel.I != null) TavernModel.I.ChangeMoney(20000, TavernData.SummaryItem.Rewards);
            }
            if (GUILayout.Button("+100,000 Gold", buttonStyle, GUILayout.Height(32)))
            {
                if (TavernModel.I != null) TavernModel.I.ChangeMoney(100000, TavernData.SummaryItem.Rewards);
            }
            if (GUILayout.Button("+1,000,000 Gold", buttonStyle, GUILayout.Height(32)))
            {
                if (TavernModel.I != null) TavernModel.I.ChangeMoney(1000000, TavernData.SummaryItem.Rewards);
            }
            GUILayout.EndHorizontal();

            if (TavernModel.I != null)
            {
                GUILayout.Space(10);
                GUILayout.Label("Saldo Saat Ini: <color=#FFD700>" + TavernModel.I.money.ToString("N0") + " Gold</color>", labelStyle);
            }
        }

        // ==================== TAB 2: KARYAWAN ====================
        private void DrawTabStaff()
        {
            GUILayout.Label("👨‍🍳 MANAJEMEN KARYAWAN (Pelayan, Koki, Bartender, Peneliti)", sectionHeaderStyle);

            DrawToggleItem("Staff God Mode (100% Mood & Anti-Lelah)", ref StaffGodMode,
                "Semua karyawan selalu bahagia, tidak pernah lelah, dan tidak akan pernah resign.");

            DrawToggleItem("Staff Super Speed (Jalan & Kerja Cepat)", ref StaffSuperSpeed,
                "Pelayan, koki, dan bartender bergerak serta melayani jauh lebih cepat.");

            if (StaffSuperSpeed)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label($"Kecepatan: {StaffSpeedMultiplier:0.0}x", labelStyle, GUILayout.Width(130));
                StaffSpeedMultiplier = GUILayout.HorizontalSlider(StaffSpeedMultiplier, 1.5f, 6.0f);
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(10);
            GUILayout.Label("⚡ Aksi Cepat Karyawan:", labelStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Naikkan Semua ke Level 9 (Max)", buttonStyle, GUILayout.Height(32)))
            {
                UpgradeAllStaffMax();
            }
            if (GUILayout.Button("+1 Level Semua Karyawan", buttonStyle, GUILayout.Height(32)))
            {
                UpgradeAllStaffOneLevel();
            }
            GUILayout.EndHorizontal();
        }

        // ==================== TAB 3: TAMU ====================
        private void DrawTabCustomers()
        {
            GUILayout.Label("👥 KEPUASAN & PERILAKU TAMU", sectionHeaderStyle);

            DrawToggleItem("Infinite Patience (Tamu Tidak Pernah Marah)", ref FreezeCustomerPatience,
                "Waktu tunggu pesanan di-freeze. Tamu tidak akan pernah kesal atau walk-out.");

            GUILayout.Space(10);
            GUILayout.Label("⚡ Aksi Tamu:", labelStyle);
            if (GUILayout.Button("Beri 1,000 Tamu untuk Riset", buttonStyle, GUILayout.Height(32)))
            {
                AddResearchCustomers(1000);
            }
        }

        // ==================== TAB 4: OPERASI & STOK ====================
        private void DrawTabOperations()
        {
            GUILayout.Label("📦 PERSEDIAAN & KEBERSIHAN", sectionHeaderStyle);

            DrawToggleItem("Infinite Ingredients (Bahan Makanan Selalu Penuh)", ref InfiniteIngredients,
                "Semua peti makanan (daging, sayur, sup, roti, kue) selalu terisi maksimal.");

            DrawToggleItem("Infinite Drinks (Tong Minuman Selalu Penuh)", ref InfiniteDrinks,
                "Semua barel bir, anggur, mead, teh tidak akan pernah kosong.");

            DrawToggleItem("Auto Clean (Meja & Piring Otomatis Bersih)", ref AutoCleanTables,
                "Piring kotor dan sisa makanan di meja langsung dibersihkan otomatis.");

            GUILayout.Space(10);
            GUILayout.Label("⚡ Aksi Cepat Bahan & Resep:", labelStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Isi Penuh Semua Peti & Barel", buttonStyle, GUILayout.Height(32)))
            {
                RefillAllStockNow();
            }
            if (GUILayout.Button("Max Semua Level Makanan (Food XP)", buttonStyle, GUILayout.Height(32)))
            {
                MaxAllFoodXp();
            }
            GUILayout.EndHorizontal();
        }

        // ==================== TAB 5: RISET & WAKTU ====================
        private void DrawTabResearchAndTime()
        {
            GUILayout.Label("⚡ RISET & KONTROL WAKTU", sectionHeaderStyle);

            if (GUILayout.Button("🔓 Buka Semua Riset (Unlock All Research)", buttonStyle, GUILayout.Height(36)))
            {
                UnlockAllTech();
            }

            GUILayout.Space(10);
            GUILayout.Label("⏩ Game Speed Multiplier:", sectionHeaderStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("1x (Normal)", buttonStyle, GUILayout.Height(28))) SetTimeScale(1.0f);
            if (GUILayout.Button("2x (Cepat)", buttonStyle, GUILayout.Height(28))) SetTimeScale(2.0f);
            if (GUILayout.Button("5x (Sangat Cepat)", buttonStyle, GUILayout.Height(28))) SetTimeScale(5.0f);
            if (GUILayout.Button("10x (Ultra)", buttonStyle, GUILayout.Height(28))) SetTimeScale(10.0f);
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Speed Slider: {Time.timeScale:0.0}x", labelStyle, GUILayout.Width(130));
            float newScale = GUILayout.HorizontalSlider(Time.timeScale, 1.0f, 10.0f);
            if (Math.Abs(newScale - Time.timeScale) > 0.05f) SetTimeScale(newScale);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            if (GUILayout.Button("⏭ Lewati Hari (Skip to End of Day)", buttonStyle, GUILayout.Height(32)))
            {
                if (TimeModel.I != null) TimeModel.I.DayTimer = 420f;
            }
        }

        // Helper untuk komponen toggle dengan visual [ON] / [OFF]
        private void DrawToggleItem(string title, ref bool toggleValue, string description)
        {
            GUILayout.BeginVertical(itemCardStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Label(title, labelStyle);
            GUILayout.FlexibleSpace();
            string stateLabel = toggleValue ? "● ON" : "○ OFF";
            GUIStyle btnStyle = toggleValue ? toggleOnStyle : toggleOffStyle;
            if (GUILayout.Button(stateLabel, btnStyle, GUILayout.Width(75), GUILayout.Height(26)))
            {
                toggleValue = !toggleValue;
                ApplyActiveToggles();
            }
            GUILayout.EndHorizontal();
            GUILayout.Label($"<color=#999999>{description}</color>", labelStyle);
            GUILayout.EndVertical();
            GUILayout.Space(4);
        }

        // ==================== ACTIONS ====================
        private void UpgradeAllStaffMax()
        {
            if (StaffModel.I == null || StaffModel.I.StaffInfos == null) return;
            for (int i = 0; i < StaffModel.I.StaffInfos.Count; i++)
            {
                var s = StaffModel.I.StaffInfos[i];
                if (s != null && s.Level < 9)
                {
                    StaffModel.I.UpdateXp(s.Id, 25000);
                }
            }
        }

        private void UpgradeAllStaffOneLevel()
        {
            if (StaffModel.I == null || StaffModel.I.StaffInfos == null) return;
            for (int i = 0; i < StaffModel.I.StaffInfos.Count; i++)
            {
                var s = StaffModel.I.StaffInfos[i];
                if (s != null && s.Level < 9)
                {
                    StaffModel.I.UpdateXp(s.Id, 1000);
                }
            }
        }

        private void AddResearchCustomers(int count)
        {
            if (TechTreeModel.I == null) return;
            for (int i = 0; i < count; i++)
            {
                TechTreeModel.I.ReduceAllCustomers();
            }
        }

        private void RefillAllStockNow()
        {
            if (FoodsModel.I != null && FoodsModel.I.CrateInfos != null)
            {
                for (int i = 0; i < FoodsModel.I.CrateInfos.Count; i++)
                {
                    var crate = FoodsModel.I.CrateInfos[i];
                    if (crate != null && crate.Ingredients != null)
                    {
                        for (int j = 0; j < crate.Ingredients.Count; j++)
                        {
                            var ing = crate.Ingredients[j];
                            if (ing != null) ing.Amount = ing.Max;
                        }
                    }
                }
            }
            if (DrinksModel.I != null && DrinksModel.I.BarInfos != null)
            {
                for (int i = 0; i < DrinksModel.I.BarInfos.Count; i++)
                {
                    var bar = DrinksModel.I.BarInfos[i];
                    if (bar != null && bar.DrinkInfos != null)
                    {
                        for (int j = 0; j < bar.DrinkInfos.Count; j++)
                        {
                            var drink = bar.DrinkInfos[j];
                            if (drink != null) drink.Amount = drink.Capacity;
                        }
                    }
                }
            }
        }

        private void MaxAllFoodXp()
        {
            if (FoodsModel.I == null || FoodsModel.I.XpKeys == null) return;
            for (int i = 0; i < FoodsModel.I.XpKeys.Count; i++)
            {
                FoodsModel.I.AddXp(FoodsModel.I.XpKeys[i], 100);
            }
        }

        private void UnlockAllTech()
        {
            if (TechTreeModel.I == null) return;
            TechTreeModel.I.TechResearchInProgress = null;
            List<TechTreeModel.TechTreeResearchType> list = new List<TechTreeModel.TechTreeResearchType>(TechTreeModel.I.ResearchedTechs);
            TechTreeModel.I.ResearchedTechs.Clear();
            List<TechTreeModel.TechTreeResearchType> allTechTypesInUse = TechTreeModel.I.GetAllTechTypesInUse();
            if (AllSavableModels.I != null && AllSavableModels.I.SelectedTavernType == AllSavableModels.TavernType.Winter)
            {
                allTechTypesInUse.AddRange(TechTreeModel.WinterStartingResearches);
            }
            if (AllSavableModels.I != null && AllSavableModels.I.SelectedTavernType == AllSavableModels.TavernType.Pirate)
            {
                allTechTypesInUse.AddRange(TechTreeModel.PirateStartingResearches);
            }
            TechTreeModel.I.ResearchedTechs.AddRange(allTechTypesInUse);
            for (int i = 0; i < list.Count; i++)
            {
                if (!TechTreeModel.I.ResearchedTechs.Contains(list[i]))
                {
                    TechTreeModel.I.ResearchedTechs.Add(list[i]);
                }
            }
            for (int j = 0; j < allTechTypesInUse.Count; j++)
            {
                TechTreeModel.I.OnResearchDone(allTechTypesInUse[j], j == 0, false);
            }
        }

        private void SetTimeScale(float scale)
        {
            CustomGameSpeed = scale;
            Time.timeScale = scale;
        }
    }
}
