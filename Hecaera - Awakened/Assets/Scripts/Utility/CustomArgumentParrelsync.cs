// This should be editor only
#if UNITY_EDITOR
using UnityEngine;
using ParrelSync;
using System.Net;
using Unity.Services.Authentication;
using Unity.Services.Core;

namespace Erikduss
{
    public class CustomArgumentParrelsync : MonoBehaviour
    {
        public static CustomArgumentParrelsync Instance;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            //ProfileSwitch();
        }

        private void ProfileSwitch()
        {
            //Is this unity editor instance opening a clone project?
            if (ClonesManager.IsClone())
            {
                Debug.Log("This is a clone project.");
                // Get the custom argument for this clone project.  
                string customArgument = ClonesManager.GetArgument();
                // Do what ever you need with the argument string.
                Debug.Log("The custom argument of this clone project is: " + customArgument);

                InitializationOptions options = new InitializationOptions();
                options.SetProfile("Clone_" + customArgument + "_Profile");

                AuthenticationService.Instance.SignInAnonymouslyAsync();
                AuthenticationService.Instance.SwitchProfile("Clone_" + customArgument + "_Profile");
            }
            else
            {
                Debug.Log("This is the original project.");
            }

            Debug.Log(AuthenticationService.Instance.PlayerId);
        }

        public void RetryProfileSwitch()
        {
            //ProfileSwitch();
        }
    }
}
#endif