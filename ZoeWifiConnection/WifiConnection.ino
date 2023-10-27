void InitWifi()
{
  if (ClientWasConnectedOnce)
    return;

  String foundAp;

  if (IsWaitingTimeOver(LastWiFiScan, WifiScanInterval))
    foundAp = ScanForAP();
  else
    return;

  bool apAvaliable = foundAp.length() > 0;
  int tries = 0;
  int maxTries = 20;

  if (apAvaliable)
  {
    Log("[WiFi] mode: STA");
    WiFi.mode(WIFI_STA);
    WiFi.disconnect();
    WiFi.begin(foundAp, STAPSK);

    while (WiFi.status() != WL_CONNECTED) {
      if (tries > maxTries)
        break;

      delay(500);
      tries++;
    }

    WiFiInit = false;

    if (WiFi.status() == WL_CONNECTED)
      SoftAPActive = false;
  }

  if (!SoftAPActive && !apAvaliable)
  {
    Log("[WiFi] ExternalAp not avaliable, changing to SoftAPMode");
    WiFi.mode(WIFI_AP);
    WiFi.softAPConfig(myLocalIp, myLocalIp, myLocalSubnet);
    WiFi.softAP(STASSID, STAPSK);

    SoftAPActive = true;
  }

  Udp.begin(localPort);
}

String ScanForAP()
{
  int n = WiFi.scanNetworks();
  Log("[WiFi] scan done");
  if (n == 0) {
    Log("no networks found");
  }
  else
  {
    Log((String)n + " networks found");
    for (int i = 0; i < n; ++i) {
      Log(WiFi.SSID(i));

      if (((String)WiFi.SSID(i)).indexOf(STASSID_CONTAINMENT) > -1)
      {
        Log("Connecting to:" + (String)WiFi.SSID(i));
        return (String)WiFi.SSID(i);
      }
    }
  }

  LastWiFiScan = millis();
  return "";
}
