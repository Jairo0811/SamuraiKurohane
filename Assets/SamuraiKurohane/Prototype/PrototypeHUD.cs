using UnityEngine;
using UnityEngine.InputSystem;

namespace JMGameStudios.SamuraiKurohane.Prototype
{
    /// <summary>
    /// Immediate-mode HUD (no Canvas needed): vitals, posture, dash gauge, enemy bars,
    /// attack warnings, damage vignette and a toggleable control reference.
    /// </summary>
    [DefaultExecutionOrder(200)]
    public sealed class PrototypeHUD : MonoBehaviour
    {
        private static readonly Color Crimson = new(0.86f, 0.06f, 0.10f, 1f);
        private static readonly Color CrimsonDark = new(0.30f, 0.02f, 0.04f, 1f);
        private static readonly Color Steel = new(0.82f, 0.85f, 0.92f, 1f);
        private static readonly Color SteelDark = new(0.20f, 0.21f, 0.25f, 1f);
        private static readonly Color Amber = new(1f, 0.62f, 0.18f, 1f);
        private static readonly Color Panel = new(0.02f, 0.01f, 0.015f, 0.72f);

        private AkioPrototypeController player;
        private GUIStyle titleStyle;
        private GUIStyle smallStyle;
        private GUIStyle bigStyle;
        private GUIStyle centerStyle;
        private bool showControls = true;
        private float displayedHealth = 1f;
        private float startTime;

        public void Initialize(AkioPrototypeController controller)
        {
            player = controller;
            startTime = Time.unscaledTime;
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame)
            {
                showControls = !showControls;
            }
        }

        private void OnGUI()
        {
            if (player == null || Event.current.type != EventType.Repaint)
            {
                return;
            }

            float scale = Mathf.Max(0.75f, Screen.height / 1080f);
            EnsureStyles(scale);

            DrawDamageVignette();
            DrawPlayerPanel(scale);
            DrawEnemies(scale);
            DrawCenterMessages(scale);
            DrawControls(scale);
        }

        // ------------------------------------------------------------------

        private void EnsureStyles(float scale)
        {
            titleStyle ??= new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
            smallStyle ??= new GUIStyle(GUI.skin.label);
            bigStyle ??= new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            centerStyle ??= new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };

