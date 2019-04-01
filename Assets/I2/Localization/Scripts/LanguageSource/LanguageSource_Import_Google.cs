﻿using UnityEngine;
using System;
using System.Collections;

namespace I2.Loc
{
	public partial class LanguageSource
	{
		#region Variables

		public string Google_WebServiceURL;
		public string Google_SpreadsheetKey;
		public string Google_SpreadsheetName;
		public string Google_LastUpdatedVersion;

        #if UNITY_EDITOR
        public string Google_Password = "change_this";
        #endif

        public enum eGoogleUpdateFrequency { Always, Never, Daily, Weekly, Monthly, OnlyOnce }
		public eGoogleUpdateFrequency GoogleUpdateFrequency = eGoogleUpdateFrequency.Weekly;
		public eGoogleUpdateFrequency GoogleInEditorCheckFrequency = eGoogleUpdateFrequency.Daily;

		public float GoogleUpdateDelay = 5; // How many second to delay downloading data from google (to avoid lag on the startup)

		public event Action<LanguageSource, bool, string> Event_OnSourceUpdateFromGoogle;    // (LanguageSource, bool ReceivedNewData, string errorMsg)
		
		#endregion

		#region Connection to Web Service 

		public static void FreeUnusedLanguages()
		{
			var source    = LocalizationManager.Sources[0];
			int langIndex = source.GetLanguageIndex(LocalizationManager.CurrentLanguage);

			for (int i=0; i<source.mTerms.Count; ++i)
			{
				var term = source.mTerms[i];
				for (int j=0; j<term.Languages.Length; j++)
				{
					if (j != langIndex)
						term.Languages[j] = null;
				}
			}
		}

		public void Import_Google_FromCache()
		{
			if (GoogleUpdateFrequency==eGoogleUpdateFrequency.Never)
				return;
			
			if (!I2Utils.IsPlaying())
					return;
					
			string PlayerPrefName = GetSourcePlayerPrefName();
			string I2SavedData = PersistentStorage.LoadFile(PersistentStorage.eFileType.Persistent, "I2Source_"+ PlayerPrefName + ".loc", false);
			if (string.IsNullOrEmpty (I2SavedData))
				return;

            if (I2SavedData.StartsWith("[i2e]", StringComparison.Ordinal))
            {
                I2SavedData = StringObfucator.Decode(I2SavedData.Substring(5, I2SavedData.Length-5));
            }

			//--[ Compare with current version ]-----
			bool shouldUpdate = false;
			string savedSpreadsheetVersion = Google_LastUpdatedVersion;
			if (PersistentStorage.HasSetting("I2SourceVersion_"+PlayerPrefName))
			{
				savedSpreadsheetVersion = PersistentStorage.GetSetting_String("I2SourceVersion_"+PlayerPrefName, Google_LastUpdatedVersion);
//				Debug.Log (Google_LastUpdatedVersion + " - " + savedSpreadsheetVersion);
				shouldUpdate = IsNewerVersion(Google_LastUpdatedVersion, savedSpreadsheetVersion);
			}

			if (!shouldUpdate)
			{
                PersistentStorage.DeleteFile(PersistentStorage.eFileType.Persistent, "I2Source_"+PlayerPrefName+".loc", false);
                PersistentStorage.DeleteSetting("I2SourceVersion_"+PlayerPrefName);
				return;
			}

			if (savedSpreadsheetVersion.Length > 19) // Check for corruption from previous versions
				savedSpreadsheetVersion = string.Empty;
			Google_LastUpdatedVersion = savedSpreadsheetVersion;

			//Debug.Log ("[I2Loc] Using Saved (PlayerPref) data in 'I2Source_"+PlayerPrefName+"'" );
			Import_Google_Result(I2SavedData, eSpreadsheetUpdateMode.Replace);
		}

		bool IsNewerVersion( string currentVersion, string newVersion )
		{
			if (string.IsNullOrEmpty (newVersion))			// if no new version
				return false;
			if (string.IsNullOrEmpty (currentVersion))		// there is a new version, but not a current one
				return true;
			
			long currentV, newV;
			if (!long.TryParse (newVersion, out newV) || !long.TryParse (currentVersion, out currentV))	// if can't parse either, then force get the new one
				return true;

			return newV > currentV;
		}

