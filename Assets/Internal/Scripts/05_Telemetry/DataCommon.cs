using System;
using UnityEngine;

[Serializable]
public class DataCommon
{
   public string deviceId;
   public string deviceName;
   public int deviceType;
   public bool isOnline;
   
   //cctv
   public string StreamUrl;
   public int ResolutionW;
   public int ResolutionH;
   //temp sensor
   public float Temperature;
   public float Humidity;
}
