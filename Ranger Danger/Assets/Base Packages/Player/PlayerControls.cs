// GENERATED AUTOMATICALLY FROM 'Assets/Base Packages/Player/PlayerControls.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @PlayerControls : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayerControls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerControls"",
    ""maps"": [
        {
            ""name"": ""Normal Movement"",
            ""id"": ""eafcfb38-55a2-4e97-8430-5e63fab9d290"",
            ""actions"": [
                {
                    ""name"": ""LeftRight"",
                    ""type"": ""PassThrough"",
                    ""id"": ""02e8fa2f-8c66-40a3-8fcd-8dfbccdf9bf3"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""UpDown"",
                    ""type"": ""Button"",
                    ""id"": ""b868d5e2-ed81-45d2-aeab-6b8450e27bf1"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Sprint"",
                    ""type"": ""Button"",
                    ""id"": ""b044234d-4d7c-4ae0-9fdb-c01866d6b1c5"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""LeftRight"",
                    ""id"": ""dcac4115-ed47-4a77-86d4-80547876615f"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""LeftRight"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""d075d386-9531-4ee8-a099-705c1ff89821"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""LeftRight"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""6fa1f181-015d-4d9d-9b4c-b7c0c1f23249"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""LeftRight"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""UpDown"",
                    ""id"": ""bf9c9bac-15e2-4cd2-95cf-0c9bf15f59ab"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""UpDown"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""c0ec5407-cdc0-4a12-aad0-d891ff42dc06"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""UpDown"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""ceace089-912a-4dbe-a392-1bd0e29fc729"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""UpDown"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""05dd4fff-e3ff-4c3f-aa8d-e6a83a4eb718"",
                    ""path"": ""<Keyboard>/leftShift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Sprint"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""Combat"",
            ""id"": ""6899c78a-4c8b-441e-aaa2-ff1389100d25"",
            ""actions"": [
                {
                    ""name"": ""Aim"",
                    ""type"": ""Value"",
                    ""id"": ""f85d5036-5622-4547-817c-766dae07d1ea"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""dd6183fd-fc8a-4736-86bd-b96f8a6da028"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Aim"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // Normal Movement
        m_NormalMovement = asset.FindActionMap("Normal Movement", throwIfNotFound: true);
        m_NormalMovement_LeftRight = m_NormalMovement.FindAction("LeftRight", throwIfNotFound: true);
        m_NormalMovement_UpDown = m_NormalMovement.FindAction("UpDown", throwIfNotFound: true);
        m_NormalMovement_Sprint = m_NormalMovement.FindAction("Sprint", throwIfNotFound: true);
        // Combat
        m_Combat = asset.FindActionMap("Combat", throwIfNotFound: true);
        m_Combat_Aim = m_Combat.FindAction("Aim", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // Normal Movement
    private readonly InputActionMap m_NormalMovement;
    private INormalMovementActions m_NormalMovementActionsCallbackInterface;
    private readonly InputAction m_NormalMovement_LeftRight;
    private readonly InputAction m_NormalMovement_UpDown;
    private readonly InputAction m_NormalMovement_Sprint;
    public struct NormalMovementActions
    {
        private @PlayerControls m_Wrapper;
        public NormalMovementActions(@PlayerControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @LeftRight => m_Wrapper.m_NormalMovement_LeftRight;
        public InputAction @UpDown => m_Wrapper.m_NormalMovement_UpDown;
        public InputAction @Sprint => m_Wrapper.m_NormalMovement_Sprint;
        public InputActionMap Get() { return m_Wrapper.m_NormalMovement; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(NormalMovementActions set) { return set.Get(); }
        public void SetCallbacks(INormalMovementActions instance)
        {
            if (m_Wrapper.m_NormalMovementActionsCallbackInterface != null)
            {
                @LeftRight.started -= m_Wrapper.m_NormalMovementActionsCallbackInterface.OnLeftRight;
                @LeftRight.performed -= m_Wrapper.m_NormalMovementActionsCallbackInterface.OnLeftRight;
                @LeftRight.canceled -= m_Wrapper.m_NormalMovementActionsCallbackInterface.OnLeftRight;
                @UpDown.started -= m_Wrapper.m_NormalMovementActionsCallbackInterface.OnUpDown;
                @UpDown.performed -= m_Wrapper.m_NormalMovementActionsCallbackInterface.OnUpDown;
                @UpDown.canceled -= m_Wrapper.m_NormalMovementActionsCallbackInterface.OnUpDown;
                @Sprint.started -= m_Wrapper.m_NormalMovementActionsCallbackInterface.OnSprint;
                @Sprint.performed -= m_Wrapper.m_NormalMovementActionsCallbackInterface.OnSprint;
                @Sprint.canceled -= m_Wrapper.m_NormalMovementActionsCallbackInterface.OnSprint;
            }
            m_Wrapper.m_NormalMovementActionsCallbackInterface = instance;
            if (instance != null)
            {
                @LeftRight.started += instance.OnLeftRight;
                @LeftRight.performed += instance.OnLeftRight;
                @LeftRight.canceled += instance.OnLeftRight;
                @UpDown.started += instance.OnUpDown;
                @UpDown.performed += instance.OnUpDown;
                @UpDown.canceled += instance.OnUpDown;
                @Sprint.started += instance.OnSprint;
                @Sprint.performed += instance.OnSprint;
                @Sprint.canceled += instance.OnSprint;
            }
        }
    }
    public NormalMovementActions @NormalMovement => new NormalMovementActions(this);

    // Combat
    private readonly InputActionMap m_Combat;
    private ICombatActions m_CombatActionsCallbackInterface;
    private readonly InputAction m_Combat_Aim;
    public struct CombatActions
    {
        private @PlayerControls m_Wrapper;
        public CombatActions(@PlayerControls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Aim => m_Wrapper.m_Combat_Aim;
        public InputActionMap Get() { return m_Wrapper.m_Combat; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(CombatActions set) { return set.Get(); }
        public void SetCallbacks(ICombatActions instance)
        {
            if (m_Wrapper.m_CombatActionsCallbackInterface != null)
            {
                @Aim.started -= m_Wrapper.m_CombatActionsCallbackInterface.OnAim;
                @Aim.performed -= m_Wrapper.m_CombatActionsCallbackInterface.OnAim;
                @Aim.canceled -= m_Wrapper.m_CombatActionsCallbackInterface.OnAim;
            }
            m_Wrapper.m_CombatActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Aim.started += instance.OnAim;
                @Aim.performed += instance.OnAim;
                @Aim.canceled += instance.OnAim;
            }
        }
    }
    public CombatActions @Combat => new CombatActions(this);
    public interface INormalMovementActions
    {
        void OnLeftRight(InputAction.CallbackContext context);
        void OnUpDown(InputAction.CallbackContext context);
        void OnSprint(InputAction.CallbackContext context);
    }
    public interface ICombatActions
    {
        void OnAim(InputAction.CallbackContext context);
    }
}
