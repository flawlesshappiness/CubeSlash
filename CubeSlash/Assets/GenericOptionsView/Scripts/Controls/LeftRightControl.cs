using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Flawliz.GenericOptions
{
    public class LeftRightControl : SelectableControl
    {
        [SerializeField] protected PointerControl _btn_left, _btn_right;
        [SerializeField] protected TMP_Text _tmp_value;
        [SerializeField] protected CanvasGroup _cvg_left, _cvg_right;

        public bool can_hold;

        public event Action OnLeft, OnRight;

        private bool _is_right_down, _is_left_down;

        private readonly float[] _intervals = new float[2] { 0.5f, 0.1f };
        private readonly float _interval_next = 0.5f;

        private float _time_next_interval;
        private float _time_next_update;
        private int _idx_interval;

        protected override void Awake()
        {
            base.Awake();

            if (!Application.isPlaying) return;

            Hide();
        }

        protected override void Start()
        {
            base.Start();

            if (!Application.isPlaying) return;

            _btn_left.OnPointerDownEvent += OnLeftDown;
            _btn_left.OnPointerUpEvent += OnLeftUp;

            _btn_right.OnPointerDownEvent += OnRightDown;
            _btn_right.OnPointerUpEvent += OnRightUp;
        }

        private void Update()
        {
            if (!can_hold) return;
            if (!_is_left_down && !_is_right_down) return;
            if (Time.unscaledTime < _time_next_update) return;
            if (Time.unscaledTime > _time_next_interval)
            {
                _time_next_interval = Time.unscaledTime + _interval_next;
                _idx_interval = Mathf.Clamp(_idx_interval + 1, 0, _intervals.Length - 1);
            }

            _time_next_update = Time.unscaledTime + _intervals[_idx_interval];

            UpdateLeft();
            UpdateRight();
        }

        private void ResetInterval()
        {
            _idx_interval = 0;
            _time_next_update = Time.unscaledTime + _intervals[_idx_interval];
            _time_next_interval = Time.unscaledTime + _interval_next;
        }

        private void UpdateLeft()
        {
            if (!_is_left_down) return;
            Left();
        }

        private void UpdateRight()
        {
            if (!_is_right_down) return;
            Right();
        }

        protected virtual void Left()
        {
            OnLeft?.Invoke();
        }

        protected virtual void Right()
        {
            OnRight?.Invoke();
        }

        private void OnRightUp(PointerEventData obj)
        {
            _is_right_down = false;
            ResetInterval();
        }

        private void OnRightDown(PointerEventData obj)
        {
            Right();
            _is_right_down = true;
            _is_left_down = false;
            ResetInterval();
        }

        private void OnLeftUp(PointerEventData obj)
        {
            _is_left_down = false;
            ResetInterval();
        }

        private void OnLeftDown(PointerEventData obj)
        {
            Left();
            _is_left_down = true;
            _is_right_down = false;
            ResetInterval();
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            Show();
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            Hide();
        }

        public override void OnMove(AxisEventData eventData)
        {
            if (eventData.moveDir == MoveDirection.Left)
            {
                Left();
            }
            else if (eventData.moveDir == MoveDirection.Right)
            {
                Right();
            }
            else
            {
                base.OnMove(eventData);
                OnLeftUp(null);
                OnRightUp(null);
            }
        }

        private void Show()
        {
            _cvg_left.alpha = 1;
            _cvg_right.alpha = 1;
        }

        private void Hide()
        {
            _cvg_left.alpha = 0;
            _cvg_right.alpha = 0;
        }

        public void SetText(string text)
        {
            _tmp_value.text = text;
        }
    }
}