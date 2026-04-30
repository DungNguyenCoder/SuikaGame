using System;
using Cysharp.Threading.Tasks;
using Facebook.Unity;
using UnityEngine;

namespace Development
{
    public class FacebookAuth : MonoBehaviour
    {
        private Firebase.Auth.FirebaseAuth _auth;
        private UniTaskCompletionSource _facebookInitCompletionSource;
        private UniTaskCompletionSource<ILoginResult> _facebookLoginCompletionSource;
        private UniTaskCompletionSource<string> _facebookProfileNameCompletionSource;

        public bool IsLoggedIn => FB.IsInitialized && FB.IsLoggedIn;

        private void Awake()
        {
            EnsureFirebaseAuth();
        }

        public async UniTask<(bool success, string userName)> TryGetFacebookUserNameAsync()
        {
            try
            {
                await EnsureFacebookInitializedAsync();
                ILoginResult loginResult = await RequestFacebookLoginAsync();
                if (!TryGetAccessToken(loginResult, out string accessToken))
                {
                    return (false, string.Empty);
                }

                string displayName = await ResolveDisplayNameAsync(accessToken);
                return (true, displayName);
            }
            catch (Exception exception)
            {
                Debug.LogError("Facebook login failed: " + exception);
                return (false, string.Empty);
            }
        }

        public void Logout()
        {
            EnsureFirebaseAuth();
            if (IsLoggedIn)
            {
                FB.LogOut();
            }

            _auth.SignOut();
            Debug.Log("User logged out from Facebook.");
        }

        private void EnsureFirebaseAuth()
        {
            if (_auth == null)
            {
                _auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
            }
        }

        private UniTask EnsureFacebookInitializedAsync()
        {
            if (FB.IsInitialized)
            {
                FB.ActivateApp();
                return UniTask.CompletedTask;
            }

            if (_facebookInitCompletionSource != null)
            {
                return _facebookInitCompletionSource.Task;
            }

            _facebookInitCompletionSource = new UniTaskCompletionSource();
            FB.Init(InitCallback, OnHideUnity);
            return _facebookInitCompletionSource.Task;
        }

        private void InitCallback()
        {
            if (!FB.IsInitialized)
            {
                _facebookInitCompletionSource.TrySetException(
                    new InvalidOperationException("Failed to initialize the Facebook SDK."));
                _facebookInitCompletionSource = null;
                return;
            }

            FB.ActivateApp();
            _facebookInitCompletionSource.TrySetResult();
            _facebookInitCompletionSource = null;
        }

        private void OnHideUnity(bool isGameShown)
        {
            Time.timeScale = isGameShown ? 1 : 0;
        }

        private UniTask<ILoginResult> RequestFacebookLoginAsync()
        {
            Debug.Log("Facebook login started.");
            if (_facebookLoginCompletionSource != null)
            {
                return _facebookLoginCompletionSource.Task;
            }

            _facebookLoginCompletionSource = new UniTaskCompletionSource<ILoginResult>();
            FB.LogInWithReadPermissions(new[] { "public_profile" }, HandleFacebookLogin);
            return _facebookLoginCompletionSource.Task;
        }

        private void HandleFacebookLogin(ILoginResult result)
        {
            if (_facebookLoginCompletionSource == null)
            {
                Debug.LogError("Facebook login callback received without a pending request.");
                return;
            }

            UniTaskCompletionSource<ILoginResult> completionSource = _facebookLoginCompletionSource;
            _facebookLoginCompletionSource = null;

            if (result == null)
            {
                completionSource.TrySetException(new InvalidOperationException("Facebook login returned no result."));
                return;
            }

            completionSource.TrySetResult(result);
        }

        private bool TryGetAccessToken(ILoginResult result, out string accessToken)
        {
            accessToken = string.Empty;
            if (!string.IsNullOrEmpty(result.Error))
            {
                Debug.LogError("Facebook login error: " + result.Error);
                return false;
            }

            if (result.Cancelled || !FB.IsLoggedIn)
            {
                Debug.Log("User cancelled Facebook login.");
                return false;
            }

            AccessToken currentAccessToken = AccessToken.CurrentAccessToken;
            if (currentAccessToken == null || string.IsNullOrEmpty(currentAccessToken.TokenString))
            {
                Debug.LogError("Facebook access token is missing.");
                return false;
            }

            accessToken = currentAccessToken.TokenString;
            return true;
        }

        private async UniTask<string> ResolveDisplayNameAsync(string accessToken)
        {
            Firebase.Auth.Credential credential = Firebase.Auth.FacebookAuthProvider.GetCredential(accessToken);
            try
            {
                Firebase.Auth.AuthResult result = await _auth.SignInAndRetrieveDataWithCredentialAsync(credential);
                await UniTask.SwitchToMainThread();

                Debug.Log("User signed in successfully with Facebook.");

                if (!string.IsNullOrWhiteSpace(result.User.DisplayName))
                {
                    return result.User.DisplayName;
                }
            }
            catch (Exception exception)
            {
                await UniTask.SwitchToMainThread();
                Debug.LogError("Firebase Facebook sign-in failed, falling back to Facebook profile name: " + exception);
            }

            return await RequestFacebookProfileNameAsync();
        }

        private UniTask<string> RequestFacebookProfileNameAsync()
        {
            if (_facebookProfileNameCompletionSource != null)
            {
                return _facebookProfileNameCompletionSource.Task;
            }

            _facebookProfileNameCompletionSource = new UniTaskCompletionSource<string>();
            FB.API("/me?fields=name", HttpMethod.GET, HandleFacebookProfileName);
            return _facebookProfileNameCompletionSource.Task;
        }

        private void HandleFacebookProfileName(IResult result)
        {
            if (_facebookProfileNameCompletionSource == null)
            {
                Debug.LogError("Facebook profile callback received without a pending request.");
                return;
            }

            UniTaskCompletionSource<string> completionSource = _facebookProfileNameCompletionSource;
            _facebookProfileNameCompletionSource = null;

            if (result == null)
            {
                completionSource.TrySetException(new InvalidOperationException("Facebook profile request returned no result."));
                return;
            }

            if (!string.IsNullOrEmpty(result.Error))
            {
                completionSource.TrySetException(new InvalidOperationException("Facebook profile request error: " + result.Error));
                return;
            }

            string displayName = string.Empty;
            if (result.ResultDictionary != null &&
                result.ResultDictionary.TryGetValue("name", out object nameValue))
            {
                displayName = nameValue is string name ? name : string.Empty;
            }

            completionSource.TrySetResult(displayName);
        }
    }
}
