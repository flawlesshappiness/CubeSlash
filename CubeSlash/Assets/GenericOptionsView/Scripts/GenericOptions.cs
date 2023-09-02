using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Flawliz.GenericOptions
{
    public class GenericOptions : MonoBehaviour
    {
        [SerializeField] private CategoryControl _active_category_control;
        [SerializeField] private ButtonControl _btn_back, _btn_restore_defaults, _btn_apply;
        [SerializeField] private ConfirmWindow _confirm_window;
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
            _confirm_window ??= GetComponentInChildren<ConfirmWindow>();
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

            _confirm_window.OnShow += () => SetApplyWindowEnabled(true);
            _confirm_window.OnHide += () => SetApplyWindowEnabled(false);
            _confirm_window.Hide();

            _btn_apply.SetInteractable(false);

            EventSystem.current.SetSelectedGameObject(_active_category_control.gameObject);
        }

        private void ClickApply()
        {
            _confirm_window.TitleText = "Apply changes?";
            _confirm_window.ConfirmText = "Yes";
            _confirm_window.CancelText = "No";
            _confirm_window.Show(OnApply, null);
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
            _confirm_window.TitleText = "Restore defaults?";
            _confirm_window.ConfirmText = "Yes";
            _confirm_window.CancelText = "No";
            _confirm_window.Show(OnRestoreDefaults, null);
        }

        public void ShowConfirmWindow(string title, string confirmText, string cancelText, System.Action onConfirm, System.Action onCancel)
        {
            _confirm_window.TitleText = title;
            _confirm_window.ConfirmText = confirmText;
            _confirm_window.CancelText = cancelText;
            _confirm_window.Show(onConfirm, onCancel);
        }

        private void OnRestoreDefaults()
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