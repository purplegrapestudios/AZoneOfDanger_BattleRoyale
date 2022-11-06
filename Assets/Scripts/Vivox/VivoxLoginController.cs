using System;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class VivoxLoginController : MonoBehaviour
{
    private VivoxVoiceManager _vivoxVoiceManager;

    [SerializeField] private string m_displayName = "TEST"; //=> GameUI.Lobby.Lobby.Instance.GetPlayerSetup().GetNameInputValue();
    //InputField DisplayNameInput;
    //public GameObject LoginScreen;

    private int defaultMaxStringLength = 9;
    private int PermissionAskedCount = 0;
    #region Unity Callbacks

    private EventSystem _evtSystem;

    private void Awake()
    {
        _evtSystem = FindObjectOfType<EventSystem>();
        _vivoxVoiceManager = VivoxVoiceManager.Instance;
        _vivoxVoiceManager.OnUserLoggedInEvent += OnUserLoggedIn;
        _vivoxVoiceManager.OnUserLoggedOutEvent += OnUserLoggedOut;

#if !(UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_STADIA)
        DisplayNameInput.interactable = false;
#else
        //DisplayNameInput.onEndEdit.AddListener((string text) => { LoginToVivoxService(); });
#endif
        //LoginButton.onClick.AddListener(() => { LoginToVivoxService(); });

        if (_vivoxVoiceManager.LoginState == VivoxUnity.LoginState.LoggedIn)
        {
            OnUserLoggedIn();
            Debug.Log("Logged into VivoxVoice Service");
            m_displayName = _vivoxVoiceManager.LoginSession.Key.DisplayName;
        }
        else
        {
            OnUserLoggedOut();
            var systInfoDeviceName = String.IsNullOrWhiteSpace(SystemInfo.deviceName) == false ? SystemInfo.deviceName : Environment.MachineName;

            m_displayName = Environment.MachineName.Substring(0, Math.Min(defaultMaxStringLength, Environment.MachineName.Length));
        }

        _vivoxVoiceManager.SetBeginLoginCallback(LoginToVivoxService);

        Invoke("LoginToVivoxService", 5);
    }

    private void OnDestroy()
    {
        _vivoxVoiceManager.OnUserLoggedInEvent -= OnUserLoggedIn;
        _vivoxVoiceManager.OnUserLoggedOutEvent -= OnUserLoggedOut;

        //LoginButton.onClick.RemoveAllListeners();
#if UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_STADIA
        //DisplayNameInput.onEndEdit.RemoveAllListeners();
#endif
    }

    #endregion

#if (UNITY_ANDROID && !UNITY_EDITOR) || __ANDROID__
    private bool IsAndroid12AndUp()
    {
        // android12VersionCode is hardcoded because it might not be available in all versions of Android SDK
        const int android12VersionCode = 31;
        AndroidJavaClass buildVersionClass = new AndroidJavaClass("android.os.Build$VERSION");
        int buildSdkVersion = buildVersionClass.GetStatic<int>("SDK_INT");

        return buildSdkVersion >= android12VersionCode;
    }

    private string GetBluetoothConnectPermissionCode()
    {
        if (IsAndroid12AndUp())
        {
            // UnityEngine.Android.Permission does not contain the BLUETOOTH_CONNECT permission, fetch it from Android
            AndroidJavaClass manifestPermissionClass = new AndroidJavaClass("android.Manifest$permission");
            string permissionCode = manifestPermissionClass.GetStatic<string>("BLUETOOTH_CONNECT");

            return permissionCode;
        }

        return "";
    }
#endif

    private bool IsMicPermissionGranted()
    {
        bool isGranted = Permission.HasUserAuthorizedPermission(Permission.Microphone);
#if (UNITY_ANDROID && !UNITY_EDITOR) || __ANDROID__
        if (IsAndroid12AndUp())
        {
            // On Android 12 and up, we also need to ask for the BLUETOOTH_CONNECT permission for all features to work
            isGranted &= Permission.HasUserAuthorizedPermission(GetBluetoothConnectPermissionCode());
        }
#endif
        return isGranted;
    }

    private void AskForPermissions()
    {
        string permissionCode = Permission.Microphone;

#if (UNITY_ANDROID && !UNITY_EDITOR) || __ANDROID__
        if (PermissionAskedCount == 1 && IsAndroid12AndUp())
        {
            permissionCode = GetBluetoothConnectPermissionCode();
        }
#endif
        PermissionAskedCount++;
        Permission.RequestUserPermission(permissionCode);
    }

    private bool IsPermissionsDenied()
    {
#if (UNITY_ANDROID && !UNITY_EDITOR) || __ANDROID__
        // On Android 12 and up, we also need to ask for the BLUETOOTH_CONNECT permission
        if (IsAndroid12AndUp())
        {
            return PermissionAskedCount == 2;
        }
#endif
        return PermissionAskedCount == 1;
    }

    private void LoginToVivoxService()
    {
        if (IsMicPermissionGranted())
        {
            // The user authorized use of the microphone.
            LoginToVivox();
        }
        else
        {
            // We do not have the needed permissions.
            // Ask for permissions or proceed without the functionality enabled if they were denied by the user
            if (IsPermissionsDenied())
            {
                PermissionAskedCount = 0;
                LoginToVivox();
            }
            else
            {
                AskForPermissions();
            }
        }
    }

    private void LoginToVivox()
    {
        //LoginButton.interactable = false;

        if (string.IsNullOrEmpty(m_displayName))
        {
            Debug.LogError("Please enter a display name.");
            return;
        }
        Debug.Log($"Vivox DisplayName: {m_displayName}");
        _vivoxVoiceManager.Login(m_displayName);
    }

    #region Vivox Callbacks

    private void OnUserLoggedIn()
    {
        //Do nothing
    }

    private void OnUserLoggedOut()
    {
        //Do nothing
    }

    #endregion
}