#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;
using UnityEngine;

public class SocialLoginPostProcess
{

    private const string FacebookAppID = "1208241240711938";
    private const string FacebookDisplayName = "CIdleNinjaVie";

    [PostProcessBuild(45)]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
    {
        if (buildTarget != BuildTarget.iOS)
            return;

        string plistPath = Path.Combine(path, "Info.plist");

        PlistDocument plist = new PlistDocument();
        plist.ReadFromFile(plistPath);

        PlistElementDict rootDict = plist.root;

        // ==== CẤU HÌNH FACEBOOK ====
        rootDict.SetString("FacebookAppID", FacebookAppID);
        rootDict.SetString("FacebookDisplayName", FacebookDisplayName);
        rootDict.SetBoolean("FacebookAutoLogAppEventsEnabled", true);
        rootDict.SetBoolean("FacebookAdvertiserIDCollectionEnabled", true);

        // URL Schemes cho Facebook (fb{AppID})
        var urlTypes = rootDict["CFBundleURLTypes"]?.AsArray() ?? rootDict.CreateArray("CFBundleURLTypes");
        var fbUrlDict = urlTypes.AddDict();
        fbUrlDict.CreateArray("CFBundleURLSchemes").AddString("fb" + FacebookAppID);

        // Thêm LSApplicationQueriesSchemes để Facebook SDK hoạt động đúng
        var queriesSchemes = rootDict["LSApplicationQueriesSchemes"]?.AsArray() ?? rootDict.CreateArray("LSApplicationQueriesSchemes");
        queriesSchemes.AddString("fbapi");
        queriesSchemes.AddString("fb-messenger-share-api");
        queriesSchemes.AddString("fbauth2");
        queriesSchemes.AddString("fbshareextension");

        // ==== CẤU HÌNH GOOGLE SIGN-IN ====
        // Lấy reversed_client_id từ GoogleService-Info.plist
        string googlePlistPath = Path.Combine(Application.dataPath, "GoogleService-Info.plist");
        if (File.Exists(googlePlistPath))
        {
            var googlePlist = new PlistDocument();
            googlePlist.ReadFromFile(googlePlistPath);

            string reversedClientId = googlePlist.root["REVERSED_CLIENT_ID"].AsString();

            // Thêm URL scheme cho Google Sign-In nếu chưa có
            bool hasGoogleURL = false;
            foreach (var el in urlTypes.values)
            {
                var schemes = el.AsDict()["CFBundleURLSchemes"].AsArray();
                foreach (var s in schemes.values)
                {
                    if (s.AsString() == reversedClientId)
                    {
                        hasGoogleURL = true;
                        break;
                    }
                }
            }

            if (!hasGoogleURL)
            {
                var googleUrlDict = urlTypes.AddDict();
                googleUrlDict.CreateArray("CFBundleURLSchemes").AddString(reversedClientId);
            }
        }

        // Ghi lại plist sau khi chỉnh
        plist.WriteToFile(plistPath);

        UnityEngine.Debug.Log("✅ Facebook + Google Info.plist config added automatically!");
    }
}
#endif
