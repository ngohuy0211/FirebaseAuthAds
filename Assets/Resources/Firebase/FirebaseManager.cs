using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Facebook.Unity;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;

public enum LoginType
{
    Google,
    Facebook,
    Apple
}

public class FirebaseManager : Singleton<FirebaseManager>
{
    private string _googleAPI = "638088285968-79r5m560k75ga0s9nm0pbreckurfu073.apps.googleusercontent.com";
    private GoogleSignInConfiguration _configuration;
    private Firebase.Auth.FirebaseAuth _auth;
    private Firebase.Auth.FirebaseUser _user;
    private bool _isGoogleSignInInitialized = false;

    public System.Action<LoginType, string> LoginDoneCb; //Google is authToken, FB is accessToken
    public System.Action LogOutDoneCb;

    public void InitFirebase()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                _auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
                LogOut();
                
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                // Set a flag here to indicate whether Firebase is ready to use by your app.
                
                //Auto Signin
                // _auth.StateChanged += AuthStateChanged;
                // AuthStateChanged(this, null);
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                    "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });

        //
        if (!FB.IsInitialized) FB.Init(InitCallback, OnHideUnity);
        else FB.ActivateApp();
    }

    public void LogOut()
    {
        if (_auth.CurrentUser != null)
        {
            _auth.SignOut();
            _user = null;
            LogOutDoneCb?.Invoke();   
        }
    }
    
    #region Login Google

    public void LoginWithGoogle()
    {
        if (!_isGoogleSignInInitialized)
        {
            GoogleSignIn.Configuration = new GoogleSignInConfiguration
            {
                RequestIdToken = true,
                WebClientId = _googleAPI,
                RequestEmail = true,
                UseGameSignIn = false,
                RequestAuthCode = true,
                ForceTokenRefresh = true
            };

            _isGoogleSignInInitialized = true;
        }

        Task<GoogleSignInUser> signIn = GoogleSignIn.DefaultInstance.SignIn();

        TaskCompletionSource<FirebaseUser> signInCompleted = new TaskCompletionSource<FirebaseUser>();
        signIn.ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                signInCompleted.SetCanceled();
                Debug.Log("Cancelled");
            }
            else if (task.IsFaulted)
            {
                signInCompleted.SetException(task.Exception);
                
                ShowErrorMessage(task);
                
                Debug.Log("Faulted " + task.Exception);
            }
            else
            {
                Credential credential =
                    Firebase.Auth.GoogleAuthProvider.GetCredential(((Task<GoogleSignInUser>) task).Result.IdToken,
                        null);
                _auth.SignInWithCredentialAsync(credential).ContinueWith(authTask =>
                {
                    if (authTask.IsCanceled)
                    {
                        signInCompleted.SetCanceled();
                    }
                    else if (authTask.IsFaulted)
                    {
                        signInCompleted.SetException(authTask.Exception);
                        Debug.Log("Faulted In Auth " + task.Exception);
                    }
                    else
                    {
                        signInCompleted.SetResult(((Task<FirebaseUser>) authTask).Result);
                        _user = _auth.CurrentUser;
                        Debug.LogFormat("User signed in successfully: {0} ({1})",
                            _user.DisplayName, _user.UserId);
                        string authCode = task.Result.AuthCode;
                        LoginDoneCb?.Invoke(LoginType.Google, authCode);
                    }
                });
            }
        });
    }

    #endregion

    #region Login Facebook

    public void LoginWithFb()
    {
        Debug.Log("Login Facebook Called");
        var perms = new List<string>() {"public_profile", "email"};
        FB.LogInWithReadPermissions(perms, AuthCallback);
    }

    private void AuthCallback(ILoginResult result)
    {
        Debug.Log(FB.IsLoggedIn);
        if (FB.IsLoggedIn)
        {
            // AccessToken class will have session details
            var aToken = Facebook.Unity.AccessToken.CurrentAccessToken.TokenString;
            FacebookAuth(aToken);
        }
        else
        {
            Debug.Log("User cancelled login");
        }
    }

    private void FacebookAuth(string accessToken)
    {
        Firebase.Auth.Credential credential =
            Firebase.Auth.FacebookAuthProvider.GetCredential(accessToken);
        _auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInAndRetrieveDataWithCredentialAsync was canceled.");
                return;
            }

            if (task.IsFaulted)
            {
                Debug.LogError("SignInAndRetrieveDataWithCredentialAsync encountered an error: " + task.Exception);
                ShowErrorMessage(task);
                
                return;
            }

            _user = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                _user.DisplayName, _user.UserId);

            LoginDoneCb?.Invoke(LoginType.Facebook, accessToken);
        });
    }

    private void InitCallback()
    {
        if (FB.IsInitialized)
        {
            // Signal an app activation App Event
            FB.ActivateApp();
            // Continue with Facebook SDK
            // ...
        }
        else
        {
            Debug.Log("Failed to Initialize the Facebook SDK");
        }
    }

    private void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown)
        {
            // Pause the game - we will need to hide
            Time.timeScale = 0;
        }
        else
        {
            // Resume the game - we're getting focus again
            Time.timeScale = 1;
        }
    }

    #endregion

    #region Login Apple

    public void LoginWithApple()
    {
        var credential = OAuthProvider.GetCredential("apple.com", 
            idToken: "",
            rawNonce: "", 
            accessToken: "");

        _auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Apple Sign-in failed: " + task.Exception);
                return;
            }

            _user = task.Result;
            Debug.Log("Signed in with Apple. UID: " + _user.UserId);

            _user.TokenAsync(true).ContinueWith(tokenTask =>
            {
                if (tokenTask.IsCanceled || tokenTask.IsFaulted)
                {
                    Debug.LogError("Get token failed: " + tokenTask.Exception);
                    return;
                }

                string idToken = tokenTask.Result;
                Debug.Log("Firebase idToken: " + idToken);
                LoginDoneCb?.Invoke(LoginType.Apple, idToken);
            });
        });
    }

    #endregion

    #region Action

    private void UpdateUserProfile(string userName)
    {
        Firebase.Auth.FirebaseUser user = _auth.CurrentUser;
        if (user != null)
        {
            Firebase.Auth.UserProfile profile = new Firebase.Auth.UserProfile
            {
                DisplayName = userName,
                PhotoUrl = new System.Uri("https://dummyimage.com/300"),
            };
            user.UpdateUserProfileAsync(profile).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("UpdateUserProfileAsync was canceled.");
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError("UpdateUserProfileAsync encountered an error: " + task.Exception);
                    return;
                }
            });
        }
    }


    private void ShowErrorMessage(Task task)
    {
        string strErr = "";
        foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
        {
            if (exception is not FirebaseException firebaseEx) return;
            var errorCode = (AuthError)firebaseEx.ErrorCode;
            strErr += GetErrorMessage(errorCode);
        }
        Debug.LogError(strErr);
    }
    private string GetErrorMessage(AuthError errorCode)
    {
        var message = errorCode switch
        {
            AuthError.AccountExistsWithDifferentCredentials => "The account already exists with different credentials",
            AuthError.MissingPassword => "Password is missing",
            AuthError.WeakPassword => "The password is weak",
            AuthError.WrongPassword => "The password is wrong",
            AuthError.EmailAlreadyInUse => "The account with that email already exists",
            AuthError.InvalidEmail => "Invalid email",
            AuthError.MissingEmail => "Email is required",
            _ => "An error occurred. Please recheck email or password"
        };
        return message;
    }

    #endregion
}