        // When JustCheck is true, importing from google will not download any data, just detect if the Spreadsheet is up-to-date
		public void Import_Google( bool ForceUpdate, bool justCheck)
		{
            if (!ForceUpdate && GoogleUpdateFrequency==eGoogleUpdateFrequency.Never)
				return;

            if (!I2Utils.IsPlaying())
                return;

            #if UNITY_EDITOR
            if (justCheck && GoogleInEditorCheckFrequency==eGoogleUpdateFrequency.Never)
                return;
            #endif

            #if UNITY_EDITOR
                        var updateFrequency = GoogleInEditorCheckFrequency;
            #else
                        var updateFrequency = GoogleUpdateFrequency;
            #endif

            string PlayerPrefName = GetSourcePlayerPrefName();

            if (!ForceUpdate && updateFrequency != eGoogleUpdateFrequency.Always)
			{
                #if UNITY_EDITOR
                    string sTimeOfLastUpdate = UnityEditor.EditorPrefs.GetString("LastGoogleUpdate_"+PlayerPrefName, "");
                #else
                    string sTimeOfLastUpdate = PersistentStorage.GetSetting_String("LastGoogleUpdate_"+PlayerPrefName, "");
                #endif
				DateTime TimeOfLastUpdate;
				try
				{
					if (DateTime.TryParse( sTimeOfLastUpdate, out TimeOfLastUpdate ))
					{
						double TimeDifference = (DateTime.Now-TimeOfLastUpdate).TotalDays;
                        switch (updateFrequency)
						{
							case eGoogleUpdateFrequency.Daily: if (TimeDifference<1) return;
								break;
							case eGoogleUpdateFrequency.Weekly: if (TimeDifference<8) return;
								break;
							case eGoogleUpdateFrequency.Monthly: if (TimeDifference<31) return;
								break;
							case eGoogleUpdateFrequency.OnlyOnce: return;
						}
					}
				}
				catch(Exception)
				{ }
			}
            #if UNITY_EDITOR
                UnityEditor.EditorPrefs.SetString("LastGoogleUpdate_" + PlayerPrefName, DateTime.Now.ToString());
            #else
                PersistentStorage.SetSetting_String("LastGoogleUpdate_"+PlayerPrefName, DateTime.Now.ToString());
            #endif

			//--[ Checking google for updated data ]-----------------
			CoroutineManager.Start(Import_Google_Coroutine(justCheck));
		}

		string GetSourcePlayerPrefName()
		{
            string sourceName = name;
            if (!string.IsNullOrEmpty(Google_SpreadsheetKey))
            {
                sourceName += Google_SpreadsheetKey;
            }
            // If its a global source, use its name, otherwise, use the name and the level it is in
            if (Array.IndexOf(LocalizationManager.GlobalSources, name)>=0)
				return sourceName;
			else
			{
#if UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
				return Application.loadedLevelName + "_" + sourceName;
#else
                return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name+"_"+ sourceName;
#endif
			}
		}

		IEnumerator Import_Google_Coroutine(bool JustCheck)
		{
			WWW www = Import_Google_CreateWWWcall(false, JustCheck);
			if (www==null)
				yield break;

			while (!www.isDone)
				yield return null;

			//Debug.Log ("Google Result: " + www.text);
			bool notError = string.IsNullOrEmpty(www.error);
			string wwwText = null;

			if (notError)
			{
				var bytes = www.bytes;
				wwwText = System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length); //www.text

                bool isEmpty = string.IsNullOrEmpty(wwwText) || wwwText == "\"\"";

                if (JustCheck)
                {
                    if (!isEmpty)
                    {
                        Debug.LogWarning("Spreadsheet is not up-to-date and Google Live Synchronization is enabled\nWhen playing in the device the Spreadsheet will be downloaded and translations may not behave as what you see in the editor.\nTo fix this, Import or Export replace to Google");
                        GoogleLiveSyncIsUptoDate = false;
                    }

                    yield break;
                }

                if (!isEmpty)
                {
                    var errorMsg = Import_Google_Result(wwwText, eSpreadsheetUpdateMode.Replace, true);
                    if (string.IsNullOrEmpty(errorMsg))
                    {
                        if (Event_OnSourceUpdateFromGoogle != null)
                            Event_OnSourceUpdateFromGoogle(this, true, www.error);

                        LocalizationManager.LocalizeAll(true);
                        Debug.Log("Done Google Sync");
                    }
                    else
                    {
                        if (Event_OnSourceUpdateFromGoogle != null)
                            Event_OnSourceUpdateFromGoogle(this, false, www.error);

                        Debug.Log("Done Google Sync: source was up-to-date");
                    }
                    yield break;
                }
			}

			if (Event_OnSourceUpdateFromGoogle != null)
				Event_OnSourceUpdateFromGoogle(this, false, www.error);

			Debug.Log("Language Source was up-to-date with Google Spreadsheet");
		}

