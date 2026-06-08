using TopDownShooter;
using UnityEngine;

namespace LegendOfZed.Player
{
    [DisallowMultipleComponent]
    public class ZedPlayerHealth : MonoBehaviour
    {
        [Header("Health")]
        public float MaxHealth = 100f;
        public float CurrentHealth = 100f;
        public bool IsDead;

        [Header("Damage Rules")]
        public float DamageInvulnerabilitySeconds = 0.65f;
        public bool LogDamageForTesting = true;

        [Header("Audio")]
        public ZedPlayerAudioBridge AudioBridge;
        public bool PlayHurtAudio = true;

        [Header("Death")]
        public bool DisableMovementOnDeath = true;
        public bool DisableShootingOnDeath = true;
        public string DeathMessage = "PLAYER DOWN";

        [Header("Debug HUD")]
        public bool ShowDebugHud = true;
        public bool ShowDamageFlash = true;
        public float DamageFlashSeconds = 0.22f;

        private float _nextDamageAllowedTime;
        private float _damageFlashUntilTime;
        private MovementCharacterController _movement;
        private ShooterController _shooter;

        private void Awake()
        {
            _movement = GetComponent<MovementCharacterController>();
            _shooter = GetComponent<ShooterController>();
            if (AudioBridge == null)
            {
                AudioBridge = GetComponent<ZedPlayerAudioBridge>();
            }

            if (CurrentHealth <= 0f)
            {
                CurrentHealth = MaxHealth;
            }

            IsDead = CurrentHealth <= 0f;
        }

        private void OnEnable()
        {
            if (CurrentHealth <= 0f)
            {
                CurrentHealth = MaxHealth;
            }

            IsDead = false;

            if (_movement == null)
            {
                _movement = GetComponent<MovementCharacterController>();
            }

            if (_shooter == null)
            {
                _shooter = GetComponent<ShooterController>();
            }

            if (AudioBridge == null)
            {
                AudioBridge = GetComponent<ZedPlayerAudioBridge>();
            }
        }

        public void ApplyDamage(float amount)
        {
            TakeDamage(amount);
        }

        public void Damage(float amount)
        {
            TakeDamage(amount);
        }

        public void TakeDamage(float amount)
        {
            if (IsDead || amount <= 0f)
            {
                return;
            }

            if (Time.time < _nextDamageAllowedTime)
            {
                return;
            }

            _nextDamageAllowedTime = Time.time + DamageInvulnerabilitySeconds;
            _damageFlashUntilTime = Time.time + DamageFlashSeconds;

            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);

            if (PlayHurtAudio && AudioBridge != null)
            {
                AudioBridge.PlayHurtAudio();
            }

            if (LogDamageForTesting)
            {
                Debug.Log("Player took " + amount + " damage. Health=" + CurrentHealth + "/" + MaxHealth, this);
            }

            if (CurrentHealth <= 0f)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (amount <= 0f || IsDead)
            {
                return;
            }

            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        }

        public void ResetHealth()
        {
            CurrentHealth = MaxHealth;
            IsDead = false;
            _nextDamageAllowedTime = 0f;
            _damageFlashUntilTime = 0f;

            if (_movement != null)
            {
                _movement.enabled = true;
            }

            if (_shooter != null)
            {
                _shooter.enabled = true;
            }
        }

        private void Die()
        {
            if (IsDead)
            {
                return;
            }

            IsDead = true;
            CurrentHealth = 0f;

            if (DisableMovementOnDeath && _movement != null)
            {
                _movement.enabled = false;
            }

            if (DisableShootingOnDeath && _shooter != null)
            {
                _shooter.enabled = false;
            }

            Debug.Log(DeathMessage, this);
            SendMessage("OnZedPlayerDied", SendMessageOptions.DontRequireReceiver);
        }

        private void OnGUI()
        {
            if (!ShowDebugHud)
            {
                return;
            }

            DrawHealthHud();

            if (ShowDamageFlash && Time.time < _damageFlashUntilTime)
            {
                DrawDamageFlash();
            }

            if (IsDead)
            {
                DrawDeathMessage();
            }
        }

        private void DrawHealthHud()
        {
            const float x = 20f;
            const float y = 20f;
            const float width = 220f;
            const float height = 22f;

            float percent = MaxHealth > 0f ? Mathf.Clamp01(CurrentHealth / MaxHealth) : 0f;

            GUI.Box(new Rect(x, y, width, height), string.Empty);
            GUI.Box(new Rect(x + 2f, y + 2f, (width - 4f) * percent, height - 4f), string.Empty);
            GUI.Label(new Rect(x + 8f, y + 2f, width, height), "Health: " + Mathf.CeilToInt(CurrentHealth) + " / " + Mathf.CeilToInt(MaxHealth));
        }

        private static void DrawDamageFlash()
        {
            Color oldColor = GUI.color;
            GUI.color = new Color(1f, 0f, 0f, 0.18f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = oldColor;
        }

        private void DrawDeathMessage()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 36;
            style.fontStyle = FontStyle.Bold;

            Rect rect = new Rect(0f, Screen.height * 0.4f, Screen.width, 80f);
            GUI.Label(rect, DeathMessage, style);
        }
    }
}
