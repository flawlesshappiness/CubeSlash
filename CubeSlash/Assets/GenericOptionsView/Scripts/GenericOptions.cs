using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Flawliz.GenericOptions
{
    public class GenericOptions : MonoBehaviour
    {
        [SerializeField] private CategoryControl _active_category_control;
        [SerializeField] private ButtonControl _btn_back, _btn_restore_defaults, _btn_apply;
        [SerializeField] private ApplyWindow _apply_window;
        [SerializeField] private CanvasGroup _cvg_options_window;
        [SerializeField] private GameObject _options_foreground;

        public event System.Action OnBack;
        public event System.Action<OptionsData> OnDataChanged;

        public bool HasChanges { get; private set; }

        public OptionsData Data { get; private set; } = new OptionsData();

        private bool _clicked_back;

        private void OnValidate()
        {
            _active_category_control ??= GetComponentInChildren<CategoryControl>();
            _apply_window ??= GetComponentInChildren<ApplyWindow>();
        }

        private void Awake()
        {
            LoadDataFromPlayerPrefs();
        }

        private void Start()
        {
            _active_category_control.Submit();

            _btn_back.OnSubmitEvent += ClickBack;
            _btn_restore_defaults.OnSubmitEvent += ClickRestoreDefaults;
            _btn_apply.OnSubmitEvent += ClickApply;

            _apply_window.OnApply += OnApply;
            _apply_window.OnShow += () => SetApplyWindowEnabled(true);
            _apply_window.OnHide += () => SetApplyWindowEnabled(false);
            _apply_window.Hide();

            _btn_apply.SetInteractable(false);

            EventSystem.current.SetSelectedGameObject(_active_category_control.gameObject);
        }

        private void ClickApply()
        {
            _apply_window.Show();
        }

        public void ClickBack()
        {
            if (HasChanges)
            {
                ClickApply();
                _clicked_back = true;
            }
            else
            {
                OnBack?.Invoke();
            }
        }

        public void ClickRestoreDefaults()
        {
            var category = CategoryControl.ActiveCategory;
            var controls = category.Content.GetComponentsInChildren<GenericOptionsHandler>();
            foreach (var control in controls)
            {
                control.RestoreDefault();
            }
        }

        private void OnApply()
        {
            SaveDataToPlayerPrefs();

            var controls = GetComponentsInChildren<GenericOptionsHandler>();
            foreach (var control in controls)
            {
                control.Apply();
            }

            HasChanges = false;
            _btn_apply.SetInteractable(false);
        }

        private void SetApplyWindowEnabled(bool enabled)
        {
            _cvg_options_window.interactable = !enabled;
            _cvg_options_window.blocksRaycasts = !enabled;

            _options_foreground.SetActive(enabled);

            if (!enabled)
            {
                EventSystem.current.SetSelectedGameObject(HasChanges ? _btn_apply.gameObject : _btn_back.gameObject);

                if (_clicked_back)
                {
                    _clicked_back = false;
                    HasChanges = false;
                    ClickBack();
                }
            }
        }

        public void SetHasChanges()
        {
            HasChanges = true;
            _btn_apply.SetInteractable(true);
        }

        public void SetData(OptionsData data)
        {
            Data = data ?? new OptionsData();
            OnDataChanged?.Invoke(data);
        }

        public void SaveDataToPlayerPrefs()
        {
            var json = JsonConvert.SerializeObject(Data);
            PlayerPrefs.SetString(typeof(OptionsData).AssemblyQualifiedName, json);
        }

        public void LoadDataFromPlayerPrefs()
        {
            var data = GetDataFromPlayerPrefs();
            SetData(data);
        }

        public static OptionsData GetDataFromPlayerPrefs()
        {
            var json = PlayerPrefs.GetString(typeof(OptionsData).AssemblyQualifiedName);
            var data = JsonConvert.DeserializeObject<OptionsData>(json);
            return data ?? new OptionsData();
        }
    }
}