		public WWW Import_Google_CreateWWWcall( bool ForceUpdate, bool justCheck )
		{
			if (!HasGoogleSpreadsheet())
				return null;

			string savedVersion = PersistentStorage.GetSetting_String("I2SourceVersion_"+GetSourcePlayerPrefName(), Google_LastUpdatedVersion);
			if (savedVersion.Length > 19) // Check for corruption
				savedVersion= string.Empty;

#if !UNITY_EDITOR
            if (IsNewerVersion(savedVersion, Google_LastUpdatedVersion))
				Google_LastUpdatedVersion = savedVersion;
#endif

			string query =  string.Format("{0}?key={1}&action=GetLanguageSource&version={2}", 
										  LocalizationManager.GetWebServiceURL(this),
										  Google_SpreadsheetKey,
										  ForceUpdate ? "0" : Google_LastUpdatedVersion);
#if UNITY_EDITOR
            if (justCheck)
            {
                query += "&justcheck=true";
            }
#endif
            WWW www = new WWW(query);
			return www;
		}

		public bool HasGoogleSpreadsheet()
		{
            return !string.IsNullOrEmpty(Google_WebServiceURL) && !string.IsNullOrEmpty(Google_SpreadsheetKey) &&
                   !string.IsNullOrEmpty(LocalizationManager.GetWebServiceURL(this));
        }

		public string Import_Google_Result( string JsonString, eSpreadsheetUpdateMode UpdateMode, bool saveInPlayerPrefs = false )
		{
            try
            {
                string ErrorMsg = string.Empty;
                if (string.IsNullOrEmpty(JsonString) || JsonString == "\"\"")
                {
                    return ErrorMsg;
                }

                int idxV = JsonString.IndexOf("version=", StringComparison.Ordinal);
                int idxSV = JsonString.IndexOf("script_version=", StringComparison.Ordinal);
                if (idxV < 0 || idxSV < 0)
                {
                    return "Invalid Response from Google, Most likely the WebService needs to be updated";
                }

                idxV += "version=".Length;
                idxSV += "script_version=".Length;

                string newSpreadsheetVersion = JsonString.Substring(idxV, JsonString.IndexOf(",", idxV, StringComparison.Ordinal) - idxV);
                var scriptVersion = int.Parse(JsonString.Substring(idxSV, JsonString.IndexOf(",", idxSV, StringComparison.Ordinal) - idxSV));

                if (newSpreadsheetVersion.Length > 19) // Check for corruption
                    newSpreadsheetVersion = string.Empty;

                if (scriptVersion != LocalizationManager.GetRequiredWebServiceVersion())
                {
                    return "The current Google WebService is not supported.\nPlease, delete the WebService from the Google Drive and Install the latest version.";
                }

                //Debug.Log (Google_LastUpdatedVersion + " - " + newSpreadsheetVersion);
                if (saveInPlayerPrefs && !IsNewerVersion(Google_LastUpdatedVersion, newSpreadsheetVersion))
#if UNITY_EDITOR
                    return "";
#else
				return "LanguageSource is up-to-date";
#endif

                if (saveInPlayerPrefs)
                {
                    string PlayerPrefName = GetSourcePlayerPrefName();
                    PersistentStorage.SaveFile(PersistentStorage.eFileType.Persistent, "I2Source_" + PlayerPrefName, "[i2e]" + StringObfucator.Encode(JsonString) + ".loc");
                    PersistentStorage.SetSetting_String("I2SourceVersion_" + PlayerPrefName, newSpreadsheetVersion);
                    PersistentStorage.ForceSaveSettings();
                }
                Google_LastUpdatedVersion = newSpreadsheetVersion;

                if (UpdateMode == eSpreadsheetUpdateMode.Replace)
                    ClearAllData();

                int CSVstartIdx = JsonString.IndexOf("[i2category]", StringComparison.Ordinal);
                while (CSVstartIdx > 0)
                {
                    CSVstartIdx += "[i2category]".Length;
                    int endCat = JsonString.IndexOf("[/i2category]", CSVstartIdx, StringComparison.Ordinal);
                    string category = JsonString.Substring(CSVstartIdx, endCat - CSVstartIdx);
                    endCat += "[/i2category]".Length;

                    int endCSV = JsonString.IndexOf("[/i2csv]", endCat, StringComparison.Ordinal);
                    string csv = JsonString.Substring(endCat, endCSV - endCat);

                    CSVstartIdx = JsonString.IndexOf("[i2category]", endCSV, StringComparison.Ordinal);

                    Import_I2CSV(category, csv, UpdateMode);

                    // Only the first CSV should clear the Data
                    if (UpdateMode == eSpreadsheetUpdateMode.Replace)
                        UpdateMode = eSpreadsheetUpdateMode.Merge;
                }

                GoogleLiveSyncIsUptoDate = true;
                if (I2Utils.IsPlaying())
                {
                    SaveLanguages(true);
                }

#if UNITY_EDITOR
                if (!string.IsNullOrEmpty(ErrorMsg))
                    UnityEditor.EditorUtility.SetDirty(this);
#endif
                return ErrorMsg;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning(e);
                return e.ToString();
            }
		}

#endregion
	}
}