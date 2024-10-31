using System;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace GarawellCase
{
    public class InputController : MonoSingleton<InputController>
    {
        public event Action<Vector2> Pressed;
        public event Action<Vector2> Dragged;
        public event Action<Vector2> Released;
        
        private InputActions _inputActions;

        /*private float lastElapsedTime;
        private float elapsedTime;

        private bool _inputCheck = false;
        private bool _input = false;*/

        #region Unity Methods
        
        protected override void Awake()
        {
            base.Awake();
            _inputActions = new InputActions();
        }

        private void Start()
        {
            _inputActions.Touch.TouchPress.started += OnTouchPressed;
            _inputActions.Touch.TouchDrag.started += OnTouchDrag;
            _inputActions.Touch.TouchRelease.performed += OnTouchRelease;
        }

        /*private void Update()
        {
            if (!_inputCheck)
            {
                elapsedTime = 0;
                return;
            }
            Debug.Log($"Delta: {Time.deltaTime}");
            elapsedTime += Time.deltaTime;
            if (_input)
            {
                _input = false;
                Debug.Log($"Elapsed: {elapsedTime}");
                elapsedTime = 0;
            }
            
        }*/
        
        private void OnDestroy()
        {
            if(_inputActions == null)
                return;
            
            _inputActions.Touch.TouchPress.started -= OnTouchPressed;
            _inputActions.Touch.TouchDrag.started -= OnTouchDrag;
            _inputActions.Touch.TouchRelease.performed -= OnTouchRelease;
            _inputActions.Dispose();
        }

        private void OnEnable()
        {
            _inputActions.Enable();
        }
        

        private void OnDisable()
        {
            _inputActions.Disable();
        }

        #endregion
        
        #region Input Listeners

        private void OnTouchPressed(InputAction.CallbackContext context)
        {
            //_inputCheck = true;
            var value = _inputActions.Touch.TouchPosition.ReadValue<Vector2>();
            //Debug.Log($"Touch Pressed: {value}");
            Pressed?.Invoke(value);
        }
        
        private void OnTouchDrag(InputAction.CallbackContext context)
        {
            //_input = true;
            var value = _inputActions.Touch.TouchPosition.ReadValue<Vector2>();
            //Debug.Log($"Touch Dragged: {value}");
            Dragged?.Invoke(value);
        }
        
        private void OnTouchRelease(InputAction.CallbackContext context)
        {
            //_inputCheck = false;
            var value = _inputActions.Touch.TouchPosition.ReadValue<Vector2>();
            //Debug.Log($"Touch Released: {value}");
            Released?.Invoke(value);
        }

        #endregion

    }
}


