namespace StudyGame.Combat
{
    public sealed class DashTimer
    {
        readonly float _duration;
        readonly float _invulnerableDuration;
        readonly float _cooldown;
        float _elapsed;
        bool _active;
        float _cooldownRemaining;

        public DashTimer(float duration, float invulnerableDuration, float cooldown)
        {
            _duration = duration;
            _invulnerableDuration = invulnerableDuration;
            _cooldown = cooldown;
        }

        public bool IsDashing => _active;
        public bool IsInvulnerable => _active && _elapsed < _invulnerableDuration;
        public bool CanDash => !_active && _cooldownRemaining <= 0f;

        public bool TryStart()
        {
            if (!CanDash) return false;
            _active = true;
            _elapsed = 0f;
            return true;
        }

        public float Tick(float deltaTime)
        {
            if (!_active)
            {
                if (_cooldownRemaining > 0f) _cooldownRemaining -= deltaTime;
                return 0f;
            }
            if (_elapsed + deltaTime < _duration)
            {
                _elapsed += deltaTime;
                return deltaTime;
            }
            float remaining = _duration - _elapsed;
            _elapsed = _duration;
            _active = false;
            _cooldownRemaining = _cooldown - (deltaTime - remaining);
            return remaining;
        }
    }
}
