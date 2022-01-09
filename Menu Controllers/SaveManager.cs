/*
Copyright(c) 2021 Chicho Studio

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files
(the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge,
publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO
THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
DEALINGS IN THE SOFTWARE.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System;
using System.Reflection;
using UnityEngine.Rendering.Universal;
using ShadowQuality = UnityEngine.ShadowQuality;
using ShadowResolution = UnityEngine.Rendering.Universal.ShadowResolution;

namespace SettingsSystem.DataStorage
{
    public enum DocType
    {
        PlayerData,
        GeneralData,
        CustomizationData,
    }

    public static class SaveManager
    {
        private static PLayerData pLayerData = new();
        private static GeneralData generalData = new();
        private static CustomizationData customizationData = new();

        private static readonly string myFileNameExtension = ".xml";

        public static PLayerData PLayerData { get => pLayerData ?? new(); set => pLayerData = value; }
        public static GeneralData GeneralData { get => generalData ?? new(); set => generalData = value; }
        public static CustomizationData CustomizationData { get => customizationData ?? new(); set => customizationData = value; }

        public static void Save<T>(T dataToSave, string fileName)
        {
            string dataPath = Application.persistentDataPath;

            var serializer = new XmlSerializer(typeof(T));
            var stream = new FileStream(dataPath + "/" + fileName + myFileNameExtension, FileMode.Create);
            serializer.Serialize(stream, dataToSave);
            stream.Close();

            Debug.Log("Saved");

        }

        public static void Save<T>(T dataToSave, DocType fileType)
        {
            string dataPath = Application.persistentDataPath;

            var serializer = new XmlSerializer(typeof(T));
            var stream = new FileStream(dataPath + "/" + GetFileName(fileType) + myFileNameExtension, FileMode.Create);
            serializer.Serialize(stream, dataToSave);
            stream.Close();

            Debug.Log("Saved");

        }

        public static void Load<T>(out T dataToLoad, string fileName) where T : class, new()
        {
            string dataPath = Application.persistentDataPath;

            if (System.IO.File.Exists(dataPath + "/" + fileName + myFileNameExtension))
            {
                var serializer = new XmlSerializer(typeof(T));
                var stream = new FileStream(dataPath + "/" + fileName + myFileNameExtension, FileMode.Open);
                dataToLoad = serializer.Deserialize(stream) as T;
                stream.Close();

                Debug.Log("Loaded");
            }
            else
            {
                dataToLoad = new T();
            }
        }

        public static void Load<T>(out T dataToLoad, DocType fileType) where T : class, new()
        {
            string dataPath = Application.persistentDataPath;

            if (System.IO.File.Exists(dataPath + "/" + GetFileName(fileType) + myFileNameExtension))
            {
                var serializer = new XmlSerializer(typeof(T));
                var stream = new FileStream(dataPath + "/" + GetFileName(fileType) + myFileNameExtension, FileMode.Open);
                dataToLoad = serializer.Deserialize(stream) as T;
                stream.Close();

                Debug.Log("Loaded");
            }
            else
            {
                dataToLoad = new T();
            }
        }

        public static T Load<T>(string fileName) where T : class, new()
        {
            string dataPath = Application.persistentDataPath;

            if (System.IO.File.Exists(dataPath + "/" + fileName + myFileNameExtension))
            {
                T data;

                var serializer = new XmlSerializer(typeof(T));
                var stream = new FileStream(dataPath + "/" + fileName + myFileNameExtension, FileMode.Open);
                data = serializer.Deserialize(stream) as T;
                stream.Close();

                Debug.Log("Loaded");

                return data;
            }
            else
            {
                return new T();
            }
        }

        public static T Load<T>(DocType fileType) where T : class, new()
        {
            string dataPath = Application.persistentDataPath;

            if (System.IO.File.Exists(dataPath + "/" + GetFileName(fileType) + myFileNameExtension))
            {
                T data;

                var serializer = new XmlSerializer(typeof(T));
                var stream = new FileStream(dataPath + "/" + GetFileName(fileType) + myFileNameExtension, FileMode.Open);
                data = serializer.Deserialize(stream) as T;
                stream.Close();

                Debug.Log("Loaded");

                return data;
            }
            else
            {
                return new T();
            }
        }

        public static void DeleteSavedData<T>(string fileName) where T : class
        {
            string dataPath = Application.persistentDataPath;

            if (System.IO.File.Exists(dataPath + "/" + fileName + myFileNameExtension))
            {
                File.Delete(dataPath + "/" + fileName + myFileNameExtension);

                Debug.Log("Deleted data");
            }
        }

        public static void DeleteSavedData<T>(DocType fileType) where T : class
        {
            string dataPath = Application.persistentDataPath;

            if (System.IO.File.Exists(dataPath + "/" + GetFileName(fileType) + myFileNameExtension))
            {
                File.Delete(dataPath + "/" + GetFileName(fileType) + myFileNameExtension);

                Debug.Log("Deleted data");
            }
        }

        public static void LoadAndSave<T>(ref T dataToLoadAndSave, DocType docType, Action<T> codeBetween = null) where T : class, new()
        {
            dataToLoadAndSave = Load<T>(docType);

            codeBetween?.Invoke(dataToLoadAndSave);

            Save(dataToLoadAndSave, docType);
        }

        public static T SaveAndLoad<T>(T dataToLoadAndSave, DocType docType, Action codeBetween = null) where T : class, new()
        {
            Save(dataToLoadAndSave, docType);

            codeBetween?.Invoke();

            return Load<T>(docType);
        }


        public static T GetFieldValue<T>(object obj, string fieldName)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            var field = obj.GetType().GetField(fieldName, BindingFlags.Public |
                                                          BindingFlags.NonPublic |
                                                          BindingFlags.Instance);

            if (field == null)
                throw new ArgumentException("fieldName", "No such field was found.");

            if (!typeof(T).IsAssignableFrom(field.FieldType))
                throw new InvalidOperationException("Field type and requested type are not compatible.");

            return (T)field.GetValue(obj);
        }

        private static string GetFileName(DocType docType)
        {
            return docType switch
            {
                DocType.PlayerData => nameof(PLayerData),
                DocType.GeneralData => nameof(GeneralData),
                DocType.CustomizationData => nameof(CustomizationData),
                _ => nameof(GeneralData)
            };
        }
    }

    [System.Serializable]
    public sealed class GeneralData
    {

    }

    [System.Serializable]
    public sealed class PLayerData
    {

    }

    [System.Serializable]
    public sealed class CustomizationData
    {
        // Video Settings
        public Vector2Int screenResolution = new(0, 0);
        public FullScreenMode screenMode = FullScreenMode.ExclusiveFullScreen;
        public int frameRate = -1;
        public byte vSyncCount = 1;

        //Audio Settings
        public float rawMasterVolume = 1f;
        public float rawSFXVolume = 1f;
        public float rawMusicVolume = 1f;
        public bool enableTextSpeechSound = true;

        //General Settings
        public float cameraShake = 1f;
        public float fieldOfView = 90f;
        public float sensitivity = 1f;
        public bool visualTips = true;

        //Graphics Settings
        public byte qualityLevel = 2;
        public bool postProcessing = true;
        public ShadowQuality shadowQuality = ShadowQuality.All;
        public ShadowResolution shadowResolution = ShadowResolution._2048;
        public byte shadowCascadesCount = 4;
        public float shadowDistance = 100f;
        public MsaaQuality msaaQuality = MsaaQuality._2x;
        public float renderScale = 1f;
    }
}