            titleStyle.fontSize = Mathf.RoundToInt(15f * scale);
            smallStyle.fontSize = Mathf.RoundToInt(12f * scale);
            bigStyle.fontSize = Mathf.RoundToInt(44f * scale);
            centerStyle.fontSize = Mathf.RoundToInt(15f * scale);
        }

        private static void Fill(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        private static void Outline(Rect rect, Color color, float thickness)
        {
            Fill(new Rect(rect.x, rect.y, rect.width, thickness), color);
            Fill(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            Fill(new Rect(rect.x, rect.y, thickness, rect.height), color);
            Fill(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
        }

        private static void Label(Rect rect, string text, GUIStyle style, Color color)
        {
            Color previous = GUI.contentColor;
            GUI.contentColor = new Color(0f, 0f, 0f, 0.85f);
            GUI.Label(new Rect(rect.x + 1.5f, rect.y + 1.5f, rect.width, rect.height), text, style);
            GUI.contentColor = color;
            GUI.Label(rect, text, style);
            GUI.contentColor = previous;
        }

        private void DrawDamageVignette()
        {
            float sinceHit = Time.unscaledTime - player.LastDamageTime;
            if (sinceHit < 0.5f)
            {
                float a = (1f - sinceHit / 0.5f) * 0.32f;
                Color c = new(0.9f, 0.02f, 0.03f, a);
                float t = Screen.height * 0.09f;
                Fill(new Rect(0, 0, Screen.width, t), c);
                Fill(new Rect(0, Screen.height - t, Screen.width, t), c);
                Fill(new Rect(0, 0, t, Screen.height), c);
                Fill(new Rect(Screen.width - t, 0, t, Screen.height), c);
            }

            float sinceParry = Time.unscaledTime - player.LastParryTime;
            if (sinceParry < 0.18f)
            {
                Fill(new Rect(0, 0, Screen.width, Screen.height), new Color(1f, 0.95f, 0.85f, (1f - sinceParry / 0.18f) * 0.16f));
            }
        }

        private void DrawPlayerPanel(float scale)
        {
            float x = 28f * scale;
            float y = 24f * scale;
            float width = 430f * scale;
            float barHeight = 16f * scale;

            Rect panel = new(x - 12f * scale, y - 10f * scale, width + 24f * scale, 112f * scale);
            Fill(panel, Panel);
            Fill(new Rect(panel.x, panel.y, 3f * scale, panel.height), Crimson);

            Label(new Rect(x, y - 2f * scale, width, 22f * scale), "AKIO KUROHANE", titleStyle, Color.white);

            displayedHealth = Mathf.Lerp(displayedHealth, player.Health / player.MaxHealth, Mathf.Clamp01(Time.unscaledDeltaTime * 6f));

            // Health: ghost bar behind the real value.
            Rect healthRect = new(x, y + 26f * scale, width, barHeight);
            Fill(healthRect, new Color(0.05f, 0.01f, 0.015f, 0.95f));
            Fill(new Rect(healthRect.x, healthRect.y, healthRect.width * Mathf.Clamp01(displayedHealth), healthRect.height), CrimsonDark);
            Fill(new Rect(healthRect.x, healthRect.y, healthRect.width * Mathf.Clamp01(player.Health / player.MaxHealth), healthRect.height), Crimson);
            Outline(healthRect, new Color(0.7f, 0.1f, 0.12f, 0.9f), 1.5f);

            // Posture: steel bar that flashes red when nearly broken.
            float postureFraction = Mathf.Clamp01(player.Posture / player.MaxPosture);
            Rect postureRect = new(x, y + 50f * scale, width * 0.72f, barHeight * 0.7f);
            Fill(postureRect, new Color(0.03f, 0.03f, 0.04f, 0.95f));
            Color postureColor = postureFraction < 0.25f
                ? Color.Lerp(Steel, Crimson, Mathf.PingPong(Time.unscaledTime * 6f, 1f))
                : Steel;
            Fill(new Rect(postureRect.x, postureRect.y, postureRect.width * postureFraction, postureRect.height), postureColor);
            Outline(postureRect, SteelDark, 1.5f);

            Label(new Rect(postureRect.xMax + 10f * scale, postureRect.y - 4f * scale, 140f * scale, 20f * scale), "POSTURE", smallStyle, Steel);

            // Dash gauge.
            Rect dashRect = new(x, y + 72f * scale, 120f * scale, 8f * scale);
            Fill(dashRect, new Color(0.03f, 0.03f, 0.04f, 0.95f));
            Fill(new Rect(dashRect.x, dashRect.y, dashRect.width * player.DashReady01, dashRect.height), player.DashReady01 >= 1f ? Color.white : SteelDark);
            Label(new Rect(dashRect.xMax + 10f * scale, dashRect.y - 7f * scale, 160f * scale, 20f * scale), "DASH", smallStyle, Steel);

            if (player.ComboIndex > 0)
            {
                Label(new Rect(x + width - 120f * scale, y - 2f * scale, 120f * scale, 22f * scale), $"COMBO x{player.ComboIndex}", titleStyle, Amber);
            }

            if (player.CounterReady)
            {
                Label(new Rect(x + 180f * scale, y - 2f * scale, 200f * scale, 22f * scale), "COUNTER!", titleStyle, new Color(1f, 0.95f, 0.8f, 1f));
            }
        }

        private void DrawEnemies(float scale)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                return;
            }

            foreach (PrototypeCombatTarget enemy in PrototypeCombatTarget.All)
            {
                if (enemy == null || enemy.IsDead)
                {
                    continue;
                }

                Vector3 screen = cam.WorldToScreenPoint(enemy.transform.position + Vector3.up * enemy.HeadHeight);
                if (screen.z <= 0f || screen.x < -150f || screen.x > Screen.width + 150f)
                {
                    continue;
                }

                float width = 120f * scale;
                float x = screen.x - width * 0.5f;
                float y = Screen.height - screen.y;

                Rect healthRect = new(x, y, width, 7f * scale);
                Fill(healthRect, new Color(0.05f, 0.01f, 0.015f, 0.9f));
                Fill(new Rect(x, y, width * Mathf.Clamp01(enemy.Health / enemy.MaxHealth), healthRect.height), Crimson);
                Outline(healthRect, new Color(0.7f, 0.1f, 0.12f, 0.8f), 1f);

                Rect postureRect = new(x, y + 9f * scale, width, 4f * scale);
                Fill(postureRect, new Color(0.03f, 0.03f, 0.04f, 0.9f));
                Fill(new Rect(x, postureRect.y, width * Mathf.Clamp01(enemy.Posture / enemy.MaxPosture), postureRect.height), enemy.IsPostureBroken ? Amber : Steel);

                Label(new Rect(x - 20f * scale, y - 20f * scale, width + 40f * scale, 20f * scale), enemy.DisplayName, centerStyle, new Color(0.9f, 0.85f, 0.85f, 0.9f));

                if (enemy.TelegraphIsUnblockable)
                {
                    Label(new Rect(x - 20f * scale, y - 46f * scale, width + 40f * scale, 26f * scale), "!! DODGE !!", centerStyle, new Color(1f, 0.15f, 0.1f, 1f));
                }
                else if (enemy.IsTelegraphing)
                {
                    Label(new Rect(x - 20f * scale, y - 46f * scale, width + 40f * scale, 26f * scale), "! PARRY !", centerStyle, Amber);
                }
                else if (enemy.IsPostureBroken)
                {
                    Label(new Rect(x - 20f * scale, y - 46f * scale, width + 40f * scale, 26f * scale), "POSTURE BROKEN", centerStyle, Amber);
                }
            }
        }

        private void DrawCenterMessages(float scale)
        {
            if (player.IsDead)
            {
                float a = Mathf.Clamp01(player.DeathProgress * 1.6f);
                Fill(new Rect(0, 0, Screen.width, Screen.height), new Color(0f, 0f, 0f, a * 0.55f));
                Label(new Rect(0, Screen.height * 0.38f, Screen.width, 90f * scale), "DEFEATED", bigStyle, new Color(0.9f, 0.08f, 0.1f, a));
            }
            else if (Time.unscaledTime - player.LastGuardBreakTime < 0.9f)
            {
                Label(new Rect(0, Screen.height * 0.2f, Screen.width, 40f * scale), "GUARD BROKEN", centerStyle, Amber);
            }
        }

        private void DrawControls(float scale)
        {
            if (!showControls)
            {
                Label(new Rect(28f * scale, Screen.height - 34f * scale, 300f * scale, 22f * scale), "F1  Controls", smallStyle, new Color(0.7f, 0.7f, 0.75f, 0.7f));
                return;
            }

            float fade = Time.unscaledTime - startTime > 25f ? 0.55f : 1f;
            Rect panel = new(28f * scale, Screen.height - 96f * scale, 880f * scale, 72f * scale);
            Fill(panel, new Color(0.02f, 0.01f, 0.015f, 0.6f * fade));

            Color text = new(0.85f, 0.87f, 0.92f, fade);
            Label(new Rect(panel.x + 14f * scale, panel.y + 6f * scale, panel.width, 22f * scale),
                "A/D Move   Space Jump (hold = higher)   Shift Dash (i-frames)   J / LMB Light combo   K / RMB Heavy", smallStyle, text);
            Label(new Rect(panel.x + 14f * scale, panel.y + 26f * scale, panel.width, 22f * scale),
                "Q Parry (time it as the enemy strikes)   L Guard   |   Orange ! = parry   Red !! = dodge, cannot be blocked", smallStyle, text);
            Label(new Rect(panel.x + 14f * scale, panel.y + 46f * scale, panel.width, 22f * scale),
                "A successful parry opens a COUNTER window: your next light attack hits much harder.   F1 hide", smallStyle, new Color(1f, 0.7f, 0.5f, fade));
        }
    }
}
